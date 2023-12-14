using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Core.Services.TrsDataSync;

public class TrsDataSyncHelper(
    IDbContextFactory<TrsDbContext> dbContextFactory,
    [FromKeyedServices(TrsDataSyncService.CrmClientName)] IOrganizationServiceAsync2 organizationService,
    ReferenceDataCache referenceDataCache,
    IClock clock)
{
    private delegate Task SyncEntitiesHandler(IReadOnlyCollection<Entity> entities, bool ignoreInvalid, CancellationToken cancellationToken);

    private const string NowParameterName = "@now";
    private const string IdsParameterName = "@ids";

    private static readonly IReadOnlyDictionary<string, ModelTypeSyncInfo> _modelTypeSyncInfo = new Dictionary<string, ModelTypeSyncInfo>()
    {
        { ModelTypes.Person, GetModelTypeSyncInfoForPerson() },
        { ModelTypes.MandatoryQualification, GetModelTypeSyncInfoForMandatoryQualification() },
    };

    public static (string EntityLogicalName, string[] AttributeNames) GetEntityInfoForModelType(string modelType)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);
        return (modelTypeSyncInfo.EntityLogicalName, modelTypeSyncInfo.AttributeNames);
    }

    public async Task DeleteRecords(string modelType, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return;
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = modelTypeSyncInfo.DeleteStatement;
            cmd.Parameters.Add(new NpgsqlParameter(IdsParameterName, ids.ToArray()));
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task<DateTime?> GetLastModifiedOnForModelType(string modelType)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync();

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = modelTypeSyncInfo.GetLastModifiedOnStatement;
            await using var reader = await cmd.ExecuteReaderAsync();
            var read = reader.Read();
            Debug.Assert(read);
            return reader.IsDBNull(0) ? null : reader.GetDateTime(0);
        }
    }

    public Task SyncRecords(string modelType, IReadOnlyCollection<Entity> entities, bool ignoreInvalid, CancellationToken cancellationToken = default)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);
        return modelTypeSyncInfo.GetSyncHandler(this)(entities, ignoreInvalid, cancellationToken);
    }

    public async Task<bool> SyncPerson(Guid contactId, CancellationToken cancellationToken)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(ModelTypes.Person);
        var contacts = await GetEntities<Contact>(Contact.EntityLogicalName, Contact.PrimaryIdAttribute, [contactId], modelTypeSyncInfo.AttributeNames, cancellationToken);
        return await SyncPersons(contacts, ignoreInvalid: false, cancellationToken) == 1;
    }

    public async Task<bool> SyncPerson(Contact entity, bool ignoreInvalid, CancellationToken cancellationToken = default) =>
        await SyncPersons(new[] { entity }, ignoreInvalid, cancellationToken) == 1;

    public async Task<int> SyncPersons(IReadOnlyCollection<Contact> entities, bool ignoreInvalid, CancellationToken cancellationToken = default)
    {
        // For now, only sync records that have TRNs.
        // Keep this in sync with the filter in the SyncAllContactsFromCrmJob job.
        var toSync = entities.Where(e => !string.IsNullOrEmpty(e.dfeta_TRN));

        if (ignoreInvalid)
        {
            // Some bad data in the build environment has a TRN that's longer than 7 digits
            toSync = toSync.Where(e => e.dfeta_TRN.Length == 7);
        }

        var people = MapPersons(toSync);

        if (people.Count == 0)
        {
            return 0;
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo<Person>(ModelTypes.Person);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        using var txn = await connection.BeginTransactionAsync(cancellationToken);

        using (var createTempTableCommand = connection.CreateCommand())
        {
            createTempTableCommand.CommandText = modelTypeSyncInfo.CreateTempTableStatement;
            createTempTableCommand.Transaction = txn;
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        using var writer = await connection.BeginBinaryImportAsync(modelTypeSyncInfo.CopyStatement, cancellationToken);

        foreach (var person in people)
        {
            writer.StartRow();
            modelTypeSyncInfo.WriteRecord(writer, person);
        }

        await writer.CompleteAsync(cancellationToken);
        await writer.CloseAsync(cancellationToken);

        try
        {
            using (var mergeCommand = connection.CreateCommand())
            {
                mergeCommand.CommandText = modelTypeSyncInfo.InsertStatement;
                mergeCommand.Parameters.Add(new NpgsqlParameter(NowParameterName, clock.UtcNow));
                mergeCommand.Transaction = txn;
                await mergeCommand.ExecuteNonQueryAsync();
            }
        }
        catch (PostgresException ex) when (ignoreInvalid && ex.SqlState == "23505" && ex.ConstraintName == "ix_persons_trn")
        {
            // Record already exists with TRN.
            // Remove the record from the collection and try again.

            // ex.Detail will be something like "Key (trn)=(1000336) already exists."
            var trn = ex.Detail!.Substring("Key (trn)=(".Length, 7);

            if (trn.Length != 7 || !trn.All(char.IsAsciiDigit))
            {
                Debug.Fail("Failed parsing TRN from exception message.");
                throw;
            }

            var entitiesExceptFailedOne = entities.Where(e => e.dfeta_TRN != trn).ToArray();

            // Be extra sure we've actually removed a record (otherwise we'll go in an endless loop and stack overflow)
            if (!(entitiesExceptFailedOne.Length < entities.Count))
            {
                Debug.Fail("No entities removed from collection.");
                throw;
            }

            await txn.DisposeAsync();
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();

            return await SyncPersons(entitiesExceptFailedOne, ignoreInvalid, cancellationToken);
        }

        await txn.CommitAsync(cancellationToken);

        return people.Count;
    }

    public async Task<bool> SyncMandatoryQualification(dfeta_qualification entity, bool ignoreInvalid, CancellationToken cancellationToken = default) =>
        await SyncMandatoryQualifications(new[] { entity }, ignoreInvalid, cancellationToken) == 1;

    public async Task<int> SyncMandatoryQualifications(
        IReadOnlyCollection<dfeta_qualification> entities,
        bool ignoreInvalid,
        CancellationToken cancellationToken = default)
    {
        // Not all dfeta_qualification records are MQs..
        var toSync = entities.Where(q => q.dfeta_Type == dfeta_qualification_dfeta_Type.MandatoryQualification);

        var (mqs, events) = await MapMandatoryQualifications(toSync);

        if (mqs.Count == 0)
        {
            Debug.Assert(events.Count == 0);
            return 0;
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo<MandatoryQualification>(ModelTypes.MandatoryQualification);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        do
        {
            try
            {
                using var txn = await connection.BeginTransactionAsync(cancellationToken);

                using (var createTempTableCommand = connection.CreateCommand())
                {
                    createTempTableCommand.CommandText = modelTypeSyncInfo.CreateTempTableStatement;
                    createTempTableCommand.Transaction = txn;
                    await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                using var writer = await connection.BeginBinaryImportAsync(modelTypeSyncInfo.CopyStatement, cancellationToken);

                foreach (var mq in mqs)
                {
                    writer.StartRow();
                    modelTypeSyncInfo.WriteRecord(writer, mq);
                }

                await writer.CompleteAsync(cancellationToken);
                await writer.CloseAsync(cancellationToken);

                using (var mergeCommand = connection.CreateCommand())
                {
                    mergeCommand.CommandText = modelTypeSyncInfo.InsertStatement;
                    mergeCommand.Parameters.Add(new NpgsqlParameter(NowParameterName, clock.UtcNow));
                    mergeCommand.Transaction = txn;
                    await mergeCommand.ExecuteNonQueryAsync();
                }

                await txn.CommitAsync(cancellationToken);
            }
            catch (PostgresException ex) when (ex.SqlState == "23503" && ex.ConstraintName == Qualification.PersonForeignKeyName)
            {
                // Person does not exist.
                // This is unlikely to happen often so the handling of this doesn't need to be super optimal.
                // In this case we just pull the contact record directly, sync it then go again.

                // ex.Detail will be something like "Key (person_id)=(6ac8dc26-c8ae-e311-b8ed-005056822391) is not present in table "persons"."
                var personId = Guid.Parse(ex.Detail!.Substring("Key (person_id)=(".Length, Guid.Empty.ToString().Length));

                var personSynced = await SyncPerson(personId, cancellationToken);
                if (!personSynced)
                {
                    // The person sync may fail if the record doesn't meet the criteria (e.g. it doesn't have a TRN).
                    // If we're ignoring invalid records we can skip MQs that reference this person ID,
                    // otherwise we blow up.

                    if (ignoreInvalid)
                    {
                        mqs.RemoveAll(mq => mq.PersonId == personId);
                        if (mqs.Count == 0)
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Attempted to sync MandatoryQualification for person '{personId}' but the person record does not meet the sync criteria.");
                    }
                }

                continue;
            }

            break;
        }
        while (true);

        await SyncEvents(events, cancellationToken);

        return mqs.Count;
    }

    private static ModelTypeSyncInfo<TModel> GetModelTypeSyncInfo<TModel>(string modelType) =>
        (ModelTypeSyncInfo<TModel>)GetModelTypeSyncInfo(modelType);

    private static ModelTypeSyncInfo GetModelTypeSyncInfo(string modelType) =>
        _modelTypeSyncInfo.TryGetValue(modelType, out var modelTypeSyncInfo) ?
            modelTypeSyncInfo :
            throw new ArgumentException($"Unknown data type: '{modelType}.", nameof(modelType));

    private async Task<TEntity[]> GetEntities<TEntity>(
        string entityLogicalName,
        string idAttributeName,
        Guid[] ids,
        string[] attributeNames,
        CancellationToken cancellationToken)
        where TEntity : Entity
    {
        var query = new QueryExpression(entityLogicalName);
        query.ColumnSet = new(attributeNames);
        query.Criteria.AddCondition(idAttributeName, ConditionOperator.In, ids.Cast<object>().ToArray());

        var response = await organizationService.RetrieveMultipleAsync(query, cancellationToken);
        return response.Entities.Select(e => e.ToEntity<TEntity>()).ToArray();
    }

    private static ModelTypeSyncInfo GetModelTypeSyncInfoForPerson()
    {
        var tempTableName = "temp_person_import";
        var tableName = "persons";

        var columnNames = new[]
        {
            "person_id",
            "trn",
            "first_name",
            "middle_name",
            "last_name",
            "date_of_birth",
            "email_address",
            "national_insurance_number",
            "dqt_contact_id",
            "dqt_state",
            "dqt_created_on",
            "dqt_modified_on",
            "dqt_first_name",
            "dqt_middle_name",
            "dqt_last_name"
        };

        var columnsToUpdate = columnNames.Except(new[] { "person_id", "dqt_contact_id" }).ToArray();

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement = $"CREATE TEMP TABLE {tempTableName} (LIKE {tableName} INCLUDING DEFAULTS)";

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var insertStatement =
            $"""
            INSERT INTO {tableName} ({columnList}, dqt_first_sync, dqt_last_sync)
            SELECT {columnList}, {NowParameterName}, {NowParameterName} FROM {tempTableName}
            ON CONFLICT (person_id) DO UPDATE SET dqt_last_sync = {NowParameterName}, {string.Join(", ", columnsToUpdate.Select(c => $"{c} = EXCLUDED.{c}"))}
            """;

        var deleteStatement = $"DELETE FROM {tableName} WHERE dqt_contact_id = ANY({IdsParameterName})";

        var getLastModifiedOnStatement = $"SELECT MAX(dqt_modified_on) FROM {tableName}";

        var attributeNames = new[]
        {
            Contact.Fields.ContactId,
            Contact.Fields.StateCode,
            Contact.Fields.CreatedOn,
            Contact.Fields.ModifiedOn,
            Contact.Fields.dfeta_TRN,
            Contact.Fields.FirstName,
            Contact.Fields.MiddleName,
            Contact.Fields.LastName,
            Contact.Fields.dfeta_StatedFirstName,
            Contact.Fields.dfeta_StatedMiddleName,
            Contact.Fields.dfeta_StatedLastName,
            Contact.Fields.BirthDate,
            Contact.Fields.dfeta_NINumber,
            Contact.Fields.EMailAddress1
        };

        Action<NpgsqlBinaryImporter, Person> writeRecord = (writer, person) =>
        {
            writer.WriteValueOrNull(person.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(person.Trn, NpgsqlDbType.Char);
            writer.WriteValueOrNull(person.FirstName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.MiddleName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.LastName, NpgsqlDbType.Varchar);
            writer.WriteNullableValueOrNull(person.DateOfBirth, NpgsqlDbType.Date);
            writer.WriteValueOrNull(person.EmailAddress, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.NationalInsuranceNumber, NpgsqlDbType.Char);
            writer.WriteNullableValueOrNull(person.DqtContactId, NpgsqlDbType.Uuid);
            writer.WriteNullableValueOrNull(person.DqtState, NpgsqlDbType.Integer);
            writer.WriteNullableValueOrNull(person.DqtCreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteNullableValueOrNull(person.DqtModifiedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(person.DqtFirstName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.DqtMiddleName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.DqtLastName, NpgsqlDbType.Varchar);
        };

        return new ModelTypeSyncInfo<Person>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            InsertStatement = insertStatement,
            DeleteStatement = deleteStatement,
            GetLastModifiedOnStatement = getLastModifiedOnStatement,
            EntityLogicalName = Contact.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, ct) => helper.SyncPersons(entities.Select(e => e.ToEntity<Contact>()).ToArray(), ignoreInvalid, ct),
            WriteRecord = writeRecord
        };
    }

    private static ModelTypeSyncInfo GetModelTypeSyncInfoForMandatoryQualification()
    {
        var tempTableName = "temp_mqs_import";
        var tableName = "qualifications";

        var columnNames = new[]
        {
            "qualification_id",
            "created_on",
            "updated_on",
            "deleted_on",
            "qualification_type",
            "person_id",
            "dqt_qualification_id",
            "dqt_state",
            "dqt_created_on",
            "dqt_modified_on",
            "mq_specialism",
            "mq_status",
            "start_date",
            "end_date",
            "dqt_mq_establishment_id",
            "dqt_specialism_id"
        };

        var columnsToUpdate = columnNames.Except(new[] { "qualification_id", "dqt_qualification_id" }).ToArray();

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement = $"CREATE TEMP TABLE {tempTableName} (LIKE {tableName} INCLUDING DEFAULTS)";

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var insertStatement =
            $"""
            INSERT INTO {tableName} ({columnList}, dqt_first_sync, dqt_last_sync)
            SELECT {columnList}, {NowParameterName}, {NowParameterName} FROM {tempTableName}
            ON CONFLICT (qualification_id) DO UPDATE SET dqt_last_sync = {NowParameterName}, {string.Join(", ", columnsToUpdate.Select(c => $"{c} = EXCLUDED.{c}"))}
            """;

        var deleteStatement = $"DELETE FROM {tableName} WHERE dqt_qualification_id = ANY({IdsParameterName})";

        var getLastModifiedOnStatement = $"SELECT MAX(dqt_modified_on) FROM {tableName}";

        var attributeNames = new[]
        {
            dfeta_qualification.Fields.dfeta_qualificationId,
            dfeta_qualification.Fields.CreatedOn,
            dfeta_qualification.Fields.ModifiedOn,
            dfeta_qualification.Fields.dfeta_TrsDeletedEvent,
            dfeta_qualification.Fields.dfeta_Type,
            dfeta_qualification.Fields.dfeta_PersonId,
            dfeta_qualification.Fields.StateCode,
            dfeta_qualification.Fields.dfeta_MQ_SpecialismId,
            dfeta_qualification.Fields.dfeta_MQ_Status,
            dfeta_qualification.Fields.dfeta_MQStartDate,
            dfeta_qualification.Fields.dfeta_MQ_Date,
            dfeta_qualification.Fields.dfeta_MQ_MQEstablishmentId,
        };

        Action<NpgsqlBinaryImporter, MandatoryQualification> writeRecord = (writer, mq) =>
        {
            writer.WriteValueOrNull(mq.QualificationId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(mq.CreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(mq.UpdatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteNullableValueOrNull(mq.DeletedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull((int)QualificationType.MandatoryQualification, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(mq.PersonId, NpgsqlDbType.Uuid);
            writer.WriteNullableValueOrNull(mq.DqtQualificationId, NpgsqlDbType.Uuid);
            writer.WriteNullableValueOrNull(mq.DqtState, NpgsqlDbType.Integer);
            writer.WriteNullableValueOrNull(mq.DqtCreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteNullableValueOrNull(mq.DqtModifiedOn, NpgsqlDbType.TimestampTz);
            writer.WriteNullableValueOrNull((int?)mq.Specialism, NpgsqlDbType.Integer);
            writer.WriteNullableValueOrNull((int?)mq.Status, NpgsqlDbType.Integer);
            writer.WriteNullableValueOrNull(mq.StartDate, NpgsqlDbType.Date);
            writer.WriteNullableValueOrNull(mq.EndDate, NpgsqlDbType.Date);
            writer.WriteNullableValueOrNull(mq.DqtMqEstablishmentId, NpgsqlDbType.Uuid);
            writer.WriteNullableValueOrNull(mq.DqtSpecialismId, NpgsqlDbType.Uuid);
        };

        return new ModelTypeSyncInfo<MandatoryQualification>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            InsertStatement = insertStatement,
            DeleteStatement = deleteStatement,
            GetLastModifiedOnStatement = getLastModifiedOnStatement,
            EntityLogicalName = dfeta_qualification.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, ct) => helper.SyncMandatoryQualifications(entities.Select(e => e.ToEntity<dfeta_qualification>()).ToArray(), ignoreInvalid, ct),
            WriteRecord = writeRecord
        };
    }

    private static List<Person> MapPersons(IEnumerable<Contact> contacts) => contacts
        .Select(c => new Person()
        {
            PersonId = c.ContactId!.Value,
            Trn = c.dfeta_TRN,
            FirstName = (c.HasStatedNames() ? c.dfeta_StatedFirstName : c.FirstName) ?? string.Empty,
            MiddleName = (c.HasStatedNames() ? c.dfeta_StatedMiddleName : c.MiddleName) ?? string.Empty,
            LastName = (c.HasStatedNames() ? c.dfeta_StatedLastName : c.LastName) ?? string.Empty,
            DateOfBirth = c.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            EmailAddress = c.EMailAddress1.NormalizeString(),
            NationalInsuranceNumber = c.dfeta_NINumber.NormalizeString(),
            DqtContactId = c.Id,
            DqtState = (int)c.StateCode!,
            DqtCreatedOn = c.CreatedOn!.Value,
            DqtModifiedOn = c.ModifiedOn!.Value,
            DqtFirstName = c.FirstName ?? string.Empty,
            DqtMiddleName = c.MiddleName ?? string.Empty,
            DqtLastName = c.LastName ?? string.Empty
        })
        .ToList();

    private async Task<(List<MandatoryQualification> MandatoryQualifications, List<EventBase> Events)> MapMandatoryQualifications(
        IEnumerable<dfeta_qualification> qualifications)
    {
        var mqSpecialisms = await referenceDataCache.GetMqSpecialisms();

        var mqs = new List<MandatoryQualification>();
        var events = new List<EventBase>();

        foreach (var q in qualifications)
        {
            var deletedEvent = q.dfeta_TrsDeletedEvent is not null and not "{}" ?
                EventInfo.Deserialize<MandatoryQualificationDeletedEvent>(q.dfeta_TrsDeletedEvent).Event :
                null;

            if (deletedEvent is not null)
            {
                events.Add(deletedEvent);
            }

            MandatoryQualificationSpecialism? specialism = q.dfeta_MQ_SpecialismId is not null ?
                mqSpecialisms.Single(s => s.Id == q.dfeta_MQ_SpecialismId.Id).ToMandatoryQualificationSpecialism() :
                null;

            MandatoryQualificationStatus? status = q.dfeta_MQ_Status?.ToMandatoryQualificationStatus() ??
                (q.dfeta_MQ_Date.HasValue ? MandatoryQualificationStatus.Passed : null);

            mqs.Add(new MandatoryQualification()
            {
                QualificationId = q.Id,
                CreatedOn = q.CreatedOn!.Value,
                UpdatedOn = q.ModifiedOn!.Value,
                DeletedOn = deletedEvent?.CreatedUtc,
                PersonId = q.dfeta_PersonId.Id,
                DqtQualificationId = q.Id,
                DqtState = (int)q.StateCode!,
                DqtCreatedOn = q.CreatedOn!.Value,
                DqtModifiedOn = q.ModifiedOn!.Value,
                Specialism = specialism,
                Status = status,
                StartDate = q.dfeta_MQStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                EndDate = q.dfeta_MQ_Date.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                DqtMqEstablishmentId = q.dfeta_MQ_MQEstablishmentId?.Id,
                DqtSpecialismId = q.dfeta_MQ_SpecialismId?.Id
            });
        }

        return (mqs, events);
    }

    private async Task<int> SyncEvents(IReadOnlyCollection<EventBase> events, CancellationToken cancellationToken)
    {
        var tempTableName = "temp_events_import";
        var tableName = "events";

        var columnNames = new[]
        {
            "event_id",
            "event_name",
            "created",
            "payload"
        };

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement = $"""
            CREATE TEMP TABLE {tempTableName} (
                event_id UUID NOT NULL,
                event_name VARCHAR(200) NOT NULL,
                created TIMESTAMP WITH TIME ZONE NOT NULL,
                payload JSONB NOT NULL
            )
            """;

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var insertStatement =
            $"""
            INSERT INTO {tableName} ({columnList}, published)
            SELECT {columnList}, false FROM {tempTableName}
            ON CONFLICT (event_id) DO NOTHING
            """;

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        using var txn = await connection.BeginTransactionAsync(cancellationToken);

        using (var createTempTableCommand = connection.CreateCommand())
        {
            createTempTableCommand.CommandText = createTempTableStatement;
            createTempTableCommand.Transaction = txn;
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        using var writer = await connection.BeginBinaryImportAsync(copyStatement, cancellationToken);

        foreach (var e in events)
        {
            var payload = JsonSerializer.Serialize(e, e.GetType(), EventBase.JsonSerializerOptions);

            writer.StartRow();
            writer.WriteValueOrNull(e.EventId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(e.GetEventName(), NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(e.CreatedUtc, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(payload, NpgsqlDbType.Jsonb);
        }

        await writer.CompleteAsync(cancellationToken);
        await writer.CloseAsync(cancellationToken);

        using (var mergeCommand = connection.CreateCommand())
        {
            mergeCommand.CommandText = insertStatement;
            mergeCommand.Parameters.Add(new NpgsqlParameter(NowParameterName, clock.UtcNow));
            mergeCommand.Transaction = txn;
            await mergeCommand.ExecuteNonQueryAsync();
        }

        await txn.CommitAsync(cancellationToken);

        return events.Count;
    }

    private record ModelTypeSyncInfo
    {
        public required string CreateTempTableStatement { get; init; }
        public required string CopyStatement { get; init; }
        public required string InsertStatement { get; init; }
        public required string DeleteStatement { get; init; }
        public required string GetLastModifiedOnStatement { get; init; }
        public required string EntityLogicalName { get; init; }
        public required string[] AttributeNames { get; init; }
        public required Func<TrsDataSyncHelper, SyncEntitiesHandler> GetSyncHandler { get; init; }
    }

    private record ModelTypeSyncInfo<TModel> : ModelTypeSyncInfo
    {
        public required Action<NpgsqlBinaryImporter, TModel> WriteRecord { get; init; }
    }

    public static class ModelTypes
    {
        public const string Person = "Person";
        public const string MandatoryQualification = "MandatoryQualification";
    }
}

file static class Extensions
{
    /// <summary>
    /// Returns <c>null</c> if <paramref name="value"/> is empty or whitespace.
    /// </summary>
    public static string? NormalizeString(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    public static void WriteNullableValueOrNull<T>(this NpgsqlBinaryImporter writer, T? value, NpgsqlDbType dbType)
        where T : struct
    {
        if (value.HasValue)
        {
            writer.Write(value.Value, dbType);
        }
        else
        {
            writer.WriteNull();
        }
    }

    public static void WriteValueOrNull<T>(this NpgsqlBinaryImporter writer, T? value, NpgsqlDbType dbType)
        where T : notnull
    {
        if (value is null)
        {
            writer.WriteNull();
        }
        else
        {
            writer.Write(value, dbType);
        }
    }
}

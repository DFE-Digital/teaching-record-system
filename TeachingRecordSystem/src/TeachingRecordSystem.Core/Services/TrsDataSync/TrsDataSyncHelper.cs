using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;

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
    private const int MaxAuditRequestsPerBatch = 10;

    private static readonly IReadOnlyDictionary<string, ModelTypeSyncInfo> _modelTypeSyncInfo = new Dictionary<string, ModelTypeSyncInfo>()
    {
        { ModelTypes.Person, GetModelTypeSyncInfoForPerson() },
        { ModelTypes.MandatoryQualification, GetModelTypeSyncInfoForMandatoryQualification() },
    };

    private readonly ISubject<object[]> _syncedEntitiesSubject = new Subject<object[]>();

    public IObservable<object[]> GetSyncedEntitiesObservable() => _syncedEntitiesSubject;

    private bool IsFakeXrm { get; } = organizationService.GetType().FullName == "Castle.Proxies.ObjectProxy_2";

    public static (string EntityLogicalName, string[] AttributeNames) GetEntityInfoForModelType(string modelType)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);
        return (modelTypeSyncInfo.EntityLogicalName, modelTypeSyncInfo.AttributeNames);
    }

    public static MandatoryQualification MapMandatoryQualificationFromDqtQualification(
        dfeta_qualification qualification,
        IEnumerable<dfeta_mqestablishment> mqEstablishments,
        IEnumerable<dfeta_specialism> mqSpecialisms,
        bool applyMigrationMappings)
    {
        if (qualification.dfeta_Type != dfeta_qualification_dfeta_Type.MandatoryQualification)
        {
            throw new ArgumentException("Qualification is not a mandatory qualification.", nameof(qualification));
        }

        var dqtSpecialism = qualification.dfeta_MQ_SpecialismId is not null ?
            mqSpecialisms.Single(s => s.Id == qualification.dfeta_MQ_SpecialismId.Id) :
            null;
        MandatoryQualificationSpecialism? specialism = null;

        MandatoryQualificationProvider? provider = null;
        var dqtMqStatus = qualification.dfeta_MQ_Status;
        if (applyMigrationMappings)
        {
            if (qualification.dfeta_MQ_MQEstablishmentId!?.Id is Guid establishmentId)
            {
                var establishment = mqEstablishments.Single(e => e.Id == establishmentId);

                MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(establishment, out provider);

                if (dqtSpecialism is not null)
                {
                    MandatoryQualificationSpecialismRegistry.TryMapFromDqtMqEstablishment(establishment.dfeta_Value, dqtSpecialism.dfeta_Value, out specialism);
                }
            }

            dqtMqStatus ??= (qualification.dfeta_MQ_Date.HasValue ? dfeta_qualification_dfeta_MQ_Status.Passed : null);
        }

        specialism ??= dqtSpecialism?.ToMandatoryQualificationSpecialism();
        MandatoryQualificationStatus? status = dqtMqStatus?.ToMandatoryQualificationStatus();

        return new MandatoryQualification()
        {
            QualificationId = qualification.Id,
            CreatedOn = qualification.CreatedOn!.Value,
            UpdatedOn = qualification.ModifiedOn!.Value,
            PersonId = qualification.dfeta_PersonId.Id,
            DqtQualificationId = qualification.Id,
            DqtState = (int)qualification.StateCode!,
            DqtCreatedOn = qualification.CreatedOn!.Value,
            DqtModifiedOn = qualification.ModifiedOn!.Value,
            ProviderId = provider?.MandatoryQualificationProviderId,
            Specialism = specialism,
            Status = status,
            StartDate = qualification.dfeta_MQStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            EndDate = qualification.dfeta_MQ_Date.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            DqtMqEstablishmentId = qualification.dfeta_MQ_MQEstablishmentId?.Id,
            DqtSpecialismId = qualification.dfeta_MQ_SpecialismId?.Id
        };
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

        var contacts = await GetEntities<Contact>(
            Contact.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            [contactId],
            modelTypeSyncInfo.AttributeNames,
            cancellationToken);

        return await SyncPersons(contacts, ignoreInvalid: false, cancellationToken) == 1;
    }

    public async Task<bool> SyncPerson(Contact entity, bool ignoreInvalid, CancellationToken cancellationToken = default) =>
        await SyncPersons(new[] { entity }, ignoreInvalid, cancellationToken) == 1;

    public async Task<int> SyncPersons(IReadOnlyCollection<Contact> entities, bool ignoreInvalid, CancellationToken cancellationToken = default)
    {
        // We're syncing all contacts for now.
        // Keep this in sync with the filter in the SyncAllContactsFromCrmJob job.
        IEnumerable<Contact> toSync = entities;

        if (ignoreInvalid)
        {
            // Some bad data in the build environment has a TRN that's longer than 7 digits
            toSync = toSync.Where(e => string.IsNullOrEmpty(e.dfeta_TRN) || e.dfeta_TRN.Length == 7);
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

        _syncedEntitiesSubject.OnNext(people.ToArray());
        return people.Count;
    }

    public async Task<bool> SyncMandatoryQualification(Guid qualificationId, IReadOnlyCollection<EventBase> events, CancellationToken cancellationToken)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(ModelTypes.MandatoryQualification);

        var qualifications = await GetEntities<dfeta_qualification>(
            dfeta_qualification.EntityLogicalName,
            dfeta_qualification.PrimaryIdAttribute,
            [qualificationId],
            modelTypeSyncInfo.AttributeNames,
            cancellationToken);

        if (IsFakeXrm)
        {
            var mqEstablishments = await referenceDataCache.GetMqEstablishments();
            var mqSpecialisms = await referenceDataCache.GetMqSpecialisms();

            var mqs = qualifications.Select(q => MapMandatoryQualificationFromDqtQualification(q, mqEstablishments, mqSpecialisms, applyMigrationMappings: false)).ToArray();

            return await SyncMandatoryQualifications(mqs, events, ignoreInvalid: false, cancellationToken) == 1;
        }
        else
        {
            return await SyncMandatoryQualifications(qualifications, ignoreInvalid: false, createMigratedEvent: false, cancellationToken) == 1;
        }
    }

    public async Task<bool> SyncMandatoryQualification(
        dfeta_qualification entity,
        AuditDetailCollection auditDetails,
        bool ignoreInvalid,
        bool createMigratedEvent,
        CancellationToken cancellationToken = default)
    {
        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { entity.Id, auditDetails }
        };

        return await SyncMandatoryQualifications(new[] { entity }, auditDetailsDict, ignoreInvalid, createMigratedEvent, cancellationToken) == 1;
    }

    public async Task<int> SyncMandatoryQualifications(
        IReadOnlyCollection<dfeta_qualification> entities,
        bool ignoreInvalid,
        bool createMigratedEvent,
        CancellationToken cancellationToken)
    {
        var auditDetails = await GetAuditRecords(dfeta_qualification.EntityLogicalName, entities.Select(q => q.Id), cancellationToken);

        return await SyncMandatoryQualifications(entities, auditDetails, ignoreInvalid, createMigratedEvent, cancellationToken);
    }

    public async Task<int> SyncMandatoryQualifications(
        IReadOnlyCollection<dfeta_qualification> entities,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid,
        bool createMigratedEvent,
        CancellationToken cancellationToken = default)
    {
        // Not all dfeta_qualification records are MQs..
        var toSync = entities.Where(q => q.dfeta_Type == dfeta_qualification_dfeta_Type.MandatoryQualification);

        var (mqs, events) = await MapMandatoryQualificationsAndAudits(toSync, auditDetails, createMigratedEvent);

        return await SyncMandatoryQualifications(mqs, events, ignoreInvalid, cancellationToken);
    }

    private async Task<int> SyncMandatoryQualifications(
        IReadOnlyCollection<MandatoryQualification> mqs,
        IReadOnlyCollection<EventBase> events,
        bool ignoreInvalid,
        CancellationToken cancellationToken)
    {
        if (mqs.Count == 0)
        {
            Debug.Assert(events.Count == 0);
            return 0;
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo<MandatoryQualification>(ModelTypes.MandatoryQualification);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        var toSync = mqs.ToList();

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

                foreach (var mq in toSync)
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

                await txn.SaveEvents(events, "events_import", clock, cancellationToken);

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
                        toSync.RemoveAll(mq => mq.PersonId == personId);
                        if (toSync.Count == 0)
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

        _syncedEntitiesSubject.OnNext([.. toSync, .. events]);
        return toSync.Count;
    }

    private EntityVersionInfo<TEntity>[] GetEntityVersions<TEntity>(TEntity latest, IEnumerable<AuditDetail> auditDetails, string[] attributeNames)
        where TEntity : Entity
    {
        var created = latest.GetAttributeValue<DateTime?>("createdon")!.Value;
        var createdBy = latest.GetAttributeValue<EntityReference>("createdby");

        var ordered = auditDetails
            .OfType<AttributeAuditDetail>()
            .Select(a => (AuditDetail: a, AuditRecord: a.AuditRecord.ToEntity<Audit>()))
            .OrderBy(a => a.AuditRecord.CreatedOn)
            .ToArray();

        if (ordered.Length == 0)
        {
            return [new EntityVersionInfo<TEntity>(latest.Id, latest, ChangedAttributes: Array.Empty<string>(), created, createdBy.Id, createdBy.Name)];
        }

        var versions = new List<EntityVersionInfo<TEntity>>();

        var initialVersion = GetInitialVersion();
        versions.Add(new EntityVersionInfo<TEntity>(initialVersion.Id, initialVersion, ChangedAttributes: Array.Empty<string>(), created, createdBy.Id, createdBy.Name));

        latest = initialVersion.ShallowClone();
        foreach (var audit in ordered)
        {
            if (audit.AuditRecord.Action == Audit_Action.Create)
            {
                if (audit != ordered[0])
                {
                    throw new InvalidOperationException($"Expected the {Audit_Action.Create} audit to be first.");
                }

                continue;
            }

            var thisVersion = latest.ShallowClone();
            var changedAttributes = new List<string>();

            foreach (var attr in audit.AuditDetail.DeletedAttributes)
            {
                thisVersion.Attributes.Remove(attr.Value);
                changedAttributes.Add(attr.Value);
            }

            foreach (var attr in audit.AuditDetail.NewValue.Attributes)
            {
                thisVersion.Attributes[attr.Key] = attr.Value;
                changedAttributes.Add(attr.Key);
            }

            versions.Add(new EntityVersionInfo<TEntity>(
                audit.AuditRecord.Id,
                thisVersion,
                changedAttributes.ToArray(),
                audit.AuditRecord.CreatedOn!.Value,
                audit.AuditRecord.UserId.Id,
                audit.AuditRecord.UserId.Name));

            latest = thisVersion;
        }

        return versions.ToArray();

        TEntity GetInitialVersion()
        {
            if (ordered[0] is { AuditRecord: { Action: Audit_Action.Create } } createAction)
            {
                var entity = createAction.AuditDetail.NewValue.ToEntity<TEntity>();
                entity.Id = latest.Id;
                entity["createdon"] = created;
                entity["createdby"] = createdBy;
                entity["modifiedon"] = created;
                return entity;
            }

            // Starting with `latest`, go through each event in reverse and undo the changes it applied.
            // When we're done we end up with the initial version of the record.
            var initial = latest.ShallowClone();

            foreach (var a in ordered.Reverse())
            {
                // Check that new attributes align with what we have in `initial`;
                // if they don't, then we've got an incomplete history
                foreach (var attr in a.AuditDetail.NewValue.Attributes.Where(kvp => attributeNames.Contains(kvp.Key)))
                {
                    if (!AttributeValuesEqual(attr.Value, initial.Attributes.TryGetValue(attr.Key, out var initialAttr) ? initialAttr : null))
                    {
                        throw new Exception($"Non-contiguous audit records for {initial.LogicalName} '{initial.Id}':\n" +
                            $"Expected '{attr.Key}' to be '{attr.Value ?? "<null>"}' but was '{initialAttr ?? "<null>"}'.");
                    }

                    if (!a.AuditDetail.OldValue.Attributes.Contains(attr.Key))
                    {
                        initial.Attributes.Remove(attr.Key);
                    }
                }

                foreach (var attr in a.AuditDetail.OldValue.Attributes.Where(kvp => attributeNames.Contains(kvp.Key)))
                {
                    initial.Attributes[attr.Key] = attr.Value;
                }
            }

            return initial;
        }

        static bool AttributeValuesEqual(object? first, object? second)
        {
            if (first is null && second is null)
            {
                return true;
            }

            if (first is null || second is null)
            {
                return false;
            }

            if (first.GetType() != second.GetType())
            {
                return false;
            }

            return first is EntityReference firstRef && second is EntityReference secondRef ?
                firstRef.Name == secondRef.Name && firstRef.Id == secondRef.Id :
                first.Equals(second);
        }
    }

    private static ModelTypeSyncInfo<TModel> GetModelTypeSyncInfo<TModel>(string modelType) =>
        (ModelTypeSyncInfo<TModel>)GetModelTypeSyncInfo(modelType);

    private static ModelTypeSyncInfo GetModelTypeSyncInfo(string modelType) =>
        _modelTypeSyncInfo.TryGetValue(modelType, out var modelTypeSyncInfo) ?
            modelTypeSyncInfo :
            throw new ArgumentException($"Unknown data type: '{modelType}.", nameof(modelType));

    private async Task<IReadOnlyDictionary<Guid, AuditDetailCollection>> GetAuditRecords(
        string entityLogicalName,
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken)
    {
        return (await Task.WhenAll(ids
            .Chunk(MaxAuditRequestsPerBatch)
            .Select(async chunk =>
            {
                var request = new ExecuteMultipleRequest()
                {
                    Requests = new(),
                    Settings = new()
                    {
                        ContinueOnError = false,
                        ReturnResponses = true
                    }
                };

                request.Requests.AddRange(chunk.Select(e => new RetrieveRecordChangeHistoryRequest() { Target = e.ToEntityReference(entityLogicalName) }));

                ExecuteMultipleResponse response;
                while (true)
                {
                    try
                    {
                        response = (ExecuteMultipleResponse)await organizationService.ExecuteAsync(request, cancellationToken);
                    }
                    catch (FaultException fex) when (fex.IsCrmRateLimitException(out var retryAfter))
                    {
                        await Task.Delay(retryAfter, cancellationToken);
                        continue;
                    }

                    if (response.IsFaulted)
                    {
                        var firstFault = response.Responses.First(r => r.Fault is not null).Fault;

                        if (firstFault.IsCrmRateLimitFault(out var retryAfter))
                        {
                            await Task.Delay(retryAfter, cancellationToken);
                            continue;
                        }
                        else if (firstFault.Message.Contains("The HTTP status code of the response was not expected (429)"))
                        {
                            await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
                            continue;
                        }

                        throw new FaultException<OrganizationServiceFault>(firstFault, new FaultReason(firstFault.Message));
                    }

                    break;
                }

                return response.Responses.Zip(
                    chunk,
                    (r, e) => (Id: e, ((RetrieveRecordChangeHistoryResponse)r.Response).AuditDetailCollection));
            })))
            .SelectMany(b => b)
            .ToDictionary(t => t.Id, t => t.AuditDetailCollection);
    }

    private async Task<TEntity[]> GetEntities<TEntity>(
        string entityLogicalName,
        string idAttributeName,
        IEnumerable<Guid> ids,
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
            "created_on",
            "updated_on",
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
            INSERT INTO {tableName} AS t ({columnList}, dqt_first_sync, dqt_last_sync)
            SELECT {columnList}, {NowParameterName}, {NowParameterName} FROM {tempTableName}
            ON CONFLICT (person_id) DO UPDATE
            SET dqt_last_sync = {NowParameterName}, {string.Join(", ", columnsToUpdate.Select(c => $"{c} = EXCLUDED.{c}"))}
            WHERE t.dqt_modified_on < EXCLUDED.dqt_modified_on
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
            writer.WriteValueOrNull(person.CreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(person.UpdatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(person.Trn, NpgsqlDbType.Char);
            writer.WriteValueOrNull(person.FirstName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.MiddleName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.LastName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.DateOfBirth, NpgsqlDbType.Date);
            writer.WriteValueOrNull(person.EmailAddress, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.NationalInsuranceNumber, NpgsqlDbType.Char);
            writer.WriteValueOrNull(person.DqtContactId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(person.DqtState, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(person.DqtCreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(person.DqtModifiedOn, NpgsqlDbType.TimestampTz);
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
            "mq_provider_id",
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
            INSERT INTO {tableName} AS t ({columnList}, dqt_first_sync, dqt_last_sync)
            SELECT {columnList}, {NowParameterName}, {NowParameterName} FROM {tempTableName}
            ON CONFLICT (qualification_id) DO UPDATE
            SET dqt_last_sync = {NowParameterName}, {string.Join(", ", columnsToUpdate.Select(c => $"{c} = EXCLUDED.{c}"))}
            WHERE t.dqt_modified_on < EXCLUDED.dqt_modified_on
            """;

        var deleteStatement = $"DELETE FROM {tableName} WHERE dqt_qualification_id = ANY({IdsParameterName})";

        var getLastModifiedOnStatement = $"SELECT MAX(dqt_modified_on) FROM {tableName}";

        var attributeNames = new[]
        {
            dfeta_qualification.Fields.dfeta_qualificationId,
            dfeta_qualification.Fields.CreatedOn,
            dfeta_qualification.Fields.CreatedBy,
            dfeta_qualification.Fields.ModifiedOn,
            dfeta_qualification.Fields.dfeta_TRSEvent,
            dfeta_qualification.Fields.dfeta_Type,
            dfeta_qualification.Fields.dfeta_PersonId,
            dfeta_qualification.Fields.StateCode,
            dfeta_qualification.Fields.StatusCode,
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
            writer.WriteValueOrNull(mq.DeletedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull((int)QualificationType.MandatoryQualification, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(mq.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(mq.DqtQualificationId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(mq.DqtState, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(mq.DqtCreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(mq.DqtModifiedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(mq.ProviderId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull((int?)mq.Specialism, NpgsqlDbType.Integer);
            writer.WriteValueOrNull((int?)mq.Status, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(mq.StartDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull(mq.EndDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull(mq.DqtMqEstablishmentId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(mq.DqtSpecialismId, NpgsqlDbType.Uuid);
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
            GetSyncHandler = helper => (entities, ignoreInvalid, ct) =>
                helper.SyncMandatoryQualifications(entities.Select(e => e.ToEntity<dfeta_qualification>()).ToArray(), ignoreInvalid, createMigratedEvent: false, ct),
            WriteRecord = writeRecord
        };
    }

    private static List<Person> MapPersons(IEnumerable<Contact> contacts) => contacts
        .Select(c => new Person()
        {
            PersonId = c.ContactId!.Value,
            CreatedOn = c.CreatedOn!.Value,
            UpdatedOn = c.ModifiedOn!.Value,
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

    private async Task<(List<MandatoryQualification> MandatoryQualifications, List<EventBase> Events)> MapMandatoryQualificationsAndAudits(
        IEnumerable<dfeta_qualification> qualifications,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool createMigratedEvent)
    {
        var mqEstablishments = await referenceDataCache.GetMqEstablishments();
        var mqSpecialisms = await referenceDataCache.GetMqSpecialisms();

        var mqs = new List<MandatoryQualification>();
        var events = new List<EventBase>();

        foreach (var q in qualifications)
        {
            var mapped = MapMandatoryQualificationFromDqtQualification(q, mqEstablishments, mqSpecialisms, applyMigrationMappings: true);

            var audits = auditDetails[q.Id].AuditDetails;
            var versions = GetEntityVersions(q, audits, GetModelTypeSyncInfoForMandatoryQualification().AttributeNames);

            events.Add(audits.Any(a => a.AuditRecord.ToEntity<Audit>().Action == Audit_Action.Create) ?
                MapCreatedEvent(versions.First()) :
                MapImportedEvent(versions.First()));

            foreach (var (thisVersion, previousVersion) in versions.Skip(1).Zip(versions, (thisVersion, previousVersion) => (thisVersion, previousVersion)))
            {
                var mappedEvent = MapUpdatedEvent(thisVersion, previousVersion);

                if (mappedEvent is not null)
                {
                    events.Add(mappedEvent);
                }
            }

            // If the record is deactivated, the final event should be a MandatoryQualificationDeletedEvent or MandatoryQualificationDqtDeactivatedEvent
            // or we should have a MandatoryQualificationDqtImportedEvent (with inactive status)
            if (q.StateCode == dfeta_qualificationState.Inactive)
            {
                var lastEvent = events.Last();

                if (lastEvent is MandatoryQualificationDqtDeactivatedEvent or MandatoryQualificationDeletedEvent ||
                    lastEvent is MandatoryQualificationDqtImportedEvent { DqtState: (int)dfeta_qualificationState.Inactive })
                {
                    mapped.DeletedOn = lastEvent.CreatedUtc;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Expected last event to be a {nameof(MandatoryQualificationDqtDeactivatedEvent)}" +
                        $" or {nameof(MandatoryQualificationDeletedEvent)} but was {lastEvent.GetEventName()}" +
                        $" (qualification ID: '{q.Id}').");
                }
            }
            else if (createMigratedEvent)
            {
                events.Add(MapMigratedEvent(versions.Last(), mqSpecialisms));
            }

            mqs.Add(mapped);
        }

        return (mqs, events);

        EventBase MapCreatedEvent(EntityVersionInfo<dfeta_qualification> snapshot)
        {
            if (TryDeserializeEventAttribute(snapshot.Entity.Attributes, dfeta_qualification.Fields.dfeta_TRSEvent, out var eventFromAttr))
            {
                if (!(eventFromAttr is MandatoryQualificationCreatedEvent))
                {
                    throw new InvalidOperationException(
                        $"Expected event to be {nameof(MandatoryQualificationCreatedEvent)} but got {eventFromAttr.GetEventName()}.");
                }

                return eventFromAttr;
            }

            return new MandatoryQualificationCreatedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Entity.Id}-Created",
                CreatedUtc = snapshot.Timestamp,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                PersonId = snapshot.Entity.dfeta_PersonId.Id,
                MandatoryQualification = GetEventMandatoryQualification(snapshot.Entity, applyMigrationMappings: false)
            };
        }

        EventBase MapImportedEvent(EntityVersionInfo<dfeta_qualification> snapshot)
        {
            return new MandatoryQualificationDqtImportedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Entity.Id}-Imported",
                CreatedUtc = snapshot.Timestamp,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                PersonId = snapshot.Entity.dfeta_PersonId.Id,
                MandatoryQualification = GetEventMandatoryQualification(snapshot.Entity, applyMigrationMappings: false),
                DqtState = (int)snapshot.Entity.StateCode!
            };
        }

        EventBase? MapUpdatedEvent(EntityVersionInfo<dfeta_qualification> snapshot, EntityVersionInfo<dfeta_qualification> previous)
        {
            if (TryDeserializeEventAttribute(snapshot.Entity.Attributes, dfeta_qualification.Fields.dfeta_TRSEvent, out var eventFromAttr))
            {
                // dfeta_TRSEvent may contain a stale value from a previous version
                // (if the record was updated from the CRM UI then dfeta_TRSEvent won't have been updated).
                // Check the previous events we've created; if we've seen it before we know we should ignore it this time.
                if (!events.Any(e => e.EventId == eventFromAttr.EventId))
                {
                    return eventFromAttr;
                }
            }

            if (snapshot.ChangedAttributes.Contains(dfeta_qualification.Fields.StateCode))
            {
                var nonStateAttributes = snapshot.ChangedAttributes
                    .Where(a => !(a is dfeta_qualification.Fields.StateCode or dfeta_qualification.Fields.StatusCode))
                    .ToArray();

                if (nonStateAttributes.Length > 0)
                {
                    throw new InvalidOperationException(
                        $"Expected state and status attributes to change in isolation but also received: {string.Join(", ", nonStateAttributes)}.");
                }

                if (snapshot.Entity.StateCode == dfeta_qualificationState.Inactive)
                {
                    return new MandatoryQualificationDqtDeactivatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        Key = $"{snapshot.Id}",  // The CRM Audit ID
                        CreatedUtc = snapshot.Timestamp,
                        RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                        PersonId = snapshot.Entity.dfeta_PersonId.Id,
                        MandatoryQualification = GetEventMandatoryQualification(snapshot.Entity, applyMigrationMappings: false)
                    };
                }
                else
                {
                    return new MandatoryQualificationDqtReactivatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        Key = $"{snapshot.Id}",  // The CRM Audit ID
                        CreatedUtc = snapshot.Timestamp,
                        RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                        PersonId = snapshot.Entity.dfeta_PersonId.Id,
                        MandatoryQualification = GetEventMandatoryQualification(snapshot.Entity, applyMigrationMappings: false)
                    };
                }
            }

            var changes = MandatoryQualificationUpdatedEventChanges.None |
                (snapshot.ChangedAttributes.Contains(dfeta_qualification.Fields.dfeta_MQ_MQEstablishmentId) ? MandatoryQualificationUpdatedEventChanges.Provider : 0) |
                (snapshot.ChangedAttributes.Contains(dfeta_qualification.Fields.dfeta_MQ_SpecialismId) ? MandatoryQualificationUpdatedEventChanges.Specialism : 0) |
                (snapshot.ChangedAttributes.Contains(dfeta_qualification.Fields.dfeta_MQ_Status) ? MandatoryQualificationUpdatedEventChanges.Status : 0) |
                (snapshot.ChangedAttributes.Contains(dfeta_qualification.Fields.dfeta_MQStartDate) ? MandatoryQualificationUpdatedEventChanges.StartDate : 0) |
                (snapshot.ChangedAttributes.Contains(dfeta_qualification.Fields.dfeta_MQ_Date) ? MandatoryQualificationUpdatedEventChanges.EndDate : 0);

            if (changes == MandatoryQualificationUpdatedEventChanges.None)
            {
                return null;
            }

            return new MandatoryQualificationUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Id}",  // The CRM Audit ID
                CreatedUtc = snapshot.Timestamp,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                PersonId = snapshot.Entity.dfeta_PersonId.Id,
                MandatoryQualification = GetEventMandatoryQualification(snapshot.Entity, applyMigrationMappings: false),
                OldMandatoryQualification = GetEventMandatoryQualification(previous.Entity, applyMigrationMappings: false),
                ChangeReason = null,
                ChangeReasonDetail = null,
                EvidenceFile = null,
                Changes = changes
            };
        }

        EventBase MapMigratedEvent(EntityVersionInfo<dfeta_qualification> snapshot, dfeta_specialism[] mqSpecialisms)
        {
            var eventMandatoryQualification = GetEventMandatoryQualification(snapshot.Entity, applyMigrationMappings: true);

            var providerChanged = eventMandatoryQualification.Provider?.Name != eventMandatoryQualification.Provider?.DqtMqEstablishmentName;

            var specialismChanged = snapshot.Entity.dfeta_MQ_SpecialismId?.Id is Guid dqtSpecialismId ?
                mqSpecialisms.Single(s => s.Id == dqtSpecialismId).ToMandatoryQualificationSpecialism() != eventMandatoryQualification.Specialism :
                false;

            var changes = MandatoryQualificationMigratedEventChanges.None |
                (providerChanged ? MandatoryQualificationMigratedEventChanges.Provider : 0) |
                (specialismChanged ? MandatoryQualificationMigratedEventChanges.Specialism : 0);

            return new MandatoryQualificationMigratedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Entity.Id}-Migrated",
                CreatedUtc = clock.UtcNow,
                RaisedBy = EventModels.RaisedByUserInfo.FromUserId(Core.DataStore.Postgres.Models.SystemUser.SystemUserId),
                PersonId = snapshot.Entity.dfeta_PersonId.Id,
                MandatoryQualification = eventMandatoryQualification,
                Changes = changes
            };
        }

        EventModels.MandatoryQualification GetEventMandatoryQualification(dfeta_qualification snapshot, bool applyMigrationMappings)
        {
            var mapped = MapMandatoryQualificationFromDqtQualification(snapshot, mqEstablishments, mqSpecialisms, applyMigrationMappings);

            var establishment = snapshot.dfeta_MQ_MQEstablishmentId?.Id is Guid establishmentId ?
                mqEstablishments.Single(e => e.Id == establishmentId) :
                null;

            var provider = MandatoryQualificationProvider.All.SingleOrDefault(p => p.MandatoryQualificationProviderId == mapped.ProviderId);

            return new()
            {
                QualificationId = mapped.QualificationId,
                Provider = establishment is not null ?
                    new()
                    {
                        MandatoryQualificationProviderId = provider?.MandatoryQualificationProviderId,
                        Name = provider?.Name,
                        DqtMqEstablishmentId = establishment?.Id,
                        DqtMqEstablishmentName = establishment?.dfeta_name
                    } :
                    null,
                Specialism = mapped.Specialism,
                Status = mapped.Status,
                StartDate = mapped.StartDate,
                EndDate = mapped.EndDate,
            };
        }

        bool TryDeserializeEventAttribute(AttributeCollection attributes, string key, [NotNullWhen(true)] out EventBase? @event)
        {
            if (attributes.TryGetValue(key, out var attrValue) && attrValue is string serializedEvent)
            {
                var evt = EventInfo.Deserialize(serializedEvent);

                if (!(evt.Event is DummyEvent))
                {
                    @event = evt.Event;
                    return true;
                }
            }

            @event = default;
            return false;
        }
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

    private record EntityVersionInfo<TEntity>(
        Guid Id,
        TEntity Entity,
        string[] ChangedAttributes,
        DateTime Timestamp,
        Guid UserId,
        string UserName)
        where TEntity : Entity;

    public static class ModelTypes
    {
        public const string Person = "Person";
        public const string MandatoryQualification = "MandatoryQualification";
    }
}

file static class Extensions
{
    public static TEntity ShallowClone<TEntity>(this TEntity entity) where TEntity : Entity
    {
        // N.B. This only clones Attributes

        var cloned = new Entity(entity.LogicalName, entity.Id);

        foreach (var attr in entity.Attributes)
        {
            cloned.Attributes.Add(attr.Key, attr.Value);
        }

        return cloned.ToEntity<TEntity>();
    }

    /// <summary>
    /// Returns <c>null</c> if <paramref name="value"/> is empty or whitespace.
    /// </summary>
    public static string? NormalizeString(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}

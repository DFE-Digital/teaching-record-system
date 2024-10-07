using System.Diagnostics;
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
    NpgsqlDataSource trsDbDataSource,
    [FromKeyedServices(TrsDataSyncService.CrmClientName)] IOrganizationServiceAsync2 organizationService,
    ReferenceDataCache referenceDataCache,
    IClock clock)
{
    private delegate Task SyncEntitiesHandler(IReadOnlyCollection<Entity> entities, bool ignoreInvalid, bool dryRun, CancellationToken cancellationToken);

    private const string NowParameterName = "@now";
    private const string IdsParameterName = "@ids";
    private const int MaxAuditRequestsPerBatch = 10;

    private static readonly IReadOnlyDictionary<string, ModelTypeSyncInfo> _modelTypeSyncInfo = new Dictionary<string, ModelTypeSyncInfo>()
    {
        { ModelTypes.Person, GetModelTypeSyncInfoForPerson() },
        { ModelTypes.Event, GetModelTypeSyncInfoForEvent() },
        { ModelTypes.Alert, GetModelTypeSyncInfoForAlert() },
    };

    private readonly ISubject<object[]> _syncedEntitiesSubject = new Subject<object[]>();

    public IObservable<object[]> GetSyncedEntitiesObservable() => _syncedEntitiesSubject;

    private bool IsFakeXrm { get; } = organizationService.GetType().FullName == "Castle.Proxies.ObjectProxy_2";

    public static (string EntityLogicalName, string[] AttributeNames) GetEntityInfoForModelType(string modelType)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);
        return (modelTypeSyncInfo.EntityLogicalName, modelTypeSyncInfo.AttributeNames);
    }

    public static Alert? MapAlertFromDqtSanction(
        dfeta_sanction sanction,
        IEnumerable<dfeta_sanctioncode> sanctionCodes,
        IEnumerable<AlertType> alertTypes,
        bool ignoreInvalid)
    {
        if (sanction.dfeta_SanctionCodeId is null)
        {
            if (ignoreInvalid)
            {
                return null;
            }

            throw new InvalidOperationException($"Santion {sanction.Id} does not have a {nameof(dfeta_sanction.Fields.dfeta_SanctionCodeId)}.");
        }

        var sanctionCode = sanctionCodes.Single(c => c.Id == sanction.dfeta_SanctionCodeId.Id).dfeta_Value;
        var alertType = alertTypes.SingleOrDefault(t => t.DqtSanctionCode == sanctionCode);

        if (alertType is null)
        {
            return null;
        }

        return new Alert()
        {
            AlertId = sanction.Id,
            AlertTypeId = alertType.AlertTypeId,
            PersonId = sanction.dfeta_PersonId.Id,
            Details = sanction.dfeta_SanctionDetails,
            ExternalLink = sanction.dfeta_DetailsLink,
            StartDate = sanction.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            EndDate = sanction.dfeta_EndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            CreatedOn = sanction.CreatedOn!.Value,
            UpdatedOn = sanction.ModifiedOn!.Value,
            DqtSanctionId = sanction.Id,
            DqtState = (int)sanction.StateCode!,
            DqtCreatedOn = sanction.CreatedOn!.Value,
            DqtModifiedOn = sanction.ModifiedOn!.Value,
        };
    }

    public async Task DeleteRecords(string modelType, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return;
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);

        if (modelTypeSyncInfo.DeleteStatement is null)
        {
            throw new NotSupportedException($"Cannot delete a {modelType}.");
        }

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);

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

        if (modelTypeSyncInfo.GetLastModifiedOnStatement is null)
        {
            return null;
        }

        await using var connection = await trsDbDataSource.OpenConnectionAsync();

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = modelTypeSyncInfo.GetLastModifiedOnStatement;
            await using var reader = await cmd.ExecuteReaderAsync();
            var read = reader.Read();
            Debug.Assert(read);
            return reader.IsDBNull(0) ? null : reader.GetDateTime(0);
        }
    }

    public Task SyncRecords(string modelType, IReadOnlyCollection<Entity> entities, bool ignoreInvalid, bool dryRun, CancellationToken cancellationToken = default)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);
        return modelTypeSyncInfo.GetSyncHandler(this)(entities, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<bool> SyncPerson(Guid contactId, bool ignoreInvalid = false, bool dryRun = false, CancellationToken cancellationToken = default)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(ModelTypes.Person);

        var contacts = await GetEntities<Contact>(
            Contact.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            [contactId],
            modelTypeSyncInfo.AttributeNames,
            cancellationToken);

        return await SyncPersons(contacts, ignoreInvalid, dryRun, cancellationToken) == 1;
    }

    public async Task<bool> SyncPerson(Contact entity, bool ignoreInvalid, bool dryRun = false, CancellationToken cancellationToken = default) =>
        await SyncPersons(new[] { entity }, ignoreInvalid, dryRun, cancellationToken) == 1;

    public async Task<int> SyncPersons(IReadOnlyCollection<Contact> entities, bool ignoreInvalid, bool dryRun, CancellationToken cancellationToken = default)
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

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);
        using var txn = await connection.BeginTransactionAsync(cancellationToken);

        using (var createTempTableCommand = connection.CreateCommand())
        {
            createTempTableCommand.CommandText = modelTypeSyncInfo.CreateTempTableStatement;
            createTempTableCommand.Transaction = txn;
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        using var writer = await connection.BeginBinaryImportAsync(modelTypeSyncInfo.CopyStatement!, cancellationToken);

        foreach (var person in people)
        {
            writer.StartRow();
            modelTypeSyncInfo.WriteRecord!(writer, person);
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

            return await SyncPersons(entitiesExceptFailedOne, ignoreInvalid, dryRun, cancellationToken);
        }

        if (!dryRun)
        {
            await txn.CommitAsync(cancellationToken);
        }

        _syncedEntitiesSubject.OnNext(people.ToArray());
        return people.Count;
    }

    public async Task<bool> SyncAlert(
        dfeta_sanction entity,
        AuditDetailCollection auditDetails,
        bool ignoreInvalid,
        bool createMigratedEvent,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { entity.Id, auditDetails }
        };

        return await SyncAlerts(new[] { entity }, auditDetailsDict, ignoreInvalid, createMigratedEvent, dryRun, cancellationToken) == 1;
    }

    public async Task<int> SyncAlerts(
        IReadOnlyCollection<dfeta_sanction> entities,
        bool ignoreInvalid,
        bool createMigratedEvent,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var auditDetails = await GetAuditRecords(dfeta_sanction.EntityLogicalName, entities.Select(q => q.Id), cancellationToken);

        return await SyncAlerts(entities, auditDetails, ignoreInvalid, createMigratedEvent, dryRun, cancellationToken);
    }

    public async Task<int> SyncAlerts(
        IReadOnlyCollection<dfeta_sanction> entities,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid,
        bool createMigratedEvent,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        var (alerts, events) = await MapAlertsAndAudits(entities, auditDetails, ignoreInvalid, createMigratedEvent);

        return await SyncAlerts(alerts, events, ignoreInvalid, dryRun, cancellationToken);
    }

    private async Task<int> SyncAlerts(
        IReadOnlyCollection<Alert> alerts,
        IReadOnlyCollection<EventBase> events,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        if (alerts.Count == 0)
        {
            Debug.Assert(events.Count == 0);
            return 0;
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo<Alert>(ModelTypes.Alert);

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);

        var toSync = alerts.ToList();

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

                using var writer = await connection.BeginBinaryImportAsync(modelTypeSyncInfo.CopyStatement!, cancellationToken);

                foreach (var a in toSync)
                {
                    writer.StartRow();
                    modelTypeSyncInfo.WriteRecord!(writer, a);
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

                if (!dryRun)
                {
                    await txn.CommitAsync(cancellationToken);
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "23503" && ex.ConstraintName == Alert.PersonForeignKeyName)
            {
                // Person does not exist.
                // This is unlikely to happen often so the handling of this doesn't need to be super optimal.
                // In this case we just pull the contact record directly, sync it then go again.
                // Since the person sync happens in its own transaction we need to ensure dryRun is false otherwise this will loop forever.

                // ex.Detail will be something like "Key (person_id)=(6ac8dc26-c8ae-e311-b8ed-005056822391) is not present in table "persons"."
                var personId = Guid.Parse(ex.Detail!.Substring("Key (person_id)=(".Length, Guid.Empty.ToString().Length));

                var personSynced = await SyncPerson(personId, ignoreInvalid, dryRun: false, cancellationToken);
                if (!personSynced)
                {
                    // The person sync may fail if the record doesn't meet the criteria (e.g. it doesn't have a TRN).
                    // If we're ignoring invalid records we can skip MQs that reference this person ID,
                    // otherwise we blow up.

                    if (ignoreInvalid)
                    {
                        toSync.RemoveAll(a => a.PersonId == personId);
                        if (toSync.Count == 0)
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Attempted to sync Alert for person '{personId}' but the person record does not meet the sync criteria.");
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

    public async Task<int> SyncEvents(IReadOnlyCollection<dfeta_TRSEvent> events, bool dryRun, CancellationToken cancellationToken = default)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo<EventInfo>(ModelTypes.Event);

        var mapped = events.Select(e => EventInfo.Deserialize(e.dfeta_Payload).Event).ToArray();

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);

        using var txn = await connection.BeginTransactionAsync(cancellationToken);
        await txn.SaveEvents(mapped, tempTableSuffix: "events_import", clock, cancellationToken);

        if (!dryRun)
        {
            await txn.CommitAsync();
        }

        _syncedEntitiesSubject.OnNext(events.ToArray());
        return events.Count;
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
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncPersons(entities.Select(e => e.ToEntity<Contact>()).ToArray(), ignoreInvalid, dryRun, ct),
            WriteRecord = writeRecord
        };
    }

    private static ModelTypeSyncInfo GetModelTypeSyncInfoForEvent()
    {
        var attributeNames = new[]
        {
            dfeta_TRSEvent.Fields.dfeta_TRSEventId,
            dfeta_TRSEvent.Fields.dfeta_EventName,
            dfeta_TRSEvent.Fields.dfeta_Payload,
        };

        return new ModelTypeSyncInfo<EventInfo>()
        {
            CreateTempTableStatement = null,
            CopyStatement = null,
            InsertStatement = null,
            DeleteStatement = null,
            GetLastModifiedOnStatement = null,
            EntityLogicalName = dfeta_TRSEvent.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncEvents(entities.Select(e => e.ToEntity<dfeta_TRSEvent>()).ToArray(), dryRun, ct),
            WriteRecord = null
        };
    }

    private static ModelTypeSyncInfo GetModelTypeSyncInfoForAlert()
    {
        var tempTableName = "temp_alerts_import";
        var tableName = "alerts";

        var columnNames = new[]
        {
            "alert_id",
            "alert_type_id",
            "person_id",
            "details",
            "external_link",
            "start_date",
            "end_date",
            "created_on",
            "updated_on",
            "deleted_on",
            "dqt_sanction_id",
            "dqt_state",
            "dqt_created_on",
            "dqt_modified_on",
        };

        var columnsToUpdate = columnNames.Except(new[] { "alert_id", "dqt_sanction_id" }).ToArray();

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement = $"CREATE TEMP TABLE {tempTableName} (LIKE {tableName} INCLUDING DEFAULTS)";

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var insertStatement =
            $"""
            INSERT INTO {tableName} AS t ({columnList})
            SELECT {columnList} FROM {tempTableName}
            ON CONFLICT (alert_id) DO UPDATE
            SET {string.Join(", ", columnsToUpdate.Select(c => $"{c} = EXCLUDED.{c}"))}
            WHERE t.dqt_modified_on < EXCLUDED.dqt_modified_on
            """;

        var deleteStatement = $"DELETE FROM {tableName} WHERE dqt_sanction_id = ANY({IdsParameterName})";

        var getLastModifiedOnStatement = $"SELECT MAX(dqt_modified_on) FROM {tableName}";

        var attributeNames = new[]
        {
            dfeta_sanction.Fields.dfeta_sanctionId,
            dfeta_sanction.Fields.dfeta_SanctionCodeId,
            dfeta_sanction.Fields.CreatedOn,
            dfeta_sanction.Fields.CreatedBy,
            dfeta_sanction.Fields.ModifiedOn,
            dfeta_sanction.Fields.dfeta_StartDate,
            dfeta_sanction.Fields.dfeta_EndDate,
            dfeta_sanction.Fields.dfeta_PersonId,
            dfeta_sanction.Fields.dfeta_SanctionDetails,
            dfeta_sanction.Fields.dfeta_DetailsLink,
            dfeta_sanction.Fields.dfeta_Spent,
            dfeta_sanction.Fields.StateCode,
            dfeta_sanction.Fields.StatusCode
        };

        Action<NpgsqlBinaryImporter, Alert> writeRecord = (writer, alert) =>
        {
            writer.WriteValueOrNull(alert.AlertId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(alert.AlertTypeId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(alert.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(alert.Details, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(alert.ExternalLink, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(alert.StartDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull(alert.EndDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull(alert.CreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(alert.UpdatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(alert.DeletedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(alert.DqtSanctionId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(alert.DqtState, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(alert.DqtCreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(alert.DqtModifiedOn, NpgsqlDbType.TimestampTz);
        };

        return new ModelTypeSyncInfo<Alert>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            InsertStatement = insertStatement,
            DeleteStatement = deleteStatement,
            GetLastModifiedOnStatement = getLastModifiedOnStatement,
            EntityLogicalName = dfeta_sanction.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncAlerts(entities.Select(e => e.ToEntity<dfeta_sanction>()).ToArray(), ignoreInvalid, createMigratedEvent: false, dryRun, ct),
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

    private async Task<(List<Alert> Alerts, List<EventBase> Events)> MapAlertsAndAudits(
        IEnumerable<dfeta_sanction> sanctions,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid,
        bool createMigratedEvent)
    {
        var sanctionCodes = await referenceDataCache.GetSanctionCodes(activeOnly: false);
        var alertTypes = await referenceDataCache.GetAlertTypes();

        var alerts = new List<Alert>();
        var events = new List<EventBase>();

        foreach (var s in sanctions)
        {
            var mapped = MapAlertFromDqtSanction(s, sanctionCodes, alertTypes, ignoreInvalid);

            if (mapped is null)
            {
                continue;
            }

            var audits = auditDetails[s.Id].AuditDetails;
            var versions = GetEntityVersions(s, audits, GetModelTypeSyncInfo(ModelTypes.Alert).AttributeNames);

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

            // If the record is deactivated then it's migrated as deleted
            if (s.StateCode == dfeta_sanctionState.Inactive)
            {
                mapped.DeletedOn = clock.UtcNow;
            }
            else if (createMigratedEvent)
            {
                events.Add(MapMigratedEvent(versions.Last()));
            }

            alerts.Add(mapped);
        }

        return (alerts, events);

        EventBase MapCreatedEvent(EntityVersionInfo<dfeta_sanction> snapshot)
        {
            return new AlertCreatedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Entity.Id}-Created",
                CreatedUtc = snapshot.Timestamp,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                PersonId = snapshot.Entity.dfeta_PersonId.Id,
                Alert = GetEventAlert(snapshot.Entity, applyMigrationMappings: false),
                Reason = null,
                EvidenceFile = null,
            };
        }

        EventBase? MapUpdatedEvent(EntityVersionInfo<dfeta_sanction> snapshot, EntityVersionInfo<dfeta_sanction> previous)
        {
            if (snapshot.ChangedAttributes.Contains(dfeta_sanction.Fields.StateCode))
            {
                var nonStateAttributes = snapshot.ChangedAttributes
                    .Where(a => !(a is dfeta_sanction.Fields.StateCode or dfeta_sanction.Fields.StatusCode))
                    .ToArray();

                if (nonStateAttributes.Length > 0)
                {
                    throw new InvalidOperationException(
                        $"Expected state and status attributes to change in isolation but also received: {string.Join(", ", nonStateAttributes)}.");
                }

                if (snapshot.Entity.StateCode == dfeta_sanctionState.Inactive)
                {
                    return new AlertDqtDeactivatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        Key = $"{snapshot.Id}",  // The CRM Audit ID
                        CreatedUtc = snapshot.Timestamp,
                        RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                        PersonId = snapshot.Entity.dfeta_PersonId.Id,
                        Alert = GetEventAlert(snapshot.Entity, applyMigrationMappings: false)
                    };
                }
                else
                {
                    return new AlertDqtReactivatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        Key = $"{snapshot.Id}",  // The CRM Audit ID
                        CreatedUtc = snapshot.Timestamp,
                        RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                        PersonId = snapshot.Entity.dfeta_PersonId.Id,
                        Alert = GetEventAlert(snapshot.Entity, applyMigrationMappings: false)
                    };
                }
            }

            var changes = AlertUpdatedEventChanges.None |
                (snapshot.ChangedAttributes.Contains(dfeta_sanction.Fields.dfeta_SanctionDetails) ? AlertUpdatedEventChanges.Details : 0) |
                (snapshot.ChangedAttributes.Contains(dfeta_sanction.Fields.dfeta_DetailsLink) ? AlertUpdatedEventChanges.ExternalLink : 0) |
                (snapshot.ChangedAttributes.Contains(dfeta_sanction.Fields.dfeta_StartDate) ? AlertUpdatedEventChanges.StartDate : 0) |
                (snapshot.ChangedAttributes.Contains(dfeta_sanction.Fields.dfeta_EndDate) ? AlertUpdatedEventChanges.EndDate : 0) |
                (snapshot.ChangedAttributes.Contains(dfeta_sanction.Fields.dfeta_Spent) ? AlertUpdatedEventChanges.DqtSpent : 0) |
                (snapshot.ChangedAttributes.Contains(dfeta_sanction.Fields.dfeta_SanctionCodeId) ? AlertUpdatedEventChanges.DqtSanctionCode : 0);

            if (changes == AlertUpdatedEventChanges.None)
            {
                return null;
            }

            return new AlertUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Id}",  // The CRM Audit ID
                CreatedUtc = snapshot.Timestamp,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                PersonId = snapshot.Entity.dfeta_PersonId.Id,
                Alert = GetEventAlert(snapshot.Entity, applyMigrationMappings: false),
                OldAlert = GetEventAlert(previous.Entity, applyMigrationMappings: false),
                ChangeReason = null,
                EvidenceFile = null,
                Changes = changes
            };
        }

        EventBase MapImportedEvent(EntityVersionInfo<dfeta_sanction> snapshot)
        {
            return new AlertDqtImportedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Entity.Id}-Imported",
                CreatedUtc = snapshot.Timestamp,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                PersonId = snapshot.Entity.dfeta_PersonId.Id,
                Alert = GetEventAlert(snapshot.Entity, applyMigrationMappings: false),
                DqtState = (int)snapshot.Entity.StateCode!
            };
        }

        EventBase MapMigratedEvent(EntityVersionInfo<dfeta_sanction> snapshot)
        {
            return new AlertMigratedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Entity.Id}-Migrated",
                CreatedUtc = clock.UtcNow,
                RaisedBy = EventModels.RaisedByUserInfo.FromUserId(Core.DataStore.Postgres.Models.SystemUser.SystemUserId),
                PersonId = snapshot.Entity.dfeta_PersonId.Id,
                Alert = GetEventAlert(snapshot.Entity, applyMigrationMappings: true)
            };
        }

        EventModels.Alert GetEventAlert(dfeta_sanction snapshot, bool applyMigrationMappings)
        {
            var dqtSanctionCode = snapshot.dfeta_SanctionCodeId is null ?
                null :
                sanctionCodes.Single(s => s.Id == snapshot.dfeta_SanctionCodeId.Id);

            var alertType = applyMigrationMappings && dqtSanctionCode is not null ?
                alertTypes.Single(t => t.DqtSanctionCode == dqtSanctionCode.dfeta_Value) :
                null;

            return new()
            {
                AlertId = snapshot.Id,
                AlertTypeId = alertType?.AlertTypeId,
                Details = snapshot.dfeta_SanctionDetails,
                ExternalLink = snapshot.dfeta_DetailsLink,
                StartDate = snapshot.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                EndDate = snapshot.dfeta_EndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                DqtSpent = applyMigrationMappings ? null : snapshot.dfeta_Spent,
                DqtSanctionCode = !applyMigrationMappings && dqtSanctionCode is not null ?
                    new EventModels.AlertDqtSanctionCode()
                    {
                        SanctionCodeId = dqtSanctionCode.Id,
                        Name = dqtSanctionCode.dfeta_name,
                        Value = dqtSanctionCode.dfeta_Value
                    } :
                    null
            };
        }
    }

    private record ModelTypeSyncInfo
    {
        public required string? CreateTempTableStatement { get; init; }
        public required string? CopyStatement { get; init; }
        public required string? InsertStatement { get; init; }
        public required string? DeleteStatement { get; init; }
        public required string? GetLastModifiedOnStatement { get; init; }
        public required string EntityLogicalName { get; init; }
        public required string[] AttributeNames { get; init; }
        public required Func<TrsDataSyncHelper, SyncEntitiesHandler> GetSyncHandler { get; init; }
    }

    private record ModelTypeSyncInfo<TModel> : ModelTypeSyncInfo
    {
        public required Action<NpgsqlBinaryImporter, TModel>? WriteRecord { get; init; }
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
        public const string Event = "Event";
        public const string Alert = "Alert";
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

using System.Diagnostics;
using System.Reactive.Subjects;
using System.ServiceModel;
using AngleSharp.Dom;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Services.Files;
using Entity = Microsoft.Xrm.Sdk.Entity;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace TeachingRecordSystem.Core.Services.TrsDataSync;

#pragma warning disable CS9113 // Parameter is unread.
public class TrsDataSyncHelper(
    NpgsqlDataSource trsDbDataSource,
    [FromKeyedServices(TrsDataSyncHelper.CrmClientName)] IOrganizationServiceAsync2 organizationService,
    ReferenceDataCache referenceDataCache,
    IClock clock,
    IAuditRepository auditRepository,
    ILogger<TrsDataSyncHelper> logger,
    IFileService fileService,
    IConfiguration configuration)
#pragma warning restore CS9113 // Parameter is unread.
{
    private delegate Task SyncEntitiesHandler(IReadOnlyCollection<Entity> entities, bool ignoreInvalid, bool dryRun, CancellationToken cancellationToken);

    public const string CrmClientName = "TrsDataSync";

    private const string NowParameterName = "@now";
    private const string IdsParameterName = "@ids";
    private const int MaxAuditRequestsPerBatch = 10;

    private IReadOnlyDictionary<string, ModelTypeSyncInfo> GetModelTypeSyncInfo() =>
        new Dictionary<string, ModelTypeSyncInfo>()
        {
            { ModelTypes.Person, GetModelTypeSyncInfoForPerson() }
        };

    private IReadOnlyDictionary<string, ModelTypeSyncInfo> AllModelTypeSyncInfo => GetModelTypeSyncInfo();

    private readonly ISubject<object[]> _syncedEntitiesSubject = new Subject<object[]>();

    public IObservable<object[]> GetSyncedEntitiesObservable() => _syncedEntitiesSubject;

    private bool IsFakeXrm { get; } = organizationService.GetType().FullName == "Castle.Proxies.ObjectProxy_2";

    public (string EntityLogicalName, string[] AttributeNames) GetEntityInfoForModelType(string modelType)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);
        return (modelTypeSyncInfo.EntityLogicalName, modelTypeSyncInfo.AttributeNames);
    }

    public async Task SyncAuditAsync(
        string entityLogicalName,
        IEnumerable<Guid> ids,
        bool skipIfExists,
        CancellationToken cancellationToken = default)
    {
        var idsToSync = ids.ToList();

        if (skipIfExists)
        {
            var existingAudits = await Task.WhenAll(
                idsToSync.Select(async id => (Id: id, HaveAudit: await auditRepository.HaveAuditDetailAsync(entityLogicalName, id))));

            foreach (var (id, _) in existingAudits.Where(t => t.HaveAudit))
            {
                idsToSync.Remove(id);
            }
        }

        if (idsToSync.Count == 0)
        {
            return;
        }

        var audits = await GetAuditRecordsAsync(entityLogicalName, idsToSync, cancellationToken);
        await Task.WhenAll(audits.Select(async kvp => await auditRepository.SetAuditDetailAsync(entityLogicalName, kvp.Key, kvp.Value)));
    }

    public async Task DeleteRecordsAsync(string modelType, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return;
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);

        if (modelTypeSyncInfo.DeleteStatement is null)
        {
            if (!modelTypeSyncInfo.IgnoreDeletions)
            {
                throw new NotSupportedException($"Cannot delete a {modelType}.");
            }
            else
            {
                logger.LogWarning($"Ignoring deletion of {ids.Count} {modelType} records.");
                return;
            }
        }

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = modelTypeSyncInfo.DeleteStatement;
            cmd.Parameters.Add(new NpgsqlParameter(IdsParameterName, ids.ToArray()));
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task<DateTime?> GetLastModifiedOnForModelTypeAsync(string modelType)
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

    public Task SyncRecordsAsync(string modelType, IReadOnlyCollection<Entity> entities, bool ignoreInvalid, bool dryRun, CancellationToken cancellationToken = default)
    {
        if (entities.Count == 0)
        {
            return Task.CompletedTask;
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);
        return modelTypeSyncInfo.GetSyncHandler(this)(entities, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> SyncPersonsAsync(IReadOnlyCollection<Guid> contactIds, bool syncAudit, bool ignoreInvalid = false, bool dryRun = false, CancellationToken cancellationToken = default)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(ModelTypes.Person);

        var contacts = await GetEntitiesAsync<Contact>(
            Contact.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            contactIds,
            modelTypeSyncInfo.AttributeNames,
            activeOnly: false,
            cancellationToken);

        return await SyncPersonsAsync(contacts, syncAudit, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<bool> SyncPersonAsync(Guid contactId, bool syncAudit, bool ignoreInvalid = false, bool dryRun = false, CancellationToken cancellationToken = default) =>
        (await SyncPersonsAsync([contactId], syncAudit, ignoreInvalid, dryRun, cancellationToken)).Count() == 1;

    public async Task<bool> SyncPersonAsync(Contact entity, bool syncAudit, bool ignoreInvalid, bool dryRun = false, CancellationToken cancellationToken = default) =>
        (await SyncPersonsAsync([entity], syncAudit, ignoreInvalid, dryRun, cancellationToken)).Count() == 1;

    public async Task<IReadOnlyCollection<Guid>> SyncPersonsAsync(
        IReadOnlyCollection<Contact> entities,
        bool syncAudit,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        if (syncAudit)
        {
            await SyncAuditAsync(Contact.EntityLogicalName, entities.Select(q => q.ContactId!.Value), skipIfExists: false, cancellationToken);
        }

        var auditDetails = await GetAuditRecordsFromAuditRepositoryAsync(Contact.EntityLogicalName, Contact.PrimaryIdAttribute, entities.Select(q => q.ContactId!.Value), cancellationToken);
        return await SyncPersonsAsync(entities, auditDetails, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> SyncPersonsAsync(
        IReadOnlyCollection<Contact> entities,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        var (persons, events) = MapPersonsAndAudits(entities, auditDetails, ignoreInvalid);
        return await SyncPersonsAsync(persons, events, ignoreInvalid, dryRun, cancellationToken);
    }

    private async Task<IReadOnlyCollection<Guid>> SyncPersonsAsync(
        IReadOnlyCollection<PersonInfo> persons,
        IReadOnlyCollection<EventBase> events,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        var toSync = persons.ToList();

        if (ignoreInvalid)
        {
            // Some bad data in the build environment has a TRN that's longer than 7 digits
            toSync = toSync.Where(p => string.IsNullOrEmpty(p.Trn) || p.Trn.Length == 7).ToList();
        }

        if (!toSync.Any())
        {
            return [];
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo<PersonInfo>(ModelTypes.Person);

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);
        await using var txn = await connection.BeginTransactionAsync(cancellationToken);

        using (var createTempTableCommand = connection.CreateCommand())
        {
            createTempTableCommand.CommandText = modelTypeSyncInfo.CreateTempTableStatement;
            createTempTableCommand.Transaction = txn;
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        using var writer = await connection.BeginBinaryImportAsync(modelTypeSyncInfo.CopyStatement!, cancellationToken);

        foreach (var person in toSync)
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
                mergeCommand.CommandText = modelTypeSyncInfo.UpsertStatement;
                mergeCommand.Parameters.Add(new NpgsqlParameter(NowParameterName, clock.UtcNow));
                mergeCommand.Transaction = txn;
                await mergeCommand.ExecuteNonQueryAsync();
            }
        }
        catch (PostgresException ex) when (ex.SqlState == "23503" && ex.ConstraintName == "fk_persons_persons_merged_with_person_id")
        {
            // Trying to insert a record that has been merged with another person record which itself has not been synced yet!!
            // ex.Detail will be something like "Key (merged_with_person_id)=(5f0d1146-1836-eb11-a813-000d3a2287a4) is not present in table "persons"."
            var unsyncedPersonId = Guid.Parse(ex.Detail!.Substring("Key (merged_with_person_id)=(".Length, 36));
            await txn.DisposeAsync();
            await connection.DisposeAsync();

            var personSynced = await SyncPersonAsync(unsyncedPersonId, syncAudit: true, ignoreInvalid, dryRun: false, cancellationToken);

            if (personSynced)
            {
                // Retry syncing the original person record
                return await SyncPersonsAsync(toSync, events, ignoreInvalid, dryRun, cancellationToken);
            }
            else
            {
                // If we failed to sync the person record, we cannot continue.
                throw new InvalidOperationException($"Failed to sync person with ID {unsyncedPersonId} which is required for syncing the original person record.");
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

            var personsExceptFailedOne = persons.Where(e => e.Trn != trn).ToArray();
            var eventsExceptFailedOne = events.Where(e => e is not IEventWithPersonId || ((IEventWithPersonId)e).PersonId != personsExceptFailedOne[0].PersonId).ToArray();

            // Be extra sure we've actually removed a record (otherwise we'll go in an endless loop and stack overflow)
            if (!(personsExceptFailedOne.Length < persons.Count))
            {
                Debug.Fail("No persons removed from collection.");
                throw;
            }

            await txn.DisposeAsync();
            await connection.DisposeAsync();

            return await SyncPersonsAsync(personsExceptFailedOne, eventsExceptFailedOne, ignoreInvalid, dryRun, cancellationToken);
        }

        await txn.SaveEventsAsync(events, "events_person_import", clock, cancellationToken, timeoutSeconds: 120);

        if (!dryRun)
        {
            await txn.CommitAsync(cancellationToken);
        }

        _syncedEntitiesSubject.OnNext([.. toSync, .. events]);
        return toSync.Select(p => p.PersonId).ToArray();
    }

    private EntityVersionInfo<TEntity>[] GetEntityVersions<TEntity>(TEntity latest, IEnumerable<AuditDetail> auditDetails, string[] attributeNames)
        where TEntity : Entity
    {
        if (!latest.TryGetAttributeValue<DateTime?>("createdon", out var createdOn) || !createdOn.HasValue)
        {
            throw new ArgumentException($"Expected {latest.LogicalName} entity with ID {latest.Id} to have a non-null 'createdon' attribute value.", nameof(latest));
        }

        var created = createdOn.Value;
        var createdBy = latest.GetAttributeValue<EntityReference>("createdby");

        var ordered = auditDetails
            .OfType<AttributeAuditDetail>()
            .Select(a => (AuditDetail: a, AuditRecord: a.AuditRecord.ToEntity<Audit>()))
            .OrderBy(a => a.AuditRecord.CreatedOn)
            .ThenBy(a => a.AuditRecord.Action == Audit_Action.Create ? 0 : 1)
            .ToArray();

        if (ordered.Length == 0)
        {
            return [new EntityVersionInfo<TEntity>(latest.Id, latest, ChangedAttributes: [], created, createdBy.Id, createdBy.Name)];
        }

        var versions = new List<EntityVersionInfo<TEntity>>();

        var initialVersion = GetInitialVersion();
        versions.Add(new EntityVersionInfo<TEntity>(initialVersion.Id, initialVersion, ChangedAttributes: [], created, createdBy.Id, createdBy.Name));

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
                if (!attributeNames.Contains(attr.Value))
                {
                    continue;
                }

                thisVersion.Attributes.Remove(attr.Value);
                changedAttributes.Add(attr.Value);
            }

            foreach (var attr in audit.AuditDetail.NewValue.Attributes)
            {
                if (!attributeNames.Contains(attr.Key))
                {
                    continue;
                }

                thisVersion.Attributes[attr.Key] = attr.Value;
                changedAttributes.Add(attr.Key);
            }

            if (changedAttributes.Count == 0)
            {
                continue;
            }

            versions.Add(new EntityVersionInfo<TEntity>(
                audit.AuditRecord.Id,
                thisVersion.SparseClone(attributeNames),
                changedAttributes.ToArray(),
                audit.AuditRecord.CreatedOn!.Value,
                audit.AuditRecord.UserId.Id,
                audit.AuditRecord.UserId.Name));

            latest = thisVersion;
        }

        return versions.ToArray();

        TEntity GetInitialVersion()
        {
            TEntity? initial;
            if (ordered[0] is { AuditRecord: { Action: Audit_Action.Create } } createAction)
            {
                initial = createAction.AuditDetail.NewValue.ToEntity<TEntity>();
                initial.Id = latest.Id;
            }
            else
            {
                // Starting with `latest`, go through each event in reverse and undo the changes it applied.
                // When we're done we end up with the initial version of the record.
                initial = latest.ShallowClone();

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
            }

            return initial.SparseClone(attributeNames);
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

    private ModelTypeSyncInfo<TModel> GetModelTypeSyncInfo<TModel>(string modelType) =>
        (ModelTypeSyncInfo<TModel>)GetModelTypeSyncInfo(modelType);

    private ModelTypeSyncInfo GetModelTypeSyncInfo(string modelType) =>
        AllModelTypeSyncInfo.TryGetValue(modelType, out var modelTypeSyncInfo) ?
            modelTypeSyncInfo :
            throw new ArgumentException($"Unknown data type: '{modelType}.", nameof(modelType));

    private async Task<IReadOnlyDictionary<Guid, AuditDetailCollection>> GetAuditRecordsAsync(
        string entityLogicalName,
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (IsFakeXrm)
        {
            return new Dictionary<Guid, AuditDetailCollection>();
        }

        // Throttle the amount of concurrent requests
        using var requestThrottle = new SemaphoreSlim(20, 20);

        // Keep track of the last seen 'retry-after' value
        var retryDelayUpdateLock = new object();
        var retryDelay = Task.Delay(0, cancellationToken);

        void UpdateRetryDelay(TimeSpan ts)
        {
            lock (retryDelayUpdateLock)
            {
                retryDelay = Task.Delay(ts, cancellationToken);
            }
        }

        return (await Task.WhenAll(ids
            .Chunk(MaxAuditRequestsPerBatch)
            .Select(async chunk =>
            {
                var request = new ExecuteMultipleRequest()
                {
                    Requests = [],
                    Settings = new()
                    {
                        ContinueOnError = false,
                        ReturnResponses = true
                    }
                };

                // The following is not supported by FakeXrmEasy hence the check above to allow more test coverage
                request.Requests.AddRange(chunk.Select(e => new RetrieveRecordChangeHistoryRequest() { Target = e.ToEntityReference(entityLogicalName) }));

                ExecuteMultipleResponse response;
                while (true)
                {
                    await retryDelay;
                    await requestThrottle.WaitAsync(cancellationToken);
                    try
                    {
                        response = (ExecuteMultipleResponse)await organizationService.ExecuteAsync(request, cancellationToken);
                    }
                    catch (FaultException fex) when (fex.IsCrmRateLimitException(out var retryAfter))
                    {
                        logger.LogWarning("Hit CRM service limits getting {entityLogicalName} audit records; Fault exception. Retrying after {retryAfter} seconds.", entityLogicalName, retryAfter.TotalSeconds);
                        UpdateRetryDelay(retryAfter);
                        continue;
                    }
                    finally
                    {
                        requestThrottle.Release();
                    }

                    if (response.IsFaulted)
                    {
                        var firstFault = response.Responses.First(r => r.Fault is not null).Fault;

                        if (firstFault.IsCrmRateLimitFault(out var retryAfter))
                        {
                            logger.LogWarning("Hit CRM service limits getting {entityLogicalName} audit records; CRM rate limit fault. Retrying after {retryAfter} seconds.", entityLogicalName, retryAfter.TotalSeconds);
                            UpdateRetryDelay(retryAfter);
                            continue;
                        }
                        else if (firstFault.Message.Contains("The HTTP status code of the response was not expected (429)"))
                        {
                            logger.LogWarning("Hit CRM service limits getting {entityLogicalName} audit records; 429 too many requests", entityLogicalName);
                            UpdateRetryDelay(TimeSpan.FromMinutes(2));
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

    private async Task<IReadOnlyDictionary<Guid, AuditDetailCollection>> GetAuditRecordsFromAuditRepositoryAsync(
        string entityLogicalName,
        string primaryIdAttribute,
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken)
    {
        var auditRecords = await Task.WhenAll(
            ids.Select(async id =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                int retryCount = 0;
                AuditDetailCollection? audit = null;
                while (true)
                {
                    audit = await auditRepository.GetAuditDetailAsync(entityLogicalName, primaryIdAttribute, id);
                    if (audit is not null || retryCount > 0)
                    {
                        break;
                    }

                    // Try and sync the audit record from CRM if it doesn't exist in the audit repository
                    await SyncAuditAsync(entityLogicalName, [id], skipIfExists: false, cancellationToken);
                    retryCount++;
                }

                if (audit is null)
                {
                    throw new Exception($"No audit detail for {entityLogicalName} with id '{id}'.");
                }

                return (Id: id, Audit: audit);
            }));

        return auditRecords.ToDictionary(a => a.Id, a => a.Audit);
    }

    private async Task<TEntity[]> GetEntitiesAsync<TEntity>(
        string entityLogicalName,
        string idAttributeName,
        IEnumerable<Guid> ids,
        string[] attributeNames,
        bool activeOnly,
        CancellationToken cancellationToken)
        where TEntity : Entity
    {
        var query = new QueryExpression(entityLogicalName);
        query.ColumnSet = new(attributeNames);
        query.Criteria.AddCondition(idAttributeName, ConditionOperator.In, ids.Cast<object>().ToArray());
        if (activeOnly)
        {
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
        }

        EntityCollection response;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                response = await organizationService.RetrieveMultipleAsync(query, cancellationToken);
            }
            catch (FaultException<OrganizationServiceFault> fex) when (fex.IsCrmRateLimitException(out var retryAfter))
            {
                logger.LogWarning("Hit CRM service limits; error code: {ErrorCode}", fex.Detail.ErrorCode);
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            break;
        }

        return response.Entities.Select(e => e.ToEntity<TEntity>()).ToArray();
    }

    private ModelTypeSyncInfo GetModelTypeSyncInfoForPerson()
    {
        var tempTableName = "temp_person_import";
        var tableName = "persons";

        var columnNames = new[]
        {
            "person_id",
            "created_on",
            "updated_on",
            "status",
            "merged_with_person_id",
            "trn",
            "first_name",
            "middle_name",
            "last_name",
            "date_of_birth",
            "email_address",
            "national_insurance_number",
            "gender",
            "dqt_contact_id",
            "dqt_state",
            "dqt_created_on",
            "dqt_modified_on",
            "dqt_first_name",
            "dqt_middle_name",
            "dqt_last_name",
            "created_by_tps",
            "source_application_user_id",
            "source_trn_request_id",
            "allow_details_updates_from_source_application",
            "dqt_allow_teacher_identity_sign_in_with_prohibitions",
            "date_of_death"
        };

        var columnsToUpdate = columnNames.Except(["person_id", "dqt_contact_id"]).ToArray();

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement = $"CREATE TEMP TABLE {tempTableName} (LIKE {tableName} INCLUDING DEFAULTS)";

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var insertStatement =
            $"""
            INSERT INTO {tableName} AS t ({columnList}, dqt_first_sync, dqt_last_sync)
            SELECT {columnList}, {NowParameterName}, {NowParameterName} FROM {tempTableName}
            ON CONFLICT (person_id) DO UPDATE
            SET dqt_last_sync = {NowParameterName}, {string.Join(", ", columnsToUpdate.Select(c => $"{c} = EXCLUDED.{c}"))}
            """;

        var deleteStatement = $"DELETE FROM {tableName} WHERE dqt_contact_id = ANY({IdsParameterName})";

        var getLastModifiedOnStatement = $"SELECT MAX(dqt_modified_on) FROM {tableName}";

        var attributeNames = new[]
        {
            Contact.PrimaryIdAttribute,
            Contact.Fields.StateCode,
            Contact.Fields.CreatedOn,
            Contact.Fields.CreatedBy,
            Contact.Fields.ModifiedOn,
            Contact.Fields.dfeta_TRN,
            Contact.Fields.FirstName,
            Contact.Fields.MiddleName,
            Contact.Fields.LastName,
            Contact.Fields.dfeta_StatedFirstName,
            Contact.Fields.dfeta_StatedMiddleName,
            Contact.Fields.dfeta_StatedLastName,
            Contact.Fields.BirthDate,
            Contact.Fields.EMailAddress1,
            Contact.Fields.dfeta_NINumber,
            Contact.Fields.MobilePhone,
            Contact.Fields.GenderCode,
            Contact.Fields.dfeta_InductionStatus,
            Contact.Fields.dfeta_MergedWith,
            Contact.Fields.dfeta_CapitaTRNChangedOn,
            Contact.Fields.dfeta_TrnRequestID,
            Contact.Fields.dfeta_AllowPiiUpdatesFromRegister,
            Contact.Fields.dfeta_AllowIDSignInWithProhibitions,
            Contact.Fields.dfeta_DateofDeath
        };

        Action<NpgsqlBinaryImporter, PersonInfo> writeRecord = (writer, person) =>
        {
            writer.WriteValueOrNull(person.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(person.CreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(person.UpdatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull((int)person.Status, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(person.MergedWithPersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(person.Trn, NpgsqlDbType.Char);
            writer.WriteValueOrNull(person.FirstName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.MiddleName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.LastName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.DateOfBirth, NpgsqlDbType.Date);
            writer.WriteValueOrNull(person.EmailAddress, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.NationalInsuranceNumber, NpgsqlDbType.Char);
            writer.WriteValueOrNull((int?)person.Gender, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(person.DqtContactId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(person.DqtState, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(person.DqtCreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(person.DqtModifiedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(person.DqtFirstName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.DqtMiddleName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.DqtLastName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.CreatedByTps, NpgsqlDbType.Boolean);
            writer.WriteValueOrNull(person.SourceApplicationUserId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(person.SourceTrnRequestId, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.AllowDetailsUpdatesFromSourceApplication, NpgsqlDbType.Boolean);
            writer.WriteValueOrNull(person.DqtAllowTeacherIdentitySignInWithProhibitions, NpgsqlDbType.Boolean);
            writer.WriteValueOrNull(person.DateOfDeath, NpgsqlDbType.Date);
        };

        return new ModelTypeSyncInfo<PersonInfo>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            UpsertStatement = insertStatement,
            DeleteStatement = deleteStatement,
            IgnoreDeletions = false,
            GetLastModifiedOnStatement = getLastModifiedOnStatement,
            EntityLogicalName = Contact.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncPersonsAsync(entities.Select(e => e.ToEntity<Contact>()).ToArray(), syncAudit: true, ignoreInvalid, dryRun, ct),
            WriteRecord = writeRecord
        };
    }

    private (List<PersonInfo> Persons, List<EventBase> Events) MapPersonsAndAudits(
        IReadOnlyCollection<Contact> contacts,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid)
    {
        var events = new List<EventBase>();
        var persons = MapPersons(contacts);

        return (persons, events);
    }

    public static List<PersonInfo> MapPersons(IEnumerable<Contact> contacts) => contacts
        .Select(c => new PersonInfo()
        {
            PersonId = c.ContactId!.Value,
            CreatedOn = c.CreatedOn!.Value,
            UpdatedOn = c.ModifiedOn!.Value,
            Status = c.StateCode == ContactState.Active ? PersonStatus.Active : PersonStatus.Deactivated,
            MergedWithPersonId = c.dfeta_MergedWith?.Id,
            Trn = c.dfeta_TRN,
            FirstName = (c.HasStatedNames() ? c.dfeta_StatedFirstName : c.FirstName) ?? string.Empty,
            MiddleName = (c.HasStatedNames() ? c.dfeta_StatedMiddleName : c.MiddleName) ?? string.Empty,
            LastName = (c.HasStatedNames() ? c.dfeta_StatedLastName : c.LastName) ?? string.Empty,
            DateOfBirth = c.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            EmailAddress = c.EMailAddress1.NormalizeString(),
            NationalInsuranceNumber = c.dfeta_NINumber.NormalizeString(),
            Gender = c.GenderCode.ToGender(),
            DqtContactId = c.Id,
            DqtState = (int)c.StateCode!,
            DqtCreatedOn = c.CreatedOn!.Value,
            DqtModifiedOn = c.ModifiedOn!.Value,
            DqtFirstName = c.FirstName ?? string.Empty,
            DqtMiddleName = c.MiddleName ?? string.Empty,
            DqtLastName = c.LastName ?? string.Empty,
            CreatedByTps = c.dfeta_CapitaTRNChangedOn == null ? true : false,
            SourceApplicationUserId = c.dfeta_TrnRequestID is not null ? Guid.Parse(c.dfeta_TrnRequestID[..Guid.Empty.ToString().Length]) : null,
            SourceTrnRequestId = c.dfeta_TrnRequestID?[(Guid.Empty.ToString().Length + 2)..],
            AllowDetailsUpdatesFromSourceApplication = c.dfeta_AllowPiiUpdatesFromRegister == true,
            DqtAllowTeacherIdentitySignInWithProhibitions = c.dfeta_AllowIDSignInWithProhibitions == true,
            DateOfDeath = c.dfeta_DateofDeath?.ToDateOnlyWithDqtBstFix(isLocalTime: true)
        })
        .ToList();

    private async Task BulkSaveEventsAsync(
        IReadOnlyCollection<EventBase> events,
        string tempTableSuffix,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);
        await using var txn = await connection.BeginTransactionAsync(cancellationToken);
        await txn.SaveEventsAsync(events, tempTableSuffix, clock, cancellationToken, timeoutSeconds: 120);
        if (!dryRun)
        {
            await txn.CommitAsync(cancellationToken);
        }
        else
        {
            await txn.RollbackAsync(cancellationToken);
        }
    }

    private record ModelTypeSyncInfo
    {
        public required string? CreateTempTableStatement { get; init; }
        public required string? CopyStatement { get; init; }
        public required string? UpsertStatement { get; init; }
        public required string? DeleteStatement { get; init; }
        public required bool IgnoreDeletions { get; init; }
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

    public record PersonInfo
    {
        public required Guid PersonId { get; init; }
        public required DateTime? CreatedOn { get; init; }
        public required DateTime? UpdatedOn { get; init; }
        public required PersonStatus Status { get; init; }
        public required Guid? MergedWithPersonId { get; init; }
        public required string? Trn { get; init; }
        public required string FirstName { get; init; }
        public required string MiddleName { get; init; }
        public required string LastName { get; init; }
        public required DateOnly? DateOfBirth { get; init; }
        public required string? EmailAddress { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required Gender? Gender { get; init; }
        public required Guid? DqtContactId { get; init; }
        public required int? DqtState { get; init; }
        public required DateTime? DqtCreatedOn { get; init; }
        public required DateTime? DqtModifiedOn { get; init; }
        public required string? DqtFirstName { get; init; }
        public required string? DqtMiddleName { get; init; }
        public required string? DqtLastName { get; init; }
        public required bool CreatedByTps { get; init; }
        public required Guid? SourceApplicationUserId { get; init; }
        public required string? SourceTrnRequestId { get; init; }
        public required bool AllowDetailsUpdatesFromSourceApplication { get; init; }
        public required bool DqtAllowTeacherIdentitySignInWithProhibitions { get; init; }
        public required DateOnly? DateOfDeath { get; set; }
    }

    private record AuditInfo<TEntity>
    {
        public required string[] AllChangedAttributes { get; init; }
        public required string[] RelevantChangedAttributes { get; init; }
        public required TEntity NewValue { get; init; }
        public required TEntity OldValue { get; init; }
        public required Audit AuditRecord { get; init; }
    }

    public static class ModelTypes
    {
        public const string Person = "Person";
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

    public static TEntity SparseClone<TEntity>(this TEntity entity, string[] attributeNames) where TEntity : Entity
    {
        // N.B. This only clones Attributes in the whitelist
        var cloned = new Entity(entity.LogicalName, entity.Id);

        foreach (var attr in entity.Attributes.Where(kvp => attributeNames.Contains(kvp.Key)))
        {
            cloned.Attributes.Add(attr.Key, attr.Value);
        }

        return cloned.ToEntity<TEntity>();
    }

    /// <summary>
    /// Returns <c>null</c> if <paramref name="value"/> is empty or whitespace.
    /// </summary>
    public static string? NormalizeString(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static readonly TimeZoneInfo _gmt = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

    public static DateTime ToDateTimeWithDqtBstFix(this DateTime dateTime, bool isLocalTime) =>
        isLocalTime ? TimeZoneInfo.ConvertTimeFromUtc(dateTime, _gmt) : dateTime;
}

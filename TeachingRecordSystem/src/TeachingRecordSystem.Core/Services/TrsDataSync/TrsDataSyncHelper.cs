using System.Diagnostics;
using System.Reactive.Subjects;
using System.ServiceModel;
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
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Services.TrsDataSync;

public class TrsDataSyncHelper(
    NpgsqlDataSource trsDbDataSource,
    [FromKeyedServices(TrsDataSyncService.CrmClientName)] IOrganizationServiceAsync2 organizationService,
#pragma warning disable CS9113 // Parameter is unread.
    ReferenceDataCache referenceDataCache,
#pragma warning restore CS9113 // Parameter is unread.
    IClock clock,
    IAuditRepository auditRepository,
    ILogger<TrsDataSyncHelper> logger)
{
    private delegate Task SyncEntitiesHandler(IReadOnlyCollection<Entity> entities, bool ignoreInvalid, bool dryRun, CancellationToken cancellationToken);

    private const string NowParameterName = "@now";
    private const string IdsParameterName = "@ids";
    private const int MaxAuditRequestsPerBatch = 10;

    private static readonly IReadOnlyDictionary<string, ModelTypeSyncInfo> _modelTypeSyncInfo = new Dictionary<string, ModelTypeSyncInfo>()
    {
        { ModelTypes.Person, GetModelTypeSyncInfoForPerson() },
        { ModelTypes.Event, GetModelTypeSyncInfoForEvent() },
        { ModelTypes.Induction, GetModelTypeSyncInfoForInduction() }
    };

    private readonly ISubject<object[]> _syncedEntitiesSubject = new Subject<object[]>();

    public IObservable<object[]> GetSyncedEntitiesObservable() => _syncedEntitiesSubject;

    private bool IsFakeXrm { get; } = organizationService.GetType().FullName == "Castle.Proxies.ObjectProxy_2";

    public static (string EntityLogicalName, string[] AttributeNames) GetEntityInfoForModelType(string modelType)
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

    private InductionInfo? MapInductionInfoFromDqtInduction(
        dfeta_induction? induction,
        Contact contact,
        bool ignoreInvalid)
    {
        // Double check that contact record induction status matches the induction record (if there is one) induction status (which should have been set via CRM plugin)
        var hasQtls = contact.dfeta_qtlsdate is not null;
        if (induction is not null && induction.dfeta_InductionStatus != contact.dfeta_InductionStatus)
        {
            var errorMessage = $"Induction status {contact.dfeta_InductionStatus} for contact {contact.ContactId} does not match induction status {induction.dfeta_InductionStatus} for induction {induction!.dfeta_inductionId}.";
            if (ignoreInvalid)
            {
                logger.LogWarning(errorMessage);
                return null;
            }

            throw new InvalidOperationException(errorMessage);
        }
        // Person with QTLS should be exempt from induction
        else if (hasQtls && contact.dfeta_InductionStatus != dfeta_InductionStatus.Exempt)
        {
            var errorMessage = $"Induction status for contact {contact.ContactId} with QTLS should be {dfeta_InductionStatus.Exempt} but is {contact.dfeta_InductionStatus}.";
            if (ignoreInvalid)
            {
                logger.LogWarning(errorMessage);
                return null;
            }

            throw new InvalidOperationException(errorMessage);
        }

        return new InductionInfo()
        {
            PersonId = contact.ContactId!.Value,
            InductionStatus = contact.dfeta_InductionStatus.ToInductionStatus(),
            InductionStartDate = induction?.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            InductionCompletedDate = induction?.dfeta_CompletionDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            InductionExemptionReasonIds = [], // this mapping will be done in a future PR
            DqtModifiedOn = induction?.ModifiedOn
        };
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
        (await SyncPersonsAsync(new[] { entity }, syncAudit, ignoreInvalid, dryRun, cancellationToken)).Count() == 1;

    public async Task<IReadOnlyCollection<Guid>> SyncPersonsAsync(
        IReadOnlyCollection<Contact> entities,
        bool syncAudit,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        // For the moment we are just making sure we have the audit history in TRS - we will amend to generate events as part of the contact migration work
        if (syncAudit)
        {
            await SyncAuditAsync(Contact.EntityLogicalName, entities.Select(q => q.ContactId!.Value), skipIfExists: false, cancellationToken);
        }

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
            return [];
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
                mergeCommand.CommandText = modelTypeSyncInfo.UpsertStatement;
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

            return await SyncPersonsAsync(entitiesExceptFailedOne, syncAudit, ignoreInvalid, dryRun, cancellationToken);
        }

        if (!dryRun)
        {
            await txn.CommitAsync(cancellationToken);
        }

        _syncedEntitiesSubject.OnNext(people.ToArray());
        return people.Select(p => p.PersonId).ToArray();
    }

    public async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<dfeta_induction> inductions,
        bool syncAudit,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var contactAttributeNames = GetModelTypeSyncInfo(ModelTypes.Induction).AttributeNames;

        var contacts = await GetEntitiesAsync<Contact>(
            Contact.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            inductions.Select(e => e.dfeta_PersonId.Id),
            contactAttributeNames,
            activeOnly: false,
            cancellationToken);

        return await SyncInductionsAsync(contacts, inductions, syncAudit, ignoreInvalid, createMigratedEvent: false, dryRun, cancellationToken);
    }

    public async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<Contact> contacts,
        bool syncAudit,
        bool ignoreInvalid,
        bool createMigratedEvent,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        var inductionAttributeNames = new[]
        {
            dfeta_induction.Fields.dfeta_PersonId,
            dfeta_induction.Fields.dfeta_CompletionDate,
            dfeta_induction.Fields.dfeta_InductionExemptionReason,
            dfeta_induction.Fields.dfeta_StartDate,
            dfeta_induction.Fields.dfeta_InductionStatus,
            dfeta_induction.Fields.CreatedOn,
            dfeta_induction.Fields.CreatedBy,
            dfeta_induction.Fields.ModifiedOn,
            dfeta_induction.Fields.StateCode
        };

        var inductions = await GetEntitiesAsync<dfeta_induction>(
            dfeta_induction.EntityLogicalName,
            dfeta_induction.Fields.dfeta_PersonId,
            contacts.Select(c => c.ContactId!.Value),
            inductionAttributeNames,
            true,
            cancellationToken);

        return await SyncInductionsAsync(contacts, inductions, syncAudit, ignoreInvalid, createMigratedEvent, dryRun, cancellationToken);
    }

    public async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<Contact> contacts,
        IReadOnlyCollection<dfeta_induction> entities,
        bool syncAudit,
        bool ignoreInvalid,
        bool createMigratedEvent,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        if (syncAudit)
        {
            await SyncAuditAsync(Contact.EntityLogicalName, contacts.Select(q => q.ContactId!.Value), skipIfExists: false, cancellationToken);
            await SyncAuditAsync(dfeta_induction.EntityLogicalName, entities.Select(q => q.Id), skipIfExists: false, cancellationToken);
        }

        var contactAuditDetails = await GetAuditRecordsFromAuditRepositoryAsync(Contact.EntityLogicalName, Contact.PrimaryIdAttribute, contacts.Select(q => q.ContactId!.Value), cancellationToken);
        var inductionAuditDetails = await GetAuditRecordsFromAuditRepositoryAsync(dfeta_induction.EntityLogicalName, dfeta_induction.PrimaryIdAttribute, entities.Select(q => q.Id), cancellationToken);

        var auditDetails = contactAuditDetails
            .Concat(inductionAuditDetails)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return await SyncInductionsAsync(contacts, entities, auditDetails, ignoreInvalid, createMigratedEvent, dryRun, cancellationToken);
    }

    public async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<Contact> contacts,
        IReadOnlyCollection<dfeta_induction> entities,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid,
        bool createMigratedEvent,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var (inductions, events) = MapInductionsAndAudits(contacts, entities, auditDetails, ignoreInvalid, createMigratedEvent);
        return await SyncInductionsAsync(inductions, events, ignoreInvalid, createMigratedEvent, dryRun, cancellationToken);
    }

    private async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<InductionInfo> inductions,
        IReadOnlyCollection<EventBase> events,
        bool ignoreInvalid,
        bool createMigratedEvent,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo<InductionInfo>(ModelTypes.Induction);

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);

        var toSync = inductions.ToList();

        do
        {
            using var txn = await connection.BeginTransactionAsync(cancellationToken);

            using (var createTempTableCommand = connection.CreateCommand())
            {
                createTempTableCommand.CommandText = modelTypeSyncInfo.CreateTempTableStatement;
                createTempTableCommand.Transaction = txn;
                await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            using var writer = await connection.BeginBinaryImportAsync(modelTypeSyncInfo.CopyStatement!, cancellationToken);

            foreach (var i in toSync)
            {
                writer.StartRow();
                modelTypeSyncInfo.WriteRecord!(writer, i);
            }

            await writer.CompleteAsync(cancellationToken);
            await writer.CloseAsync(cancellationToken);

            var syncedInductionPersonIds = new List<Guid>();

            using (var mergeCommand = connection.CreateCommand())
            {
                mergeCommand.CommandText = modelTypeSyncInfo.UpsertStatement;
                mergeCommand.Parameters.Add(new NpgsqlParameter(NowParameterName, clock.UtcNow));
                mergeCommand.Transaction = txn;
                using var reader = await mergeCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync(cancellationToken))
                {
                    syncedInductionPersonIds.Add(reader.GetGuid(0));
                }
            }

            var unsyncedContactIds = toSync
                .Where(i => !syncedInductionPersonIds.Contains(i.PersonId))
                .Select(i => i.PersonId)
                .ToArray();

            if (unsyncedContactIds.Length > 0)
            {
                var personsSynced = await SyncPersonsAsync(unsyncedContactIds, syncAudit: true, ignoreInvalid, dryRun: false, cancellationToken);
                var unableToSyncContactIds = unsyncedContactIds.Where(id => !personsSynced.Contains(id)).ToArray();
                if (unableToSyncContactIds.Length > 0)
                {
                    var errorMessage = $"Attempted to sync Induction for persons but the Contact records with IDs [{string.Join(", ", unableToSyncContactIds)}] do not meet the sync criteria.";
                    if (ignoreInvalid)
                    {
                        logger.LogWarning(errorMessage);
                        toSync.RemoveAll(i => unableToSyncContactIds.Contains(i.PersonId));
                        if (toSync.Count == 0)
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(errorMessage);
                    }
                }

                continue;
            }

            var eventsForSyncedContacts = events
                .Where(e => e is IEventWithPersonId && !unsyncedContactIds.Any(c => c == ((IEventWithPersonId)e).PersonId))
                .ToArray();

            await txn.SaveEventsAsync(eventsForSyncedContacts, "events_import", clock, cancellationToken, timeoutSeconds: 120);

            if (!dryRun)
            {
                await txn.CommitAsync(cancellationToken);
            }

            break;
        }
        while (true);

        _syncedEntitiesSubject.OnNext([.. toSync, .. events]);
        return toSync.Count;
    }

    public async Task<int> SyncEventsAsync(IReadOnlyCollection<dfeta_TRSEvent> events, bool dryRun, CancellationToken cancellationToken = default)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo<EventInfo>(ModelTypes.Event);

        var mapped = events.Select(e => EventInfo.Deserialize(e.dfeta_Payload).Event).ToArray();

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);

        using var txn = await connection.BeginTransactionAsync(cancellationToken);
        await txn.SaveEventsAsync(mapped, tempTableSuffix: "events_import", clock, cancellationToken);

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

    private static ModelTypeSyncInfo<TModel> GetModelTypeSyncInfo<TModel>(string modelType) =>
        (ModelTypeSyncInfo<TModel>)GetModelTypeSyncInfo(modelType);

    private static ModelTypeSyncInfo GetModelTypeSyncInfo(string modelType) =>
        _modelTypeSyncInfo.TryGetValue(modelType, out var modelTypeSyncInfo) ?
            modelTypeSyncInfo :
            throw new ArgumentException($"Unknown data type: '{modelType}.", nameof(modelType));

    private async Task<IReadOnlyDictionary<Guid, AuditDetailCollection>> GetAuditRecordsAsync(
        string entityLogicalName,
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken)
    {
        return (IsFakeXrm ? new Dictionary<Guid, AuditDetailCollection>() : (await Task.WhenAll(ids
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

                // The following is not supported by FakeXrmEasy hence the check above to allow more test coverage
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
                        logger.LogWarning("Hit CRM service limits getting {entityLogicalName} audit records; Fault exception. Retrying after {retryAfter} seconds.", entityLogicalName, retryAfter.TotalSeconds);
                        await Task.Delay(retryAfter, cancellationToken);
                        continue;
                    }

                    if (response.IsFaulted)
                    {
                        var firstFault = response.Responses.First(r => r.Fault is not null).Fault;

                        if (firstFault.IsCrmRateLimitFault(out var retryAfter))
                        {
                            logger.LogWarning("Hit CRM service limits getting {entityLogicalName} audit records; CRM rate limit fault. Retrying after {retryAfter} seconds.", entityLogicalName, retryAfter.TotalSeconds);
                            await Task.Delay(retryAfter, cancellationToken);
                            continue;
                        }
                        else if (firstFault.Message.Contains("The HTTP status code of the response was not expected (429)"))
                        {
                            logger.LogWarning("Hit CRM service limits getting {entityLogicalName} audit records; 429 too many requests", entityLogicalName);
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
            .ToDictionary(t => t.Id, t => t.AuditDetailCollection));
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
                var audit = await auditRepository.GetAuditDetailAsync(entityLogicalName, primaryIdAttribute, id) ??
                    throw new Exception($"No audit detail for '{id}'.");
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
            "dqt_last_name",
            "induction_status",
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
            Contact.Fields.dfeta_NINumber,
            Contact.Fields.EMailAddress1,
            Contact.Fields.dfeta_InductionStatus
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
            writer.WriteValueOrNull((int?)person.InductionStatus, NpgsqlDbType.Integer);
        };

        return new ModelTypeSyncInfo<Person>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            UpsertStatement = insertStatement,
            DeleteStatement = deleteStatement,
            GetLastModifiedOnStatement = getLastModifiedOnStatement,
            EntityLogicalName = Contact.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncPersonsAsync(entities.Select(e => e.ToEntity<Contact>()).ToArray(), syncAudit: true, ignoreInvalid, dryRun, ct),
            WriteRecord = writeRecord
        };
    }

    private static ModelTypeSyncInfo GetModelTypeSyncInfoForInduction()
    {
        var tempTableName = "temp_induction_import";
        var tableName = "persons";

        var columnNames = new[]
        {
            "person_id",
            "induction_completed_date",
            "induction_exemption_reason_ids",
            "induction_start_date",
            "induction_status",
            "induction_modified_on",
            "dqt_induction_modified_on"
        };

        var columnsToUpdate = columnNames.Except(new[] { "person_id" }).ToArray();

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement =
            $"""
            CREATE TEMP TABLE {tempTableName}
            (
                person_id uuid NOT NULL,
                induction_completed_date date,
                induction_exemption_reason_ids uuid[],
                induction_start_date date,
                induction_status integer,
                induction_modified_on timestamp with time zone,
                dqt_induction_modified_on timestamp with time zone
            )
            """;

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var updateStatement =
            $"""
            UPDATE {tableName} AS t
            SET dqt_induction_last_sync = {NowParameterName}, {string.Join(", ", columnsToUpdate.Select(c => $"{c} = temp.{c}"))}
            FROM {tempTableName} AS temp
            WHERE t.person_id = temp.person_id
            RETURNING t.person_id
            """;

        var getLastModifiedOnStatement = $"SELECT MAX(dqt_induction_modified_on) FROM {tableName}";

        var attributeNames = new[]
        {
            Contact.PrimaryIdAttribute,
            Contact.Fields.dfeta_InductionStatus,
            Contact.Fields.dfeta_qtlsdate,
            Contact.Fields.CreatedOn,
            Contact.Fields.CreatedBy,
            Contact.Fields.StateCode,
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
            Contact.Fields.EMailAddress1,
        };

        Action<NpgsqlBinaryImporter, InductionInfo> writeRecord = (writer, induction) =>
        {
            writer.WriteValueOrNull(induction.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(induction.InductionCompletedDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull(induction.InductionExemptionReasonIds, NpgsqlDbType.Array | NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(induction.InductionStartDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull((int?)induction.InductionStatus, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(induction.DqtModifiedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(induction.DqtModifiedOn, NpgsqlDbType.TimestampTz);
        };

        return new ModelTypeSyncInfo<InductionInfo>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            UpsertStatement = updateStatement,
            DeleteStatement = null,
            GetLastModifiedOnStatement = getLastModifiedOnStatement,
            EntityLogicalName = dfeta_induction.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncInductionsAsync(entities.Select(e => e.ToEntity<dfeta_induction>()).ToArray(), syncAudit: true, ignoreInvalid, dryRun, ct),
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
            UpsertStatement = null,
            DeleteStatement = null,
            GetLastModifiedOnStatement = null,
            EntityLogicalName = dfeta_TRSEvent.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncEventsAsync(entities.Select(e => e.ToEntity<dfeta_TRSEvent>()).ToArray(), dryRun, ct),
            WriteRecord = null
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
            DqtLastName = c.LastName ?? string.Empty,
            InductionStatus = c.dfeta_InductionStatus.ToInductionStatus()
        })
        .ToList();

    private (List<InductionInfo> Inductions, List<EventBase> Events) MapInductionsAndAudits(
        IReadOnlyCollection<Contact> contacts,
        IEnumerable<dfeta_induction> inductionEntities,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid,
        bool createMigratedEvent)
    {
        var inductionLookup = inductionEntities
            .GroupBy(i => i.dfeta_PersonId.Id)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var inductions = new List<InductionInfo>();
        var events = new List<EventBase>();

        foreach (var contact in contacts)
        {
            dfeta_induction? induction = null;
            if (inductionLookup.TryGetValue(contact.ContactId!.Value, out var personInductions))
            {
                // We shouldn't have multiple induction records for the same person in prod at all but we might in other environments
                // so we'll just take the most recently modified one.
                induction = personInductions.OrderByDescending(i => i.ModifiedOn).First();
                if (personInductions.Length > 1)
                {
                    var errorMessage = $"Contact '{contact.ContactId!.Value}' has multiple induction records.";
                    if (ignoreInvalid)
                    {
                        logger.LogWarning(errorMessage);
                    }
                    else
                    {
                        throw new InvalidOperationException(errorMessage);
                    }
                }
            }

            var mapped = MapInductionInfoFromDqtInduction(induction, contact, ignoreInvalid);
            if (mapped is null)
            {
                continue;
            }

            if (induction is not null)
            {
                var inductionAttributeNames = new[]
                {
                    dfeta_induction.Fields.dfeta_PersonId,
                    dfeta_induction.Fields.dfeta_CompletionDate,
                    dfeta_induction.Fields.dfeta_InductionExemptionReason,
                    dfeta_induction.Fields.dfeta_StartDate,
                    dfeta_induction.Fields.dfeta_InductionStatus,
                    dfeta_induction.Fields.ModifiedOn,
                    dfeta_induction.Fields.StateCode
                };

                if (auditDetails.TryGetValue(induction!.Id, out var inductionAudits))
                {
                    var inductionAuditDetails = inductionAudits.AuditDetails;
                    var inductionVersions = GetEntityVersions(induction, inductionAuditDetails, inductionAttributeNames);

                    events.Add(inductionAuditDetails.Any(a => a.AuditRecord.ToEntity<Audit>().Action == Audit_Action.Create) ?
                        MapCreatedEvent(inductionVersions.First()) :
                        MapImportedEvent(inductionVersions.First()));

                    foreach (var (thisVersion, previousVersion) in inductionVersions.Skip(1).Zip(inductionVersions, (thisVersion, previousVersion) => (thisVersion, previousVersion)))
                    {
                        var mappedEvent = MapUpdatedEvent(thisVersion, previousVersion);

                        if (mappedEvent is not null)
                        {
                            events.Add(mappedEvent);
                        }
                    }

                    if (createMigratedEvent)
                    {
                        events.Add(MapMigratedEvent(inductionVersions.Last(), mapped));
                    }
                }
            }

            if (auditDetails.TryGetValue(contact.ContactId!.Value, out var contactAudits))
            {
                var contactAttributeNames = new[]
                {
                    Contact.Fields.dfeta_InductionStatus
                };

                var contactAuditDetails = contactAudits.AuditDetails;
                var contactVersions = GetEntityVersions(contact, contactAuditDetails, contactAttributeNames);

                foreach (var (thisVersion, previousVersion) in contactVersions.Skip(1).Zip(contactVersions, (thisVersion, previousVersion) => (thisVersion, previousVersion)))
                {
                    var mappedEvent = MapContactInductionStatusChangedEvent(thisVersion, previousVersion);

                    if (mappedEvent is not null)
                    {
                        events.Add(mappedEvent);
                    }
                }
            }

            inductions.Add(mapped);
        }

        return (inductions, events);

        EventBase MapCreatedEvent(EntityVersionInfo<dfeta_induction> snapshot)
        {
            return new DqtInductionCreatedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Entity.Id}-Imported",
                CreatedUtc = snapshot.Timestamp,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                PersonId = snapshot.Entity.dfeta_PersonId.Id,
                Induction = GetEventDqtInduction(snapshot.Entity)
            };
        }

        EventBase? MapUpdatedEvent(EntityVersionInfo<dfeta_induction> snapshot, EntityVersionInfo<dfeta_induction> previous)
        {
            if (snapshot.ChangedAttributes.Contains(dfeta_induction.Fields.StateCode))
            {
                var nonStateAttributes = snapshot.ChangedAttributes
                    .Where(a => !(a is dfeta_induction.Fields.StateCode or dfeta_induction.Fields.StatusCode))
                    .ToArray();

                if (nonStateAttributes.Length > 0)
                {
                    throw new InvalidOperationException(
                        $"Expected state and status attributes to change in isolation but also received: {string.Join(", ", nonStateAttributes)}.");
                }

                if (snapshot.Entity.StateCode == dfeta_inductionState.Inactive)
                {
                    return new DqtInductionDeactivatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        Key = $"{snapshot.Id}",  // The CRM Audit ID
                        CreatedUtc = snapshot.Timestamp,
                        RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                        PersonId = snapshot.Entity.dfeta_PersonId.Id,
                        Induction = GetEventDqtInduction(snapshot.Entity)
                    };
                }
                else
                {
                    return new DqtInductionReactivatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        Key = $"{snapshot.Id}",  // The CRM Audit ID
                        CreatedUtc = snapshot.Timestamp,
                        RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                        PersonId = snapshot.Entity.dfeta_PersonId.Id,
                        Induction = GetEventDqtInduction(snapshot.Entity)
                    };
                }
            }

            var changes = DqtInductionUpdatedEventChanges.None |
                (snapshot.ChangedAttributes.Contains(dfeta_induction.Fields.dfeta_StartDate) ? DqtInductionUpdatedEventChanges.StartDate : 0) |
                (snapshot.ChangedAttributes.Contains(dfeta_induction.Fields.dfeta_CompletionDate) ? DqtInductionUpdatedEventChanges.CompletionDate : 0) |
                (snapshot.ChangedAttributes.Contains(dfeta_induction.Fields.dfeta_InductionStatus) ? DqtInductionUpdatedEventChanges.Status : 0) |
                (snapshot.ChangedAttributes.Contains(dfeta_induction.Fields.dfeta_InductionExemptionReason) ? DqtInductionUpdatedEventChanges.ExemptionReason : 0);

            if (changes == DqtInductionUpdatedEventChanges.None)
            {
                return null;
            }

            return new DqtInductionUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Id}",  // The CRM Audit ID
                CreatedUtc = snapshot.Timestamp,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                PersonId = snapshot.Entity.dfeta_PersonId.Id,
                Induction = GetEventDqtInduction(snapshot.Entity),
                OldInduction = GetEventDqtInduction(previous.Entity),
                Changes = changes
            };
        }

        EventBase? MapContactInductionStatusChangedEvent(EntityVersionInfo<Contact> snapshot, EntityVersionInfo<Contact> previous)
        {
            if (snapshot.ChangedAttributes.Contains(Contact.Fields.StateCode))
            {
                var nonStateAttributes = snapshot.ChangedAttributes
                    .Where(a => !(a is Contact.Fields.StateCode or Contact.Fields.StatusCode))
                    .ToArray();

                if (nonStateAttributes.Length > 0)
                {
                    throw new InvalidOperationException(
                        $"Expected state and status attributes to change in isolation but also received: {string.Join(", ", nonStateAttributes)}.");
                }
            }

            if (!snapshot.ChangedAttributes.Contains(Contact.Fields.dfeta_InductionStatus))
            {
                return null;
            }

            return new DqtContactInductionStatusChangedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Id}",  // The CRM Audit ID
                CreatedUtc = snapshot.Timestamp,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                PersonId = snapshot.Entity.Id,
                InductionStatus = snapshot.Entity.dfeta_InductionStatus.ToString(),
                OldInductionStatus = previous.Entity.dfeta_InductionStatus.ToString()
            };
        }

        EventBase MapImportedEvent(EntityVersionInfo<dfeta_induction> snapshot)
        {
            return new DqtInductionImportedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Entity.Id}-Imported",
                CreatedUtc = snapshot.Timestamp,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(snapshot.UserId, snapshot.UserName),
                PersonId = snapshot.Entity.dfeta_PersonId.Id,
                Induction = GetEventDqtInduction(snapshot.Entity),
                DqtState = (int)snapshot.Entity.StateCode!
            };
        }

        EventBase MapMigratedEvent(EntityVersionInfo<dfeta_induction> snapshot, InductionInfo mappedInduction)
        {
            return new InductionMigratedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{snapshot.Entity.Id}-Migrated",
                CreatedUtc = clock.UtcNow,
                RaisedBy = EventModels.RaisedByUserInfo.FromUserId(Core.DataStore.Postgres.Models.SystemUser.SystemUserId),
                PersonId = snapshot.Entity.dfeta_PersonId.Id,
                InductionStartDate = mappedInduction.InductionStartDate,
                InductionCompletedDate = mappedInduction.InductionCompletedDate,
                InductionStatus = mappedInduction.InductionStatus.ToString(),
                InductionExemptionReason = "",  // TODO
                DqtInduction = GetEventDqtInduction(snapshot.Entity)
            };
        }

        EventModels.DqtInduction GetEventDqtInduction(dfeta_induction induction)
        {
            return new EventModels.DqtInduction()
            {
                InductionId = induction.Id,
                StartDate = induction.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                CompletionDate = induction.dfeta_CompletionDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                InductionStatus = induction.dfeta_InductionStatus.ToString(),
                InductionExemptionReason = induction.dfeta_InductionExemptionReason.ToString()
            };
        }
    }

    private record ModelTypeSyncInfo
    {
        public required string? CreateTempTableStatement { get; init; }
        public required string? CopyStatement { get; init; }
        public required string? UpsertStatement { get; init; }
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

    private record InductionInfo
    {
        public required Guid PersonId { get; init; }
        public required DateOnly? InductionCompletedDate { get; init; }
        public required Guid[] InductionExemptionReasonIds { get; init; }
        public required DateOnly? InductionStartDate { get; init; }
        public required InductionStatus? InductionStatus { get; init; }
        public required DateTime? DqtModifiedOn { get; init; }
    }

    public static class ModelTypes
    {
        public const string Person = "Person";
        public const string Event = "Event";
        public const string Induction = "Induction";
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
}

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
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Services.TrsDataSync;

public class TrsDataSyncHelper(
    NpgsqlDataSource trsDbDataSource,
    [FromKeyedServices(TrsDataSyncService.CrmClientName)] IOrganizationServiceAsync2 organizationService,
#pragma warning disable CS9113 // Parameter is unread.
    ReferenceDataCache referenceDataCache,
#pragma warning restore CS9113 // Parameter is unread.
    IClock clock,
    IAuditRepository auditRepository,
    ILogger<TrsDataSyncHelper> logger,
    IFileService fileService)
{
    private delegate Task SyncEntitiesHandler(IReadOnlyCollection<Entity> entities, bool ignoreInvalid, bool dryRun, CancellationToken cancellationToken);

    private const string NowParameterName = "@now";
    private const string IdsParameterName = "@ids";
    private const int MaxAuditRequestsPerBatch = 10;

    private static readonly IReadOnlyDictionary<string, ModelTypeSyncInfo> _modelTypeSyncInfo = new Dictionary<string, ModelTypeSyncInfo>()
    {
        { ModelTypes.Person, GetModelTypeSyncInfoForPerson() },
        { ModelTypes.Event, GetModelTypeSyncInfoForEvent() },
        { ModelTypes.Induction, GetModelTypeSyncInfoForInduction() },
        { ModelTypes.DqtNote, GetModelTypeSyncInfoForNotes() },
    };

    private readonly ISubject<object[]> _syncedEntitiesSubject = new Subject<object[]>();

    public IObservable<object[]> GetSyncedEntitiesObservable() => _syncedEntitiesSubject;

    private bool IsFakeXrm { get; } = organizationService.GetType().FullName == "Castle.Proxies.ObjectProxy_2";

    public static (string EntityLogicalName, string[] AttributeNames) GetEntityInfoForModelType(string modelType)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);
        return (modelTypeSyncInfo.EntityLogicalName, modelTypeSyncInfo.AttributeNames);
    }

    public bool TryMapDqtInductionExemptionReason(dfeta_InductionExemptionReason dqtInductionExemptionReason, out Guid inductionExemptionReasonId)
    {
        Guid? id = dqtInductionExemptionReason switch
        {
            dfeta_InductionExemptionReason.Exempt => new("a5faff9f-29ce-4a6b-a7b8-0c1f57f15920"),
            dfeta_InductionExemptionReason.ExemptDataLossErrorCriteria => new("204f86eb-0383-40eb-b793-6fccb76ecee2"),
            dfeta_InductionExemptionReason.HasoriseligibleforfullregistrationinScotland => new("a112e691-1694-46a7-8f33-5ec5b845c181"),
            dfeta_InductionExemptionReason.OverseasTrainedTeacher => new("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"),
            dfeta_InductionExemptionReason.Qualifiedbefore07May1999 => new("5a80cee8-98a8-426b-8422-b0e81cb49b36"),
            dfeta_InductionExemptionReason.Qualifiedbetween07May1999and01April2003FirstpostwasinWalesandlastedaminimumoftwoterms => new("15014084-2d8d-4f51-9198-b0e1881f8896"),
            dfeta_InductionExemptionReason.QualifiedthroughEEAmutualrecognitionroute => new("e7118bab-c2b1-4fe8-ad3f-4095d73f5b85"),
            dfeta_InductionExemptionReason.QualifiedthroughFEroutebetween01Sep2001and01Sep2004 => new("0997ab13-7412-4560-8191-e51ed4d58d2a"),
            dfeta_InductionExemptionReason.RegisteredTeacher_havingatleasttwoyearsfulltimeteachingexperience => new("42bb7bbc-a92c-4886-b319-3c1a5eac319a"),
            dfeta_InductionExemptionReason.SuccessfullycompletedinductioninGuernsey => new("fea2db23-93e0-49af-96fd-83c815c17c0b"),
            dfeta_InductionExemptionReason.SuccessfullycompletedinductioninIsleOfMan => new("e5c3847d-8fb6-4b31-8726-812392da8c5c"),
            dfeta_InductionExemptionReason.SuccessfullycompletedinductioninJersey => new("243b21a8-0be4-4af5-8874-85944357e7f8"),
            dfeta_InductionExemptionReason.SuccessfullycompletedinductioninNorthernIreland => new("3471ab35-e6e4-4fa9-a72b-b8bd113df591"),
            dfeta_InductionExemptionReason.SuccessfullycompletedinductioninServiceChildrensEducationschoolsinGermanyorCyprus => new("7d17d904-c1c6-451b-9e09-031314bd35f7"),
            dfeta_InductionExemptionReason.SuccessfullycompletedinductioninWales => InductionExemptionReason.PassedInWalesId,
            dfeta_InductionExemptionReason.SuccessfullycompletedprobationaryperiodinGibraltar => new("a751494a-7e7a-4836-96cb-00b9ed6e1b5f"),
            dfeta_InductionExemptionReason.TeacherhasbeenawardedQTLSandisexemptprovidedtheymaintaintheirmembershipwiththeSocietyforEducationandTraining => new("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db"),
            _ => null
        };

        if (id is null)
        {
            inductionExemptionReasonId = default;
            return false;
        }

        inductionExemptionReasonId = id.Value;
        return true;
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

    private static List<string> GetIgnoreNotesContainingTerms() => new List<string>
    {
        ".",
        "itt result updated to pass or approved as a part of tq data query exercise. the result was one of 102668 results corrected by a data fix run on 09 january 2009",
        "manpay1205 letter suppressed",
        "name amended in error by tp update, name corrected to previous entry held by gtc",
        "�",
        "0 day letter",
        "anomoly",
        "apology as previous letter had an incorrect details due to file creation error",
        "autoregd nqt reminder",
        "bulk dereg",
        "cancel fee",
        "cancellation of fees",
        "card details taken",
        "cc details taken",
        "cc received",
        "contacted regarding requirements for otts and instructors to be provisionally registered",
        "currfees only) letter issued",
        "curryrfees",
        "dd cancelled",
        "dd claim",
        "dd conf",
        "dd dconfirmation",
        "dd details",
        "dd end",
        "dd failed",
        "dd failure",
        "dd fee notice",
        "dd incorrect",
        "dd letter",
        "dd mandate",
        "dd notification",
        "dd pilot",
        "dd rec`d",
        "dd rejected",
        "dd ret",
        "dd returned",
        "dd run",
        "dereg action",
        "dereg form",
        "de - reg form",
        "dereg, action",
        "dereg,action",
        "dereg.",
        "deregistered",
        "deregistration confirmation",
        "de - registration date manually changed",
        "de - regn",
        "deregrem",
        "details extracted on",
        "did not claim the teacher",
        "direct debit",
        "dual registered - info from wales",
        "due to an administrative error this unregistered teacher did not have their deregistration date changed",
        "edc 2005 / 06 teachers working at relationship updated",
        "edc put out of service",
        "email address hard bounce",
        "email address irresolvable hard bounce back",
        "employer data collection",
        "employment at school",
        "employment update letter and cod form sent",
        "employment updated",
        "fee cancellation",
        "fee chase",
        "fee notice",
        "fee notification",
        "fee notin",
        "fee paid",
        "fee receipt",
        "fee reciept",
        "fee remind",
        "fee waiver",
        "fee year",
        "finance",
        "ftf - achieve - 20060606 - raceequalityandyourschool - leeds",
        "ftf - achieve - 20060607 - raceequalityandyourschool - leeds",
        "future dereg request",
        "general query - role & remit info sheet sent in response",
        "gtc / ioe conference",
        "in service(is) updated from edc",
        "inappropriate contact address removed and site usages for school address recalculated",
        "intention to dereg",
        "intention to register updated following receipt of a suitability declaration form - itt exit",
        "invalid email address removed",
        "la address removed as no mail should be sent to la",
        "letter sent to update employment details",
        "loaded home contact address as active post migration",
        "manually reregistered",
        "more info for deregistration",
        "more info to dereg",
        "more information for deregistration requested",
        "more information requested for deregistration",
        "new card",
        "newregcard",
        "no address email sent to",
        "not enough info to dereg teacher so letter and dereg form sent",
        "not registered",
        "nqtfee",
        "paid by dd",
        "part yr fees",
        "paydereg",
        "payment",
        "possible dereg",
        "provisional registration",
        "pryrfees",
        "re deregistration",
        "re scaled fees",
        "record has been temporarily inactivated, consult data governance team before amending these records",
        "refer to payer",
        "refund issued",
        "refund request",
        "regable45s letter issued and attached",
        "registration has now been removed",
        "registration status manually amended from ineligible for full registration to ineligible",
        "replacement card",
        "replacement registration card",
        "replacementcard",
        "request refund",
        "requested more information to deregister",
        "school address in home address field removed by reg",
        "sd fees",
        "sent application to register",
        "supply(nonla) employment closed",
        "teacher not registered.sent a reminder that in error refers to them as being registered",
        "teacher put out of service and then back in service at the same school to correct site usage",
        "teacher removed from service at organisation",
        "teacher taken out of service at welsh school",
        "teacher was recorded either as teacher supply agency or unattached supply teaching and is now recorded as supply (non la)",
        "teacher was sent a confirmation letter confirming that they were provisionally? registered. the letter incorrectly referred to them as an ott and they will receive another letter",
        "this record was inactive with an active address.the the address has been inactivated  with an end date of the day it was closed",
        "total amount due",
        "total to be paid",
        "unregistered teacher had an incorrect deregistration date",
        "updated employment",
        "updated from edc",
        "withpryrfees) letter issued to"
    }
    .Select(s => s.ToLowerInvariant())
    .ToList();

    private async Task<DqtNoteInfo?> MapNoteFromDqtAnnotationAsync(
        Annotation annotation)
    {
        var ignoredTerms = GetIgnoreNotesContainingTerms();
        var lowerInput = await annotation.GetNoteTextWithoutHtmlAsync() ?? string.Empty;
        if (ignoredTerms.Any(term => lowerInput.ToLower().Contains(term)) &&
            (!string.IsNullOrEmpty(annotation.Subject) && annotation.Subject.Contains("Entered by REG", StringComparison.InvariantCultureIgnoreCase)))
        {
            return null;
        }

        return new DqtNoteInfo()
        {
            Id = annotation.Id,
            PersonId = annotation!.ObjectId.Id,
            ContentHtml = annotation.NoteText,
            CreatedByDqtUserId = annotation.CreatedBy.Id,
            CreatedByDqtUserName = annotation.CreatedBy.Name,
            UpdatedOn = annotation.ModifiedOn,
            CreatedOn = annotation.CreatedOn!.Value,
            UpdatedByDqtUserId = annotation.ModifiedBy?.Id,
            UpdatedByDqtUserName = annotation.ModifiedBy?.Name,
            FileName = null,
            AttachmentBytes = string.IsNullOrEmpty(annotation.FileName) ? null : Convert.FromBase64String(annotation.DocumentBody),
            OriginalFileName = annotation.FileName,
            MimeType = annotation.MimeType,
        };
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

        Guid[] exemptionReasonIds = [];
        if (induction?.dfeta_InductionExemptionReason is not null)
        {
            if (!TryMapDqtInductionExemptionReason(induction.dfeta_InductionExemptionReason!.Value, out var exemptionReasonId))
            {
                var errorMessage = $"Failed mapping DQT Induction Exemption Reason '{induction.dfeta_InductionExemptionReason}' for contact {contact.ContactId}.";
                if (ignoreInvalid)
                {
                    logger.LogWarning(errorMessage);
                    return null;
                }

                throw new InvalidOperationException(errorMessage);
            }

            exemptionReasonIds = [exemptionReasonId];
        }

        return new InductionInfo()
        {
            PersonId = contact.ContactId!.Value,
            InductionId = induction?.dfeta_inductionId,
            InductionStatus = contact.dfeta_InductionStatus.ToInductionStatus(),
            InductionStartDate = induction?.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            InductionCompletedDate = induction?.dfeta_CompletionDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            InductionExemptionReasonIds = exemptionReasonIds,
            DqtModifiedOn = induction?.ModifiedOn,
            InductionExemptWithoutReason = contact.dfeta_InductionStatus.ToInductionStatus() is InductionStatus.Exempt && exemptionReasonIds.Length == 0
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
        (await SyncPersonsAsync(new[] { entity }, syncAudit, ignoreInvalid, dryRun, cancellationToken)).Count() == 1;

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
        using var txn = await connection.BeginTransactionAsync(cancellationToken);

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

    public async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<dfeta_induction> inductions,
        bool syncAudit,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var contactAttributeNames = GetModelTypeSyncInfo(ModelTypes.Person).AttributeNames;

        var contacts = await GetEntitiesAsync<Contact>(
            Contact.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            inductions.Select(e => e.dfeta_PersonId.Id),
            contactAttributeNames,
            activeOnly: false,
            cancellationToken);

        return await SyncInductionsAsync(contacts, inductions, syncAudit, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<Contact> contacts,
        bool syncAudit,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(ModelTypes.Induction);

        var inductions = await GetEntitiesAsync<dfeta_induction>(
            dfeta_induction.EntityLogicalName,
            dfeta_induction.Fields.dfeta_PersonId,
            contacts.Select(c => c.ContactId!.Value),
            modelTypeSyncInfo.AttributeNames,
            true,
            cancellationToken);

        return await SyncInductionsAsync(contacts, inductions, syncAudit, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<int> SyncAnnotationsAsync(
        IReadOnlyCollection<Annotation> annotations,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo<DqtNoteInfo>(ModelTypes.DqtNote);
        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);
        var toSync = new List<DqtNoteInfo>();
        foreach (var ann in annotations)
        {
            var dqtNote = await MapNoteFromDqtAnnotationAsync(ann);
            if (dqtNote is not null)
            {
                toSync.Add(dqtNote);
            }
        }

        //upload attachments new or remove attachment
        foreach (var noteAttachment in toSync)
        {
            var fileId = noteAttachment!.Id;

            //if note does not have an attachment or length is 0, attempt delete
            if (noteAttachment!.AttachmentBytes is null || noteAttachment.AttachmentBytes!.Length == 0)
            {
                //not interested if file exists or not
                await fileService.DeleteFileAsync(fileId!.Value);
                noteAttachment.FileName = null;
                noteAttachment.OriginalFileName = null;
            }

            //upload new attachment
            if (noteAttachment!.AttachmentBytes != null)
            {
                //incoming note attachment filename is the annotation id
                var bytes = noteAttachment.AttachmentBytes;
                using (var stream = new MemoryStream(bytes))
                {
                    await fileService.UploadFileAsync(stream, noteAttachment.MimeType, noteAttachment.Id);
                    noteAttachment.OriginalFileName = noteAttachment.OriginalFileName;
                    noteAttachment.FileName = fileId.ToString();
                }
            }
        }

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
            modelTypeSyncInfo.WriteRecord!(writer, i!);
        }

        await writer.CompleteAsync(cancellationToken);
        await writer.CloseAsync(cancellationToken);

        using (var mergeCommand = connection.CreateCommand())
        {
            mergeCommand.CommandText = modelTypeSyncInfo.UpsertStatement;
            mergeCommand.Parameters.Add(new NpgsqlParameter(NowParameterName, clock.UtcNow));
            mergeCommand.Transaction = txn;
            await mergeCommand.ExecuteNonQueryAsync();
        }

        if (!dryRun)
        {
            await txn.CommitAsync(cancellationToken);
        }

        _syncedEntitiesSubject.OnNext([.. toSync]);
        return toSync.Count;
    }

    public async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<Contact> contacts,
        IReadOnlyCollection<dfeta_induction> entities,
        bool syncAudit,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        if (syncAudit)
        {
            await SyncAuditAsync(dfeta_induction.EntityLogicalName, entities.Select(q => q.Id), skipIfExists: false, cancellationToken);
        }

        var auditDetails = await GetAuditRecordsFromAuditRepositoryAsync(dfeta_induction.EntityLogicalName, dfeta_induction.PrimaryIdAttribute, entities.Select(q => q.Id), cancellationToken);
        return await SyncInductionsAsync(contacts, entities, auditDetails, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<Contact> contacts,
        IReadOnlyCollection<dfeta_induction> entities,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var (inductions, events) = MapInductionsAndAudits(contacts, entities, auditDetails, ignoreInvalid);
        return await SyncInductionsAsync(inductions, events, ignoreInvalid, dryRun, cancellationToken);
    }

    private async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<InductionInfo> inductions,
        IReadOnlyCollection<EventBase> events,
        bool ignoreInvalid,
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

            await txn.SaveEventsAsync(eventsForSyncedContacts, "events_induction_import", clock, cancellationToken, timeoutSeconds: 120);

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

    public async Task<int> MigrateInductionsAsync(
        IReadOnlyCollection<Contact> contacts,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(ModelTypes.Induction);

        var inductions = await GetEntitiesAsync<dfeta_induction>(
            dfeta_induction.EntityLogicalName,
            dfeta_induction.Fields.dfeta_PersonId,
            contacts.Select(c => c.ContactId!.Value),
            modelTypeSyncInfo.AttributeNames,
            true,
            cancellationToken);

        var inductionLookup = inductions
            .GroupBy(i => i.dfeta_PersonId.Id)
            .ToDictionary(g => g.Key, g => g.ToArray());

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

            if (induction is null && contact.dfeta_InductionStatus is null)
            {
                continue;
            }

            var mapped = MapInductionInfoFromDqtInduction(induction, contact, ignoreInvalid);
            if (mapped is null)
            {
                continue;
            }

            var migratedEvent = MapMigratedEvent(contact, induction, mapped);
            events.Add(migratedEvent);
        }

        if (events.Count == 0)
        {
            return 0;
        }

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);
        using var txn = await connection.BeginTransactionAsync(cancellationToken);
        await txn.SaveEventsAsync(events, "events_induction_migration", clock, cancellationToken);

        if (!dryRun)
        {
            await txn.CommitAsync();
        }

        return events.Count;

        EventBase MapMigratedEvent(Contact contact, dfeta_induction? dqtInduction, InductionInfo mappedInduction)
        {
            var dqtInductionStatus = (contact.dfeta_InductionStatus?.GetMetadata().Name ??
                dqtInduction?.dfeta_InductionStatus?.GetMetadata().Name)!;

            return new InductionMigratedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{dqtInduction?.Id ?? contact.Id}-Migrated",
                CreatedUtc = new DateTime(2025, 02, 18, 10, 17, 05, DateTimeKind.Utc),
                RaisedBy = EventModels.RaisedByUserInfo.FromUserId(Core.DataStore.Postgres.Models.SystemUser.SystemUserId),
                PersonId = contact.Id,
                InductionStartDate = mappedInduction.InductionStartDate,
                InductionCompletedDate = mappedInduction.InductionCompletedDate,
                InductionStatus = mappedInduction.InductionStatus,
                InductionExemptionReasonId = mappedInduction.InductionExemptionReasonIds switch
                {
                    [var id] => id,
                    _ => null
                },
                DqtInduction = dqtInduction is not null ? GetEventDqtInduction(dqtInduction) : null,
                DqtInductionStatus = dqtInductionStatus
            };
        }
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
                    await SyncAuditAsync(entityLogicalName, new[] { id }, skipIfExists: false, cancellationToken);
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

    private static ModelTypeSyncInfo GetModelTypeSyncInfoForNotes()
    {
        var tempTableName = "temp_notes_import";
        var tableName = "notes";

        var columnNames = new[]
        {
            "note_id",
            "person_id",
            "content_html",
            "created_on",
            "created_by_dqt_user_id",
            "file_name",
            "original_file_name",
            "created_by_dqt_user_name",
            "updated_by_dqt_user_id",
            "updated_by_dqt_user_name",
            "updated_on"
        };

        var columnsToUpdate = new[] {
            "content_html",
            "file_name",
            "original_file_name",
            "updated_on",
            "updated_by_dqt_user_id",
            "updated_by_dqt_user_name"
        };

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement =
            $"""
            CREATE TEMP TABLE {tempTableName}
            (
                note_id uuid NOT NULL,
                person_id uuid NOT NULL,
                content_html TEXT NULL,
                created_on timestamp with time zone,
                created_by_dqt_user_id uuid NOT NULL,
                file_name TEXT NULL,
                original_file_name TEXT NULL,
                created_by_dqt_user_name TEXT,
                updated_by_dqt_user_name text NULL,
                updated_by_dqt_user_id uuid NULL,
                updated_on timestamp with time zone
            )
            """;

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var insertStatement =
            $"""
            INSERT INTO {tableName} ({columnList})
            SELECT {columnList} FROM {tempTableName}
            ON CONFLICT (note_id) DO UPDATE
            SET {string.Join(", ", columnsToUpdate.Select(c => $"{c} = EXCLUDED.{c}"))}
            WHERE {tableName}.updated_on IS NULL OR {tableName}.updated_on < EXCLUDED.updated_on;
            """;

        //artitrary date of now-1 day because there are no rows in dqt_note
        var getLastModifiedOnStatement = $"SELECT COALESCE(MAX(updated_on), NOW() - INTERVAL '1 day') FROM {tableName}";

        var deleteStatement = $"DELETE FROM {tableName} WHERE note_id = ANY({IdsParameterName})";

        var attributeNames = new[]
        {
            Annotation.PrimaryIdAttribute,
            Annotation.Fields.FileName,
            Annotation.Fields.DocumentBody,
            Annotation.Fields.ObjectId,
            Annotation.Fields.ModifiedOn,
            Annotation.Fields.NoteText,
            Annotation.Fields.CreatedBy,
            Annotation.Fields.CreatedOn,
            Annotation.Fields.ModifiedBy,
            Annotation.Fields.MimeType,
        };

        Action<NpgsqlBinaryImporter, DqtNoteInfo> writeRecord = (writer, note) =>
        {
            writer.WriteValueOrNull(note.Id, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(note.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(note.ContentHtml, NpgsqlDbType.Text);
            writer.WriteValueOrNull(note.CreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(note.CreatedByDqtUserId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(note.FileName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(note.OriginalFileName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(note.CreatedByDqtUserName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(note.UpdatedByDqtUserId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(note.UpdatedByDqtUserName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(note.UpdatedOn ?? note.CreatedOn, NpgsqlDbType.TimestampTz); //default updated on to when it was created
        };

        return new ModelTypeSyncInfo<DqtNoteInfo>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            UpsertStatement = insertStatement,
            DeleteStatement = deleteStatement,
            IgnoreDeletions = false,
            GetLastModifiedOnStatement = getLastModifiedOnStatement,
            EntityLogicalName = Annotation.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncAnnotationsAsync(entities.Select(e => e.ToEntity<Annotation>()).ToArray(), ignoreInvalid, dryRun, ct),
            WriteRecord = writeRecord
        };
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
            "status",
            "merged_with_person_id",
            "trn",
            "first_name",
            "middle_name",
            "last_name",
            "date_of_birth",
            "email_address",
            "national_insurance_number",
            "mobile_number",
            "dqt_contact_id",
            "dqt_state",
            "dqt_created_on",
            "dqt_modified_on",
            "dqt_first_name",
            "dqt_middle_name",
            "dqt_last_name",
            "qts_date",
            "eyts_date"
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
            Contact.Fields.dfeta_NINumber,
            Contact.Fields.MobilePhone,
            Contact.Fields.EMailAddress1,
            Contact.Fields.dfeta_InductionStatus,
            Contact.Fields.dfeta_qtlsdate,
            Contact.Fields.dfeta_QTSDate,
            Contact.Fields.dfeta_EYTSDate,
            Contact.Fields.dfeta_MergedWith
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
            writer.WriteValueOrNull(person.MobileNumber, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.DqtContactId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(person.DqtState, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(person.DqtCreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(person.DqtModifiedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(person.DqtFirstName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.DqtMiddleName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.DqtLastName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.QtsDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull(person.EytsDate, NpgsqlDbType.Date);
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
            "dqt_induction_modified_on",
            "induction_exempt_without_reason"
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
                dqt_induction_modified_on timestamp with time zone,
                induction_exempt_without_reason boolean
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
            dfeta_induction.PrimaryIdAttribute,
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

        Action<NpgsqlBinaryImporter, InductionInfo> writeRecord = (writer, induction) =>
        {
            writer.WriteValueOrNull(induction.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(induction.InductionCompletedDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull(induction.InductionExemptionReasonIds, NpgsqlDbType.Array | NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(induction.InductionStartDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull((int?)induction.InductionStatus, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(induction.DqtModifiedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(induction.DqtModifiedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(induction.InductionExemptWithoutReason, NpgsqlDbType.Boolean);
        };

        return new ModelTypeSyncInfo<InductionInfo>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            UpsertStatement = updateStatement,
            DeleteStatement = null,
            IgnoreDeletions = true,
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
            IgnoreDeletions = false,
            GetLastModifiedOnStatement = null,
            EntityLogicalName = dfeta_TRSEvent.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncEventsAsync(entities.Select(e => e.ToEntity<dfeta_TRSEvent>()).ToArray(), dryRun, ct),
            WriteRecord = null
        };
    }

    private (List<PersonInfo> Persons, List<EventBase> Events) MapPersonsAndAudits(
        IReadOnlyCollection<Contact> contacts,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid)
    {
        var events = new List<EventBase>();
        var persons = MapPersons(contacts);

        foreach (var contact in contacts)
        {
            if (auditDetails.TryGetValue(contact.ContactId!.Value, out var contactAudits))
            {
                // At the moment we are only interested in induction related changes
                var contactAttributeNames = new[]
                {
                    Contact.Fields.dfeta_qtlsdate,
                    Contact.Fields.dfeta_InductionStatus
                };

                var orderedContactAuditDetails = contactAudits.AuditDetails
                    .OfType<AttributeAuditDetail>()
                    .Where(a => a.AuditRecord.ToEntity<Audit>().Action != Audit_Action.Merge)
                    .Select(a =>
                    {
                        var allChangedAttributes = a.NewValue.Attributes.Keys.Union(a.OldValue.Attributes.Keys).ToArray();
                        var relevantChangedAttributes = allChangedAttributes.Where(k => contactAttributeNames.Contains(k)).ToArray();
                        var newValue = a.NewValue.ToEntity<Contact>().SparseClone(contactAttributeNames);
                        newValue.Id = contact.ContactId!.Value;
                        var oldValue = a.OldValue.ToEntity<Contact>().SparseClone(contactAttributeNames);
                        oldValue.Id = contact.ContactId!.Value;

                        return new AuditInfo<Contact>
                        {
                            AllChangedAttributes = allChangedAttributes,
                            RelevantChangedAttributes = relevantChangedAttributes,
                            NewValue = newValue,
                            OldValue = oldValue,
                            AuditRecord = a.AuditRecord.ToEntity<Audit>()
                        };
                    })
                    .OrderBy(a => a.AuditRecord.CreatedOn)
                    .ThenBy(a => a.AuditRecord.Action == Audit_Action.Create ? 0 : 1)
                    .ToArray();

                foreach (var item in orderedContactAuditDetails)
                {
                    // For the moment (until we migrate contacts) we are not interested in the created event
                    if (item.AuditRecord.Action == Audit_Action.Create)
                    {
                        continue;
                    }

                    if (item.AllChangedAttributes.Contains(Contact.Fields.StateCode) && item.AuditRecord.Action != Audit_Action.Create)
                    {
                        var nonStateAttributes = item.AllChangedAttributes
                            .Where(a => !(a is Contact.Fields.StateCode or Contact.Fields.StatusCode))
                            .ToArray();

                        if (nonStateAttributes.Length > 0)
                        {
                            throw new InvalidOperationException(
                                $"Expected state and status attributes to change in isolation but also received: {string.Join(", ", nonStateAttributes)}.");
                        }
                    }

                    var mappedEvent = MapContactInductionStatusChangedEvent(contact.ContactId!.Value, item.RelevantChangedAttributes, item.NewValue, item.OldValue, item.AuditRecord, orderedContactAuditDetails);
                    if (mappedEvent is not null)
                    {
                        events.Add(mappedEvent);
                    }
                }
            }
        }

        return (persons, events);
    }

    private static EventBase? MapContactInductionStatusChangedEvent(Guid contactId, string[] changedAttributes, Contact newValue, Contact oldValue, Audit audit, AuditInfo<Contact>[] allAuditDetails)
    {
        // Needs to have changed because of a QTLS date change which would be in a separate audit record shortly before the induction status change one (allow 10 seconds)
        if (!changedAttributes.Contains(Contact.Fields.dfeta_InductionStatus) ||
            (changedAttributes.Contains(Contact.Fields.dfeta_InductionStatus) &&
                !allAuditDetails.Any(a => a.RelevantChangedAttributes.Contains(Contact.Fields.dfeta_qtlsdate) && audit.CreatedOn!.Value.Subtract(a.AuditRecord.CreatedOn!.Value).TotalSeconds < 10)))
        {
            return null;
        }

        return new DqtContactInductionStatusChangedEvent()
        {
            EventId = Guid.NewGuid(),
            Key = $"{audit.Id}",  // The CRM Audit ID
            CreatedUtc = audit.CreatedOn!.Value,
            RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(audit.UserId.Id, audit.UserId.Name),
            PersonId = contactId,
            InductionStatus = newValue.dfeta_InductionStatus.ToString(),
            OldInductionStatus = oldValue.dfeta_InductionStatus.ToString()
        };
    }

    private static List<PersonInfo> MapPersons(IEnumerable<Contact> contacts) => contacts
        .Select(c => new PersonInfo()
        {
            PersonId = c.ContactId!.Value,
            CreatedOn = c.CreatedOn!.Value,
            UpdatedOn = c.ModifiedOn!.Value,
            Status = c.StateCode == ContactState.Active ? PersonStatus.Active : PersonStatus.Inactive,
            MergedWithPersonId = c.dfeta_MergedWith?.Id,
            Trn = c.dfeta_TRN,
            FirstName = (c.HasStatedNames() ? c.dfeta_StatedFirstName : c.FirstName) ?? string.Empty,
            MiddleName = (c.HasStatedNames() ? c.dfeta_StatedMiddleName : c.MiddleName) ?? string.Empty,
            LastName = (c.HasStatedNames() ? c.dfeta_StatedLastName : c.LastName) ?? string.Empty,
            DateOfBirth = c.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            EmailAddress = c.EMailAddress1.NormalizeString(),
            NationalInsuranceNumber = c.dfeta_NINumber.NormalizeString(),
            MobileNumber = c.MobilePhone.NormalizeMobileNumber(),
            QtsDate = c.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            EytsDate = c.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            DqtContactId = c.Id,
            DqtState = (int)c.StateCode!,
            DqtCreatedOn = c.CreatedOn!.Value,
            DqtModifiedOn = c.ModifiedOn!.Value,
            DqtFirstName = c.FirstName ?? string.Empty,
            DqtMiddleName = c.MiddleName ?? string.Empty,
            DqtLastName = c.LastName ?? string.Empty
        })
        .ToList();

    private (List<InductionInfo> Inductions, List<EventBase> Events) MapInductionsAndAudits(
        IReadOnlyCollection<Contact> contacts,
        IEnumerable<dfeta_induction> inductionEntities,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid)
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
                    dfeta_induction.Fields.dfeta_CompletionDate,
                    dfeta_induction.Fields.dfeta_InductionExemptionReason,
                    dfeta_induction.Fields.dfeta_StartDate,
                    dfeta_induction.Fields.dfeta_InductionStatus,
                    dfeta_induction.Fields.StateCode
                };

                if (auditDetails.TryGetValue(induction!.Id, out var inductionAudits))
                {
                    var orderedInductionAuditDetails = inductionAudits.AuditDetails
                        .OfType<AttributeAuditDetail>()
                        .Where(a => a.AuditRecord.ToEntity<Audit>().Action != Audit_Action.Merge)
                        .Select(a =>
                        {
                            var allChangedAttributes = a.NewValue.Attributes.Keys.Union(a.OldValue.Attributes.Keys).ToArray();
                            var relevantChangedAttributes = allChangedAttributes.Where(k => inductionAttributeNames.Contains(k)).ToArray();
                            var newValue = a.NewValue.ToEntity<dfeta_induction>().SparseClone(inductionAttributeNames);
                            newValue.Id = induction.Id;
                            var oldValue = a.OldValue.ToEntity<dfeta_induction>().SparseClone(inductionAttributeNames);
                            oldValue.Id = induction.Id;

                            return new AuditInfo<dfeta_induction>
                            {
                                AllChangedAttributes = allChangedAttributes,
                                RelevantChangedAttributes = relevantChangedAttributes,
                                NewValue = newValue,
                                OldValue = oldValue,
                                AuditRecord = a.AuditRecord.ToEntity<Audit>()
                            };
                        })
                        .OrderBy(a => a.AuditRecord.CreatedOn)
                        .ThenBy(a => a.AuditRecord.Action == Audit_Action.Create ? 0 : 1)
                        .ToArray();

                    var auditRecordsToMap = orderedInductionAuditDetails.Skip(1).ToArray();
                    if (orderedInductionAuditDetails.FirstOrDefault() is { AuditRecord: { Action: Audit_Action.Create } } createAction)
                    {
                        events.Add(MapCreatedEvent(contact.ContactId!.Value, createAction.NewValue, createAction.AuditRecord));
                    }
                    else
                    {
                        auditRecordsToMap = orderedInductionAuditDetails;
                        // we may have gaps in the audit history so we can't be sure that we can rewind from the latest back to the imported version
                        var minimalInduction = new Entity(dfeta_induction.EntityLogicalName, induction.Id).ToEntity<dfeta_induction>();
                        minimalInduction.CreatedOn = induction.CreatedOn;
                        minimalInduction.CreatedBy = induction.CreatedBy;
                        events.Add(MapImportedEvent(contact.ContactId!.Value, minimalInduction));
                    }

                    foreach (var item in auditRecordsToMap)
                    {
                        if (item.AllChangedAttributes.Contains(dfeta_induction.Fields.StateCode))
                        {
                            var nonStateAttributes = item.AllChangedAttributes
                                .Where(a => !(a is dfeta_induction.Fields.StateCode or dfeta_induction.Fields.StatusCode))
                                .ToArray();

                            if (nonStateAttributes.Length > 0)
                            {
                                throw new InvalidOperationException(
                                    $"Expected state and status attributes to change in isolation but also received: {string.Join(", ", nonStateAttributes)}.");
                            }
                        }

                        var mappedEvent = MapUpdatedEvent(contact.ContactId!.Value, item.RelevantChangedAttributes, item.NewValue, item.OldValue, item.AuditRecord);
                        if (mappedEvent is not null)
                        {
                            events.Add(mappedEvent);
                        }
                    }
                }
            }

            inductions.Add(mapped);
        }

        return (inductions, events);

        EventBase MapCreatedEvent(Guid contactId, dfeta_induction induction, Audit audit)
        {
            return new DqtInductionCreatedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{induction.Id}-Created",
                CreatedUtc = audit.CreatedOn!.Value,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(audit.UserId.Id, audit.UserId.Name),
                PersonId = contactId,
                Induction = GetEventDqtInduction(induction)
            };
        }

        EventBase MapImportedEvent(Guid contactId, dfeta_induction induction)
        {
            var createdBy = induction.GetAttributeValue<EntityReference>("createdby");
            return new DqtInductionImportedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{induction.Id}-Imported",
                CreatedUtc = induction.CreatedOn!.Value,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(createdBy.Id, createdBy.Name),
                PersonId = contactId,
                Induction = GetEventDqtInduction(induction),
                DqtState = (int)dfeta_inductionState.Active
            };
        }

        EventBase? MapUpdatedEvent(Guid contactId, string[] changedAttributes, dfeta_induction newValue, dfeta_induction oldValue, Audit audit)
        {
            if (changedAttributes.Contains(dfeta_induction.Fields.StateCode))
            {
                if (newValue.StateCode == dfeta_inductionState.Inactive)
                {
                    return new DqtInductionDeactivatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        Key = $"{audit.Id}",  // The CRM Audit ID
                        CreatedUtc = audit.CreatedOn!.Value,
                        RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(audit.UserId.Id, audit.UserId.Name),
                        PersonId = contactId,
                        Induction = GetEventDqtInduction(newValue)
                    };
                }
                else
                {
                    return new DqtInductionReactivatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        Key = $"{audit.Id}",  // The CRM Audit ID
                        CreatedUtc = audit.CreatedOn!.Value,
                        RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(audit.UserId.Id, audit.UserId.Name),
                        PersonId = contactId,
                        Induction = GetEventDqtInduction(newValue)
                    };
                }
            }

            var changes = DqtInductionUpdatedEventChanges.None |
                (changedAttributes.Contains(dfeta_induction.Fields.dfeta_StartDate) ? DqtInductionUpdatedEventChanges.StartDate : 0) |
                (changedAttributes.Contains(dfeta_induction.Fields.dfeta_CompletionDate) ? DqtInductionUpdatedEventChanges.CompletionDate : 0) |
                (changedAttributes.Contains(dfeta_induction.Fields.dfeta_InductionStatus) ? DqtInductionUpdatedEventChanges.Status : 0) |
                (changedAttributes.Contains(dfeta_induction.Fields.dfeta_InductionExemptionReason) ? DqtInductionUpdatedEventChanges.ExemptionReason : 0);

            if (changes == DqtInductionUpdatedEventChanges.None)
            {
                return null;
            }

            return new DqtInductionUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{audit.Id}",  // The CRM Audit ID
                CreatedUtc = audit.CreatedOn!.Value,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(audit.UserId.Id, audit.UserId.Name),
                PersonId = contactId,
                Induction = GetEventDqtInduction(newValue),
                OldInduction = GetEventDqtInduction(oldValue),
                Changes = changes
            };
        }
    }

    EventModels.DqtInduction GetEventDqtInduction(dfeta_induction induction)
    {
        Option<DateOnly?> startDateOption = induction.TryGetAttributeValue<DateTime?>(dfeta_induction.Fields.dfeta_StartDate, out var startDate)
            ? Option.Some(startDate.ToDateOnlyWithDqtBstFix(isLocalTime: true))
            : Option.None<DateOnly?>();

        Option<DateOnly?> completionDateOption = induction.TryGetAttributeValue<DateTime?>(dfeta_induction.Fields.dfeta_CompletionDate, out var completionDate)
            ? Option.Some(completionDate.ToDateOnlyWithDqtBstFix(isLocalTime: true))
            : Option.None<DateOnly?>();

        Option<string?> inductionStatusOption = induction.TryGetAttributeValue<OptionSetValue>(dfeta_induction.Fields.dfeta_InductionStatus, out var inductionStatus)
            ? Option.Some(inductionStatus is not null ? (string?)((dfeta_InductionStatus?)inductionStatus!.Value)!.Value.GetMetadata().Name : null)
            : Option.None<string?>();

        Option<string?> inductionExemptionReasonOption = induction.TryGetAttributeValue<OptionSetValue>(dfeta_induction.Fields.dfeta_InductionExemptionReason, out var inductionExemptionReason)
            ? Option.Some(inductionExemptionReason is not null ? (string?)((dfeta_InductionExemptionReason?)inductionExemptionReason!.Value)!.Value.GetMetadata().Name : null)
            : Option.None<string?>();

        return new EventModels.DqtInduction()
        {
            InductionId = induction.Id,
            StartDate = startDateOption,
            CompletionDate = completionDateOption,
            InductionStatus = inductionStatusOption,
            InductionExemptionReason = inductionExemptionReasonOption
        };
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

    private record PersonInfo
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
        public required string? MobileNumber { get; init; }
        public required DateOnly? QtsDate { get; init; }
        public required DateOnly? EytsDate { get; init; }
        public required Guid? DqtContactId { get; init; }
        public required int? DqtState { get; init; }
        public required DateTime? DqtCreatedOn { get; init; }
        public required DateTime? DqtModifiedOn { get; init; }
        public required string? DqtFirstName { get; init; }
        public required string? DqtMiddleName { get; init; }
        public required string? DqtLastName { get; init; }
    }

    private record InductionInfo
    {
        public required Guid PersonId { get; init; }
        public required Guid? InductionId { get; init; }
        public required DateOnly? InductionCompletedDate { get; init; }
        public required Guid[] InductionExemptionReasonIds { get; init; }
        public required DateOnly? InductionStartDate { get; init; }
        public required InductionStatus InductionStatus { get; init; }
        public required DateTime? DqtModifiedOn { get; init; }
        public required bool InductionExemptWithoutReason { get; init; }
    }

    private record DqtNoteInfo
    {
        public required Guid? Id { get; set; }
        public required Guid PersonId { get; set; }
        public required string? ContentHtml { get; set; }
        public required DateTime CreatedOn { get; set; }
        public required Guid CreatedByDqtUserId { get; set; }
        public required string? CreatedByDqtUserName { get; set; }
        public required DateTime? UpdatedOn { get; set; }
        public required Guid? UpdatedByDqtUserId { get; set; }
        public required string? UpdatedByDqtUserName { get; set; }
        public required string? FileName { get; set; }
        public required byte[]? AttachmentBytes { get; set; }
        public required string? OriginalFileName { get; set; }
        public required string? MimeType { get; set; }

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
        public const string Event = "Event";
        public const string Induction = "Induction";
        public const string DqtNote = "Annotation";
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

    public static string? NormalizeMobileNumber(this string? value) =>
        MobileNumber.TryParse(value, out var mobileNumber) ? mobileNumber.ToString() : null;
}

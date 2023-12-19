using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

public class DeleteMqState
{
    public bool Initialized { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? ProviderName { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public MqDeletionReasonOption? DeletionReason { get; set; }

    public string? DeletionReasonDetail { get; set; }

    public bool? UploadEvidence { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(DeletionReason), nameof(UploadEvidence), nameof(EvidenceFileId))]
    public bool IsComplete => DeletionReason.HasValue &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));

    public async Task EnsureInitialized(
        ICrmQueryDispatcher crmQueryDispatcher,
        ReferenceDataCache referenceDataCache,
        dfeta_qualification qualification)
    {
        if (Initialized)
        {
            return;
        }

        var personDetail = await crmQueryDispatcher.ExecuteQuery(
            new GetContactDetailByIdQuery(
                qualification.dfeta_PersonId.Id,
                new ColumnSet(
                    Contact.Fields.Id,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedLastName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_QTSDate)));

        PersonId = personDetail!.Contact.Id;
        PersonName = personDetail!.Contact.ResolveFullName(includeMiddleName: false);
        var mqEstablishment = qualification.dfeta_MQ_MQEstablishmentId is not null ? await referenceDataCache.GetMqEstablishmentById(qualification.dfeta_MQ_MQEstablishmentId.Id) : null;
        ProviderName = mqEstablishment?.dfeta_name;
        var mqSpecialism = qualification.dfeta_MQ_SpecialismId is not null ? await referenceDataCache.GetMqSpecialismById(qualification.dfeta_MQ_SpecialismId.Id) : null;
        Specialism = mqSpecialism?.ToMandatoryQualificationSpecialism();
        StartDate = qualification.dfeta_MQStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true);
        EndDate = qualification.dfeta_MQ_Date.ToDateOnlyWithDqtBstFix(isLocalTime: true);
        Initialized = true;
    }
}

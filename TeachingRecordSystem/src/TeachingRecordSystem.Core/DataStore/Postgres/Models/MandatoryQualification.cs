using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class MandatoryQualification : Qualification
{
    public Guid? ProviderId { get; set; }
    public MandatoryQualificationSpecialism? Specialism { get; set; }
    public MandatoryQualificationStatus? Status { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public Guid? DqtMqEstablishmentId { get; set; }
    public Guid? DqtSpecialismId { get; set; }

    public static async Task<MandatoryQualification> MapFromDqtQualification(dfeta_qualification qualification, ReferenceDataCache referenceDataCache)
    {
        var mqEstablishments = await referenceDataCache.GetMqEstablishments();
        var mqSpecialisms = await referenceDataCache.GetMqSpecialisms();

        return MapFromDqtQualification(qualification, mqEstablishments, mqSpecialisms);
    }

    public static MandatoryQualification MapFromDqtQualification(
        dfeta_qualification qualification,
        IEnumerable<dfeta_mqestablishment> mqEstablishments,
        IEnumerable<dfeta_specialism> mqSpecialisms)
    {
        if (qualification.dfeta_Type != dfeta_qualification_dfeta_Type.MandatoryQualification)
        {
            throw new ArgumentException("Qualification is not a mandatory qualification.", nameof(qualification));
        }

        var deletedEvent = qualification.dfeta_TrsDeletedEvent is not null and not "{}" ?
            EventInfo.Deserialize<MandatoryQualificationDeletedEvent>(qualification.dfeta_TrsDeletedEvent).Event :
            null;

        MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(
            mqEstablishments.SingleOrDefault(e => e.Id == qualification.dfeta_MQ_MQEstablishmentId!?.Id), out var provider);

        MandatoryQualificationSpecialism? specialism = qualification.dfeta_MQ_SpecialismId is not null ?
            mqSpecialisms.Single(s => s.Id == qualification.dfeta_MQ_SpecialismId.Id).ToMandatoryQualificationSpecialism() :
            null;

        MandatoryQualificationStatus? status = qualification.dfeta_MQ_Status?.ToMandatoryQualificationStatus() ??
            (qualification.dfeta_MQ_Date.HasValue ? MandatoryQualificationStatus.Passed : null);

        return new MandatoryQualification()
        {
            QualificationId = qualification.Id,
            CreatedOn = qualification.CreatedOn!.Value,
            UpdatedOn = qualification.ModifiedOn!.Value,
            DeletedOn = deletedEvent?.CreatedUtc,
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
}

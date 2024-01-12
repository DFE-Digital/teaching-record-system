using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

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

        return TrsDataSyncHelper.MapMandatoryQualificationFromDqtQualification(qualification, mqEstablishments, mqSpecialisms);
    }
}

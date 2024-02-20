using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class MandatoryQualification : Qualification
{
    public MandatoryQualificationProvider? Provider { get; }
    public required Guid? ProviderId { get; set; }
    public required MandatoryQualificationSpecialism? Specialism { get; set; }
    public required MandatoryQualificationStatus? Status { get; set; }
    public required DateOnly? StartDate { get; set; }
    public required DateOnly? EndDate { get; set; }

    public Guid? DqtMqEstablishmentId { get; set; }
    public Guid? DqtSpecialismId { get; set; }

    public static async Task<MandatoryQualification> MapFromDqtQualification(dfeta_qualification qualification, ReferenceDataCache referenceDataCache)
    {
        var mqEstablishments = await referenceDataCache.GetMqEstablishments();
        var mqSpecialisms = await referenceDataCache.GetMqSpecialisms();

        return TrsDataSyncHelper.MapMandatoryQualificationFromDqtQualification(qualification, mqEstablishments, mqSpecialisms, applyMigrationMappings: true);
    }
}

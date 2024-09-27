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
}

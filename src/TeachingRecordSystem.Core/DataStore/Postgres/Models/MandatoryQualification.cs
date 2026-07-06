namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class MandatoryQualification : Qualification
{
    public MandatoryQualification()
    {
        QualificationType = QualificationType.MandatoryQualification;
    }

    public MandatoryQualificationProvider? Provider { get; }
    public required Guid? ProviderId { get; set; }
    public required MandatoryQualificationSpecialism? Specialism { get; set; }
    public required MandatoryQualificationStatus? Status { get; set; }
    public required DateOnly? StartDate { get; set; }
    public required DateOnly? EndDate { get; set; }

    public Guid? DqtQualificationId { get; set; }
    public Guid? DqtMqEstablishmentId { get; set; }
    public string? DqtMqEstablishmentValue { get; set; }
    public Guid? DqtSpecialismId { get; set; }
    public string? DqtSpecialismValue { get; set; }
}

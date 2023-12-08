namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class MandatoryQualification : Qualification
{
    // TODO Add Provider here when we've figured out how to model MQ Providers
    public MandatoryQualificationSpecialism? Specialism { get; set; }
    public MandatoryQualificationStatus? Status { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public Guid? DqtMqEstablishmentId { get; set; }
    public Guid? DqtSpecialismId { get; set; }
}

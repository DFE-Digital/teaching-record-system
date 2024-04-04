namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class PersonEmployment
{
    public const string PersonIdIndexName = "ix_person_employments_person_id";
    public const string EstablishmentIdIndexName = "ix_person_employments_establishment_id";

    public required Guid PersonEmploymentId { get; set; }
    public required Guid PersonId { get; set; }
    public required Guid? EstablishmentId { get; set; }
    public required DateOnly StartDate { get; set; }
    public required DateOnly? EndDate { get; set; }
    public required EmploymentType EmploymentType { get; set; }
    public required DateTime CreatedOn { get; set; }
    public required DateTime UpdatedOn { get; set; }
}

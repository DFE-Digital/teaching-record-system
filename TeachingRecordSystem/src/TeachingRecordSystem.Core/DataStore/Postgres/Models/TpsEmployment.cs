namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TpsEmployment
{
    public const string PersonIdIndexName = "ix_tps_employments_person_id";
    public const string EstablishmentIdIndexName = "ix_tps_employments_establishment_id";
    public const string KeyIndexName = "ix_tps_employments_key";

    public required Guid TpsEmploymentId { get; set; }
    public required Guid PersonId { get; set; }
    public required Guid EstablishmentId { get; set; }
    public required DateOnly StartDate { get; set; }
    public required DateOnly? EndDate { get; set; }
    public required DateOnly LastKnownTpsEmployedDate { get; set; }
    public required DateOnly LastExtractDate { get; set; }
    public required EmploymentType EmploymentType { get; set; }
    public required DateTime CreatedOn { get; set; }
    public required DateTime UpdatedOn { get; set; }
    public required string Key { get; set; }
    public required string? NationalInsuranceNumber { get; set; }
    public required string? PersonPostcode { get; set; }
    public required bool WithdrawalConfirmed { get; set; }
    public required string? PersonEmailAddress { get; set; }
    public required string? EmployerPostcode { get; set; }
    public required string? EmployerEmailAddress { get; set; }
}

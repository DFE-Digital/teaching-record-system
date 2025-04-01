namespace TeachingRecordSystem.Core.Services.WorkforceData;

public record UpdatedTpsEmploymentEmployerEmailAddress
{
    public required Guid TpsEmploymentId { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid EstablishmentId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required DateOnly LastKnownTpsEmployedDate { get; init; }
    public required EmploymentType EmploymentType { get; init; }
    public required bool WithdrawalConfirmed { get; init; }
    public required DateOnly LastExtractDate { get; init; }
    public required string? NationalInsuranceNumber { get; set; }
    public required string? PersonPostcode { get; set; }
    public required string? PersonEmailAddress { get; set; }
    public required string? EmployerPostcode { get; set; }
    public required string? CurrentEmployerEmailAddress { get; set; }
    public required string Key { get; init; }
    public required string? NewEmployerEmailAddress { get; init; }
}

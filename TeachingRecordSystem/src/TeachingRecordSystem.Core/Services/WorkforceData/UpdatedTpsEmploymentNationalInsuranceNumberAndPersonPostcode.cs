namespace TeachingRecordSystem.Core.Services.WorkforceData;

public record UpdatedTpsEmploymentNationalInsuranceNumberAndPersonPostcode
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
    public required string? CurrentNationalInsuranceNumber { get; init; }
    public required string? CurrentPersonPostcode { get; init; }
    public required string? NewNationalInsuranceNumber { get; init; }
    public required string? NewPersonPostcode { get; init; }
    public required string Key { get; init; }
}

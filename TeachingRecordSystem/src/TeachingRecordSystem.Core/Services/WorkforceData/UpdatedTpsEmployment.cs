namespace TeachingRecordSystem.Core.Services.WorkforceData;

public record UpdatedTpsEmployment
{
    public required Guid TpsEmploymentId { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid EstablishmentId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? CurrentEndDate { get; init; }
    public required DateOnly CurrentLastKnownTpsEmployedDate { get; init; }
    public required EmploymentType CurrentEmploymentType { get; init; }
    public required bool CurrentWithdrawalConfirmed { get; init; }
    public required DateOnly CurrentLastExtractDate { get; init; }
    public required string? CurrentNationalInsuranceNumber { get; init; }
    public required string? CurrentPersonPostcode { get; init; }
    public required string? CurrentPersonEmailAddress { get; init; }
    public required string? CurrentEmployerPostcode { get; init; }
    public required DateOnly? NewEndDate { get; init; }
    public required DateOnly NewLastKnownTpsEmployedDate { get; init; }
    public required EmploymentType NewEmploymentType { get; init; }
    public required bool NewWithdrawalConfirmed { get; init; }
    public required DateOnly NewLastExtractDate { get; init; }
    public required string? NewNationalInsuranceNumber { get; init; }
    public required string? NewPersonPostcode { get; init; }
    public required string? NewPersonEmailAddress { get; init; }
    public required string? NewEmployerPostcode { get; init; }
    public required string Key { get; init; }
}

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public record UpdatedPersonEmployment
{
    public required Guid PersonEmploymentId { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid EstablishmentId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required EmploymentType CurrentEmploymentType { get; init; }
    public required DateOnly CurrentLastKnownEmployedDate { get; init; }
    public required DateOnly CurrentLastExtractDate { get; init; }
    public required string? CurrentNationalInsuranceNumber { get; init; }
    public required string? CurrentPersonPostcode { get; init; }
    public required EmploymentType NewEmploymentType { get; init; }
    public required DateOnly NewLastKnownEmployedDate { get; init; }
    public required DateOnly NewLastExtractDate { get; init; }
    public required string? NewNationalInsuranceNumber { get; init; }
    public required string? NewPersonPostcode { get; init; }
    public required string Key { get; init; }
}

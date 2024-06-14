namespace TeachingRecordSystem.Core.Services.WorkforceData;

public record UpdatedPersonEmploymentEndDate
{
    public required Guid PersonEmploymentId { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid EstablishmentId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? CurrentEndDate { get; init; }
    public required EmploymentType EmploymentType { get; init; }
    public required DateOnly LastKnownEmployedDate { get; init; }
    public required DateOnly LastExtractDate { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required string? PersonPostcode { get; init; }
    public required string Key { get; init; }
    public required DateOnly? NewEndDate { get; init; }
}

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public record UpdatedPersonEmploymentEstablishment
{
    public required Guid PersonEmploymentId { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid CurrentEstablishmentId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required EmploymentType EmploymentType { get; init; }
    public required DateOnly LastKnownEmployedDate { get; init; }
    public required DateOnly LastExtractDate { get; init; }
    public required string? NationalInsuranceNumber { get; set; }
    public required string? PersonPostcode { get; set; }
    public required string Key { get; init; }
    public required Guid EstablishmentId { get; init; }
}

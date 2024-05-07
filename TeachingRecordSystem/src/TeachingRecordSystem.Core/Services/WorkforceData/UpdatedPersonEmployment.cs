namespace TeachingRecordSystem.Core.Services.WorkforceData;

public record UpdatedPersonEmployment
{
    public required Guid TpsCsvExtractItemId { get; init; }
    public required Guid PersonEmploymentId { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid EstablishmentId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required EmploymentType CurrentEmploymentType { get; init; }
    public required DateOnly CurrentLastKnownEmployedDate { get; init; }
    public required DateOnly CurrentLastExtractDate { get; init; }
    public required EmploymentType EmploymentType { get; init; }
    public required DateOnly LastKnownEmployedDate { get; init; }
    public required DateOnly LastExtractDate { get; init; }
    public required string Key { get; init; }
}

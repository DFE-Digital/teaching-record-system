namespace TeachingRecordSystem.Core.Services.WorkforceData;

public record UpdatedPersonEmployment
{
    public required Guid TpsCsvExtractItemId { get; set; }
    public required Guid PersonEmploymentId { get; init; }
    public required Guid PersonId { get; init; }
    public required Guid EstablishmentId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? CurrentEndDate { get; init; }
    public required EmploymentType CurrentEmploymentType { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required EmploymentType EmploymentType { get; init; }
}

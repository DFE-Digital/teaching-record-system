namespace TeachingRecordSystem.Core.Services.WorkforceData;

public record NewPersonEmployment
{
    public required Guid TpsCsvExtractItemId { get; set; }
    public required Guid PersonId { get; init; }
    public required Guid EstablishmentId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required EmploymentType EmploymentType { get; init; }
}

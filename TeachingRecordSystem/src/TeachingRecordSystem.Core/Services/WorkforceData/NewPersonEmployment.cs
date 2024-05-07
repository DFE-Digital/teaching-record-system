namespace TeachingRecordSystem.Core.Services.WorkforceData;

public record NewPersonEmployment
{
    public required Guid TpsCsvExtractItemId { get; set; }
    public required string Trn { get; set; }
    public required string LocalAuthorityCode { get; set; }
    public required string EstablishmentNumber { get; set; }
    public required Guid PersonId { get; init; }
    public required Guid EstablishmentId { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly LastKnownEmployedDate { get; init; }
    public required EmploymentType EmploymentType { get; init; }
    public required DateOnly LastExtractDate { get; set; }
    public required string Key { get; set; }
}

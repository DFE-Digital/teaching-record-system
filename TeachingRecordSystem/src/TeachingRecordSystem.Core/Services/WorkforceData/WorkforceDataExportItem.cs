namespace TeachingRecordSystem.Core.Services.WorkforceData;

public record WorkforceDataExportItem
{
    public required Guid TpsEmploymentId { get; init; }
    public required Guid PersonId { get; init; }
    public required string Trn { get; init; }
    public required Guid EstablishmentId { get; init; }
    public required string EstablishmentSource { get; init; }
    public required int? EstablishmentUrn { get; init; }
    public required string LocalAuthorityCode { get; init; }
    public required string? EstablishmentNumber { get; init; }
    public required string EstablishmentName { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required DateOnly LastKnownTpsEmployedDate { get; init; }
    public required string EmploymentType { get; init; }
    public required bool WithdrawalConfirmed { get; init; }
    public required DateOnly LastExtractDate { get; init; }
    public required string Key { get; init; }
    public required string NationalInsuranceNumber { get; init; }
    public required string? PersonPostcode { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required DateTime UpdatedOn { get; init; }
}

namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public record PersonDetailModel
{
    public required Guid PersonId { get; init; }
    public required bool ShowChangeLinks { get; init; }
    public required string? Trn { get; init; }
    public required string Name { get; init; }
    public required string[] PreviousNames { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required string? Gender { get; init; }
    public required string? Email { get; init; }
    public required string? MobileNumber { get; init; }
}

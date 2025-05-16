namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public record PersonDetailViewModel
{
    public required Guid PersonId { get; init; }
    public required PersonDetailViewModelOptions Options { get; init; }
    public required string? Trn { get; init; }
    public required string Name { get; init; }
    public required string[] PreviousNames { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required string? Gender { get; init; }
    public required string? Email { get; init; }
    public required string? MobileNumber { get; init; }
}

[Flags]
public enum PersonDetailViewModelOptions
{
    None = 0,
    ShowGender = 1 << 1,
    ShowEmail = 1 << 2,
    ShowMobileNumber = 1 << 3,
    ShowAll = ShowGender | ShowEmail | ShowMobileNumber
}

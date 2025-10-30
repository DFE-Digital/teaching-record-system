namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public record PersonDetailViewModel
{
    public required Guid PersonId { get; init; }
    public required PersonDetailViewModelOptions Options { get; init; }
    public required string Trn { get; init; }
    public required string Name { get; init; }
    public required string[] PreviousNames { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
    public required string? Email { get; init; }
    public required bool? IsActive { get; init; }
    public required bool CanChangeDetails { get; init; }
}

[Flags]
public enum PersonDetailViewModelOptions
{
    None = 0,
    ShowGender = 1 << 1,
    ShowEmail = 1 << 2,
    ShowStatus = 1 << 3,
    ShowAll = ShowGender | ShowEmail | ShowStatus
}

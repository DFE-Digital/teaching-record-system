namespace TeachingRecordSystem.Core.Models.SupportTaskData;

[SupportTaskData("3ca684d4-15de-4f12-b0fb-c5386360b171")]
public record NpqTrnRequestData
{
    public NpqTrnRequestDataPersonAttributes? SelectedPersonAttributes { get; init; }
    public NpqTrnRequestDataPersonAttributes? ResolvedAttributes { get; init; }
    public SupportRequestOutcome? SupportRequestOutcome { get; init; }
}

public record NpqTrnRequestDataPersonAttributes
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
}

namespace TeachingRecordSystem.Core.Models.SupportTaskData;

[SupportTaskData("37c27275-829c-4aa0-a47c-62a0092d0a71")]
public record ApiTrnRequestData
{
    public ApiTrnRequestDataPersonAttributes? SelectedPersonAttributes { get; init; }
    public ApiTrnRequestDataPersonAttributes? ResolvedAttributes { get; init; }
}

public record ApiTrnRequestDataPersonAttributes
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
}

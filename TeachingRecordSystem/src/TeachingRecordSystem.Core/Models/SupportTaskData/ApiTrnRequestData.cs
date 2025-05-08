namespace TeachingRecordSystem.Core.Models.SupportTaskData;

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
}

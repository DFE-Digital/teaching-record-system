namespace TeachingRecordSystem.Core.Models.SupportTaskData;

public record NpqTrnRequestData
{
    public NpqTrnRequestDataPersonAttributes? SelectedPersonAttributes { get; init; }
    public NpqTrnRequestDataPersonAttributes? ResolvedAttributes { get; init; }
}

public record NpqTrnRequestDataPersonAttributes
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
}

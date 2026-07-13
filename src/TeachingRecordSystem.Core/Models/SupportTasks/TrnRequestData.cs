namespace TeachingRecordSystem.Core.Models.SupportTasks;

public record TrnRequestData : ISupportTaskData
{
    public TrnRequestDataPersonAttributes? SelectedPersonAttributes { get; init; }
    public TrnRequestDataPersonAttributes? ResolvedAttributes { get; init; }
    string ISupportTaskData.GetOutcomeLabel() => "Resolved";
}

public record TrnRequestDataPersonAttributes
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
}

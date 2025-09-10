namespace TeachingRecordSystem.Core.Models.SupportTaskData;

[SupportTaskData("fdee6a10-6338-463a-b6df-e34a2b95a854")]
public record CapitaPotentialDuplicateData
{
    public CapitaPotentialDuplicateAttributes? SelectedPersonAttributes { get; init; }
    public CapitaPotentialDuplicateAttributes? ResolvedAttributes { get; init; }
    public required string FileName { get; set; }
    public required long IntegrationTransactionId { get; set; }
}

public record CapitaPotentialDuplicateAttributes
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
}

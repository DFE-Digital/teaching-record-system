namespace TeachingRecordSystem.Core.Models.SupportTasks;

public record TeacherPensionsPotentialDuplicateData : ISupportTaskData
{
    public TeacherPensionsPotentialDuplicateAttributes? SelectedPersonAttributes { get; init; }
    public TeacherPensionsPotentialDuplicateAttributes? ResolvedAttributes { get; init; }
    public required string FileName { get; init; }
    public required long IntegrationTransactionId { get; init; }
}

public record TeacherPensionsPotentialDuplicateAttributes
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
    public required string Trn { get; init; }
}

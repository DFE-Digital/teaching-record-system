namespace TeachingRecordSystem.Core.Models.SupportTaskData;

public record TeacherPensionsPotentialDuplicateData
{
    public TeacherPensionsPotentialDuplicateAttributes? SelectedPersonAttributes { get; init; }
    public TeacherPensionsPotentialDuplicateAttributes? ResolvedAttributes { get; init; }
    public required string FileName { get; set; }
    public required long IntegrationTransactionId { get; set; }
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

namespace TeachingRecordSystem.Core.Events.Models;

public record DqtInitialTeacherTraining
{
    public Guid? InitialTeacherTrainingId { get; init; }
    public string? SlugId { get; init; }
    public string? ProgrammeType { get; init; }
    public DateOnly? ProgrammeStartDate { get; init; }
    public DateOnly? ProgrammeEndDate { get; init; }
    public string? Result { get; init; }
    public string? QualificationName { get; init; }
    public string? QualificationValue { get; init; }
    public Guid? ProviderId { get; init; }
    public string? ProviderName { get; init; }
    public string? ProviderUkprn { get; init; }
    public string? CountryName { get; init; }
    public string? CountryValue { get; init; }
    public string? Subject1Name { get; init; }
    public string? Subject1Value { get; init; }
    public string? Subject2Name { get; init; }
    public string? Subject2Value { get; init; }
    public string? Subject3Name { get; init; }
    public string? Subject3Value { get; init; }
    public string? AgeRangeFrom { get; init; }
    public string? AgeRangeTo { get; init; }
}

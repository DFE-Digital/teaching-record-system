namespace TeachingRecordSystem.Core.Events.Models;

public record DqtQtsRegistration
{
    public Guid? QtsRegistrationId { get; init; }
    public string? TeacherStatusName { get; init; }
    public string? TeacherStatusValue { get; init; }
    public string? EarlyYearsStatusName { get; init; }
    public string? EarlyYearsStatusValue { get; init; }
    public DateOnly? QtsDate { get; init; }
    public DateOnly? EytsDate { get; init; }
    public DateOnly? PartialRecognitionDate { get; init; }
}

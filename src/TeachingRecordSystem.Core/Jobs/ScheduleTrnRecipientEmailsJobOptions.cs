namespace TeachingRecordSystem.Core.Jobs;

public class ScheduleTrnRecipientEmailsJobOptions
{
    public DateOnly EarliestRecordCreationDate { get; init; }
    public required string JobSchedule { get; init; }
    public required Guid[] RequestedByUserIds { get; init; } = [];
    public required int EmailDelayDays { get; init; } = 3;
}

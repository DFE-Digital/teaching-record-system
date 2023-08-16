namespace TeachingRecordSystem.Core.Jobs.Scheduling;

public class BatchSendQtsAwardedEmailsJobOptions
{
    public required DateTime InitialLastAwardedToUtc { get; init; }
    public required int EmailDelayDays { get; init; }
    public required string JobSchedule { get; init; }
}

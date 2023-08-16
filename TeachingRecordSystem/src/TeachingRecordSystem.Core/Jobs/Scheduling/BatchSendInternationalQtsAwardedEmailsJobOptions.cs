namespace TeachingRecordSystem.Core.Jobs.Scheduling;

public class BatchSendInternationalQtsAwardedEmailsJobOptions
{
    public required DateTime InitialLastAwardedToUtc { get; init; }
    public required int EmailDelayDays { get; init; }
    public required string JobSchedule { get; init; }
}

namespace TeachingRecordSystem.Core.Jobs;

public class BatchSendInductionCompletedEmailsJobOptions
{
    public required DateTime InitialLastPassedEndUtc { get; init; }
    public required int EmailDelayDays { get; init; }
    public required string JobSchedule { get; init; }
    public int MaxBatchDays { get; init; } = 7;
}

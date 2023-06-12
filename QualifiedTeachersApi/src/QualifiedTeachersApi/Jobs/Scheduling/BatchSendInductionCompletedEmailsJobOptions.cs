﻿namespace QualifiedTeachersApi.Jobs.Scheduling;

public class BatchSendInductionCompletedEmailsJobOptions
{
    public required DateTime InitialLastAwardedToUtc { get; init; }
    public required int EmailDelayDays { get; init; }
    public required string JobSchedule { get; init; }
}

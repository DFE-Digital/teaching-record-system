namespace QualifiedTeachersApi.Jobs.Scheduling;

public class RecurringJobsOptions
{ 
    public required BatchSendQtsAwardedEmailsJobOptions BatchSendQtsAwardedEmails { get; init; }
}

public class BatchSendQtsAwardedEmailsJobOptions
{
    public required DateTime InitialLastAwardedToUtc { get; init; }
    public required int EmailDelayDays { get; init; } = 3;
    public required string JobSchedule { get; init; }
}

namespace QualifiedTeachersApi.Jobs.Scheduling;

public class RecurringJobsOptions
{
    public required BatchSendQtsAwardedEmailsJobOptions BatchSendQtsAwardedEmails { get; init; }
}

namespace QualifiedTeachersApi.Jobs.Scheduling;

public class RecurringJobsOptions
{
    public required BatchSendQtsAwardedEmailsJobOptions BatchSendQtsAwardedEmails { get; init; }
    public required BatchSendInternationalQtsAwardedEmailsJobOptions BatchSendInternationalQtsAwardedEmails { get; init; }
    public required BatchSendEytsAwardedEmailsJobOptions BatchSendEytsAwardedEmails { get; init; }
}

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class InternationalQtsAwardedEmailsJobItem
{
    public const int EmailAddressMaxLength = 200;
    public const string TrnUniqueIndexName = "ix_international_qts_awarded_emails_job_items_trn";

    public required Guid InternationalQtsAwardedEmailsJobId { get; set; }
    public InternationalQtsAwardedEmailsJob? InternationalQtsAwardedEmailsJob { get; set; }
    public required Guid PersonId { get; set; }
    public required string Trn { get; set; }
    public required string EmailAddress { get; set; }
    public required Dictionary<string, string> Personalization { get; set; }
    public bool EmailSent { get; set; }
}

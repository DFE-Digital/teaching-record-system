namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class QtsAwardedEmailsJobItem
{
    public const int EmailAddressMaxLength = 200;
    public const string TrnUniqueIndexName = "ix_qts_awarded_emails_job_items_trn";

    public required Guid QtsAwardedEmailsJobId { get; set; }
    public QtsAwardedEmailsJob? QtsAwardedEmailsJob { get; set; }
    public required Guid PersonId { get; set; }
    public required string Trn { get; set; }
    public required string EmailAddress { get; set; }
    public required Dictionary<string, string> Personalization { get; set; }
    public bool EmailSent { get; set; }
}

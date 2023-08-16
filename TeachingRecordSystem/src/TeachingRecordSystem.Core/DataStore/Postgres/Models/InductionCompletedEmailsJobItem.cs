namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class InductionCompletedEmailsJobItem
{
    public const int EmailAddressMaxLength = 200;

    public required Guid InductionCompletedEmailsJobId { get; set; }
    public InductionCompletedEmailsJob? InductionCompletedEmailsJob { get; set; }
    public required Guid PersonId { get; set; }
    public required string Trn { get; set; }
    public required string EmailAddress { get; set; }
    public required Dictionary<string, string> Personalization { get; set; }
    public bool EmailSent { get; set; }
}

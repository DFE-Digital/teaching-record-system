namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class EytsAwardedEmailsJobItem
{
    public const int EmailAddressMaxLength = 200;

    public required Guid EytsAwardedEmailsJobId { get; set; }
    public EytsAwardedEmailsJob? EytsAwardedEmailsJob { get; set; }
    public required Guid PersonId { get; set; }
    public required string Trn { get; set; }
    public required string EmailAddress { get; set; }
    public required Dictionary<string, string> Personalization { get; set; }
    public bool EmailSent { get; set; }
}

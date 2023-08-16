namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class QtsAwardedEmailsJob
{
    public required Guid QtsAwardedEmailsJobId { get; set; }
    public required DateTime AwardedToUtc { get; set; }
    public required DateTime ExecutedUtc { get; set; }
    public List<QtsAwardedEmailsJobItem>? JobItems { get; set; }
}

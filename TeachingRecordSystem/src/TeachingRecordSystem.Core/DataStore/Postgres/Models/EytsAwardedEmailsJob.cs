namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class EytsAwardedEmailsJob
{
    public required Guid EytsAwardedEmailsJobId { get; set; }
    public required DateTime AwardedToUtc { get; set; }
    public required DateTime ExecutedUtc { get; set; }
    public List<EytsAwardedEmailsJobItem>? JobItems { get; set; }
}

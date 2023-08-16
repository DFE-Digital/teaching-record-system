namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class InductionCompletedEmailsJob
{
    public required Guid InductionCompletedEmailsJobId { get; set; }
    public required DateTime AwardedToUtc { get; set; }
    public required DateTime ExecutedUtc { get; set; }
    public List<InductionCompletedEmailsJobItem>? JobItems { get; set; }
}

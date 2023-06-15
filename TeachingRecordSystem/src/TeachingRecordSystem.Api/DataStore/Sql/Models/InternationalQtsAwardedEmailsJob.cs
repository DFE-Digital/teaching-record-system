namespace TeachingRecordSystem.Api.DataStore.Sql.Models;

public class InternationalQtsAwardedEmailsJob
{
    public required Guid InternationalQtsAwardedEmailsJobId { get; set; }
    public required DateTime AwardedToUtc { get; set; }
    public required DateTime ExecutedUtc { get; set; }
    public List<InternationalQtsAwardedEmailsJobItem>? JobItems { get; set; }
}

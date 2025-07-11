namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Email
{
    public required Guid EmailId { get; init; }
    public required string TemplateId { get; init; }
    public required string EmailAddress { get; init; }
    public required IDictionary<string, string> Personalization { get; init; }
    public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
    public DateTime? SentOn { get; set; }
}

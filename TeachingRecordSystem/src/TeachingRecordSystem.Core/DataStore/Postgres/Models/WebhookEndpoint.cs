namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class WebhookEndpoint
{
    public required Guid WebhookEndpointId { get; init; }
    public required Guid ApplicationUserId { get; init; }
    public required Uri Address { get; set; }
    public required string MinorVersion { get; set; }
    public required List<string> CloudEventTypes { get; set; }
    public required bool Enabled { get; set; }
}

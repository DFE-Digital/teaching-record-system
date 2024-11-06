namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class WebhookEndpoint
{
    public required Guid WebhookEndpointId { get; init; }
    public required Guid ApplicationUserId { get; init; }
    public required string Address { get; set; }
    public required string ApiVersion { get; set; }
    public required List<string> CloudEventTypes { get; set; }
    public required bool Enabled { get; set; }
}

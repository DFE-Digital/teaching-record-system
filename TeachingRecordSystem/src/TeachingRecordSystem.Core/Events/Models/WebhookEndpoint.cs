namespace TeachingRecordSystem.Core.Events.Models;

public record WebhookEndpoint
{
    public required Guid WebhookEndpointId { get; init; }
    public required Guid ApplicationUserId { get; init; }
    public required string Address { get; init; }
    public required string ApiVersion { get; init; }
    public required IReadOnlyCollection<string> CloudEventTypes { get; init; }
    public required bool Enabled { get; init; }

    public static WebhookEndpoint FromModel(Core.DataStore.Postgres.Models.WebhookEndpoint model) => new()
    {
        WebhookEndpointId = model.WebhookEndpointId,
        ApplicationUserId = model.ApplicationUserId,
        Address = model.Address,
        ApiVersion = model.ApiVersion,
        CloudEventTypes = model.CloudEventTypes,
        Enabled = model.Enabled
    };
}

namespace TeachingRecordSystem.Core.Services.Webhooks;

public class WebhookMessageDeliveryException(HttpResponseMessage response, string? rawRequest) :
    Exception($"Webhook message delivery failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}).")
{
    public string? RawRequest { get; } = rawRequest;
}

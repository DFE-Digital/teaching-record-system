namespace TeachingRecordSystem.Core.ApiSchema.V3;

public interface IWebhookMessageData
{
    static abstract string CloudEventType { get; }
}

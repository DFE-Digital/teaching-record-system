namespace TeachingRecordSystem.EndToEndTests.Infrastructure.Webhooks;

public class WebhookMessageRecorder
{
    private readonly List<HttpRequestMessage> _messages = [];

    public void Clear() => _messages.Clear();

    public void OnRequestReceived(HttpRequestMessage request)
    {
        _messages.Add(request);
    }

    public Task AssertMessagesReceivedAsync(params Func<HttpRequestMessage, Task>[] messageInspectors) =>
        Assert.CollectionAsync(_messages, messageInspectors);
}

namespace TeachingRecordSystem.Core.Services.Notify;

public class NoopNotificationSender : INotificationSender
{
    public Task SendEmailAsync(string templateId, string to, IReadOnlyDictionary<string, string> personalization) => Task.CompletedTask;

    public Task SendSmsAsync(string templateId, string to, IReadOnlyDictionary<string, string> personalization) => Task.CompletedTask;
}

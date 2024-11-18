namespace TeachingRecordSystem.Core.Services.Notify;

public interface INotificationSender
{
    Task SendEmailAsync(string templateId, string to, IReadOnlyDictionary<string, string> personalization);

    Task SendSmsAsync(string templateId, string to, IReadOnlyDictionary<string, string> personalization);
}

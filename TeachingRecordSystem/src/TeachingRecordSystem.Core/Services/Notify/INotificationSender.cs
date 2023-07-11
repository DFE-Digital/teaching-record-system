namespace TeachingRecordSystem.Core.Services.Notify;

public interface INotificationSender
{
    Task SendEmail(string templateId, string to, IReadOnlyDictionary<string, string> personalization);

    Task SendSms(string templateId, string to, IReadOnlyDictionary<string, string> personalization);
}

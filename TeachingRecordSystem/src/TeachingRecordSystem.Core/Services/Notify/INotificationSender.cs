namespace TeachingRecordSystem.Core.Services.Notify;

public interface INotificationSender
{
    Task<string> RenderEmailTemplateAsync(string templateId, IReadOnlyDictionary<string, string> personalization);

    Task SendEmailAsync(string templateId, string to, IReadOnlyDictionary<string, string> personalization);
}

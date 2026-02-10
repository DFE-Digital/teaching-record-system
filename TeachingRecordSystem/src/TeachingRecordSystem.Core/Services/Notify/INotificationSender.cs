namespace TeachingRecordSystem.Core.Services.Notify;

public interface INotificationSender
{
    Task<string> RenderEmailTemplateHtmlAsync(string templateId, IReadOnlyDictionary<string, string> personalization, bool stripLinks);

    Task SendEmailAsync(string templateId, string to, IReadOnlyDictionary<string, string> personalization);
}

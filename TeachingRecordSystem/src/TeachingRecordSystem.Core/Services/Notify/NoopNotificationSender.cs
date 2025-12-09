using System.Text;

namespace TeachingRecordSystem.Core.Services.Notify;

public class NoopNotificationSender : INotificationSender
{
    public Task<string> RenderEmailTemplateAsync(string templateId, IReadOnlyDictionary<string, string> personalization)
    {
        var sb = new StringBuilder();

        foreach (var (key, value) in personalization)
        {
            sb.AppendLine($"{key}: {value}");
        }

        return Task.FromResult(sb.ToString());
    }

    public Task SendEmailAsync(string templateId, string to, IReadOnlyDictionary<string, string> personalization) => Task.CompletedTask;
}

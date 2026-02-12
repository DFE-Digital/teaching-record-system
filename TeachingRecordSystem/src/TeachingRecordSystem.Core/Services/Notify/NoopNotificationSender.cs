using System.Text;
using System.Text.Encodings.Web;

namespace TeachingRecordSystem.Core.Services.Notify;

public class NoopNotificationSender : INotificationSender
{
    public Task<string> RenderEmailTemplateHtmlAsync(string templateId, IReadOnlyDictionary<string, string> personalization, bool stripLinks)
    {
        var encoder = HtmlEncoder.Default;
        var sb = new StringBuilder();

        foreach (var (key, value) in personalization)
        {
            sb.AppendLine($"{encoder.Encode(key)}: {encoder.Encode(value)}<br>");
        }

        return Task.FromResult(sb.ToString());
    }

    public Task SendEmailAsync(string templateId, string to, IReadOnlyDictionary<string, string> personalization) => Task.CompletedTask;
}

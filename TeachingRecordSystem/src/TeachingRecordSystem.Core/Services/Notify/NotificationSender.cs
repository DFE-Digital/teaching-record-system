using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notify.Client;

namespace TeachingRecordSystem.Core.Services.Notify;

public class NotificationSender : INotificationSender
{
    private readonly NotifyOptions _options;
    private readonly NotificationClient _notificationClient;
    private readonly NotificationClient? _noSendNotificationClient;
    private readonly ILogger<NotificationSender> _logger;

    public NotificationSender(IOptions<NotifyOptions> notifyOptionsAccessor, ILogger<NotificationSender> logger)
    {
        _options = notifyOptionsAccessor.Value;
        _notificationClient = new NotificationClient(_options.ApiKey);
        _logger = logger;

        if (_options.ApplyDomainFiltering && !string.IsNullOrEmpty(_options.NoSendApiKey))
        {
            _noSendNotificationClient = new NotificationClient(_options.NoSendApiKey);
        }
    }

    public async Task<string> RenderEmailTemplateAsync(string templateId, IReadOnlyDictionary<string, string> personalization)
    {
        var template = (await _notificationClient.GetTemplateByIdAsync(templateId)) ??
            throw new ArgumentException($"Template with ID '{templateId}' not found.", nameof(templateId));

        var rendered = template.body;
        foreach (var (key, value) in personalization)
        {
            rendered = rendered.Replace($"(({key}))", value);
        }

        return rendered;
    }

    public async Task SendEmailAsync(string templateId, string to, IReadOnlyDictionary<string, string> personalization)
    {
        NotificationClient client = _notificationClient;

        if (_options.ApplyDomainFiltering)
        {
            var toDomain = to[(to.IndexOf('@') + 1)..];

            if (!_options.DomainAllowList.Contains(toDomain))
            {
                // Domain is not in allow list, use the 'no send' client instead if we have one

                if (_noSendNotificationClient is not null)
                {
                    _logger.LogDebug("Email {Email} does not have domain in the allow list; using the 'no send' client.", to);
                    client = _noSendNotificationClient;
                }
                else
                {
                    _logger.LogInformation("Email {Email} does not have domain in the allow list; skipping send.", to);
                    return;
                }
            }
        }

        try
        {
            await client.SendEmailAsync(
                to,
                templateId,
                personalisation: personalization.ToDictionary(kvp => kvp.Key, kvp => (dynamic)kvp.Value));

            _logger.LogInformation("Successfully sent email to {Email}.", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed sending email to {Email}.", to);

            throw;
        }
    }
}

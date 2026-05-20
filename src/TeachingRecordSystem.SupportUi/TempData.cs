using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TeachingRecordSystem.SupportUi;

public static class TempDataKeys
{
    public const string FlashSuccess = "FlashSuccess";
}

public static class TempDataExtensions
{
    public static void SetFlashNotificationBanner(
        this ITempDataDictionary tempData,
        string? heading = null,
        string? messageText = null,
        Action<IHtmlContentBuilder>? buildMessageHtml = null,
        NotificationBannerType notificationBannerType = NotificationBannerType.Success)
    {
        if (messageText is not null && buildMessageHtml is not null)
        {
            throw new ArgumentException($"Cannot set both {nameof(messageText)} and {nameof(buildMessageHtml)}.");
        }

        string? messageHtml = null;
        if (buildMessageHtml is not null)
        {
            var builder = new HtmlContentBuilder();
            buildMessageHtml(builder);
            messageHtml = builder.ToHtmlString(HtmlEncoder.Default);
        }

        tempData.Add(
            TempDataKeys.FlashSuccess,
            new FlashNotificationBannerData
            {
                Heading = heading,
                Message = messageText,
                MessageHtml = messageHtml,
                NotificationBannerType = notificationBannerType
            }.Serialize());
    }

    public static bool TryGetFlashNotificationBanner(
        this ITempDataDictionary tempData,
        [NotNullWhen(true)] out (string? Heading, string? MessageText, string? MessageHtml, NotificationBannerType NotificationBannerType)? result)
    {
        if (tempData.TryGetValue(TempDataKeys.FlashSuccess, out object? flashSuccessObject) && flashSuccessObject is string flashSuccessString)
        {
            var data = FlashNotificationBannerData.Deserialize(flashSuccessString);
            result = (data.Heading, MessageText: data.Message, data.MessageHtml, data.NotificationBannerType);
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    private class FlashNotificationBannerData
    {
        public required string? Heading { get; init; }
        public required string? Message { get; init; }
        public required string? MessageHtml { get; init; }
        public required NotificationBannerType NotificationBannerType { get; init; }

        public string Serialize() => JsonSerializer.Serialize(this);

        public static FlashNotificationBannerData Deserialize(string serialized) =>
            JsonSerializer.Deserialize<FlashNotificationBannerData>(serialized) ??
                throw new ArgumentException($"Serialized {nameof(FlashNotificationBannerData)} is not valid.", nameof(serialized));
    }
}

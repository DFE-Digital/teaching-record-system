using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TeachingRecordSystem.SupportUi;

public static class TempDataKeys
{
    public const string FlashSuccess = "FlashSuccess";
}

public static class TempDataExtensions
{
    public static void SetFlashSuccess(
        this ITempDataDictionary tempData,
        string? heading = null,
        string? messageText = null,
        Action<IHtmlContentBuilder>? buildMessageHtml = null)
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
            new FlashSuccessData()
            {
                Heading = heading,
                Message = messageText,
                MessageHtml = messageHtml
            }.Serialize());
    }

    public static void SetFlashSuccessWithLinkToRecord(
        this ITempDataDictionary tempData,
        string heading,
        string link)
    {
        tempData.SetFlashSuccess(
            heading,
            buildMessageHtml: htmlBuilder =>
            {
                var linkTag = new TagBuilder("a");
                linkTag.AddCssClass("govuk-link");
                linkTag.MergeAttribute("href", link);
                linkTag.MergeAttribute("target", "_blank");
                linkTag.MergeAttribute("rel", "noopener noreferrer");
                linkTag.InnerHtml.Append("View record (opens in a new tab)");
                htmlBuilder.AppendHtml(linkTag);
            });
    }

    public static bool TryGetFlashSuccess(
        this ITempDataDictionary tempData,
        [NotNullWhen(true)] out (string? Heading, string? MessageText, string? MessageHtml)? result)
    {
        if (tempData.TryGetValue(TempDataKeys.FlashSuccess, out object? flashSuccessObject) && flashSuccessObject is string flashSuccessString)
        {
            var data = FlashSuccessData.Deserialize(flashSuccessString);
            result = (data.Heading, MessageText: data.Message, data.MessageHtml);
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    private class FlashSuccessData
    {
        public required string? Heading { get; init; }
        public required string? Message { get; init; }
        public required string? MessageHtml { get; init; }

        public string Serialize() => JsonSerializer.Serialize(this);

        public static FlashSuccessData Deserialize(string serialized) =>
            JsonSerializer.Deserialize<FlashSuccessData>(serialized) ??
                throw new ArgumentException($"Serialized {nameof(FlashSuccessData)} is not valid.", nameof(serialized));
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
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
        string? messageHtml = null)
    {
        if (messageText is not null && messageHtml is not null)
        {
            throw new ArgumentException($"Cannot set both {nameof(messageText)} and {nameof(messageHtml)}.");
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

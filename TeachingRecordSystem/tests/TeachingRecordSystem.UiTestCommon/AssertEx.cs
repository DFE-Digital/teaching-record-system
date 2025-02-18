using AngleSharp.Html.Dom;
using Xunit;
using Xunit.Sdk;

namespace TeachingRecordSystem.UiTestCommon;

public static partial class AssertEx
{
    public static async Task<IHtmlDocument> HtmlResponseAsync(HttpResponseMessage response, int expectedStatusCode = 200)
    {
        Assert.Equal(expectedStatusCode, (int)response.StatusCode);

        return await response.GetDocumentAsync();
    }

    public static void HtmlDocumentHasError(IHtmlDocument doc, string fieldName, string expectedMessage)
    {
        var errorElementId = $"{fieldName}-error";
        var errorElement = doc.GetElementById(errorElementId);

        if (errorElement == null)
        {
            throw new XunitException($"No error found for field '{fieldName}'.");
        }

        var vht = errorElement.GetElementsByTagName("span")[0];
        var errorMessage = errorElement.InnerHtml[vht.OuterHtml.Length..];
        Assert.Equal(expectedMessage, errorMessage);
    }

    public static void HtmlDocumentHasFlashSuccess(IHtmlDocument doc, string expectedMessage)
    {
        var banner = doc.GetElementsByClassName("govuk-notification-banner--success").SingleOrDefault();

        if (banner is null)
        {
            throw new XunitException("No notification banner found.");
        }

        var message = banner.GetElementsByClassName("govuk-notification-banner__heading").SingleOrDefault();

        Assert.Equal(expectedMessage, message?.TextContent?.Trim());
    }

    public static async Task HtmlResponseHasErrorAsync(
        HttpResponseMessage response,
        string fieldName,
        string expectedMessage,
        int expectedStatusCode = 400)
    {
        var doc = await HtmlResponseAsync(response, expectedStatusCode);
        HtmlDocumentHasError(doc, fieldName, expectedMessage);
    }

    public static void AssertRowContentMatches(this IHtmlDocument doc, string keyContent, string expected)
    {
        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TextContent == keyContent);
        Assert.NotNull(label);
        var value = label.NextElementSibling;
        Assert.Equal(expected, value!.TextContent.Trim());
    }
}

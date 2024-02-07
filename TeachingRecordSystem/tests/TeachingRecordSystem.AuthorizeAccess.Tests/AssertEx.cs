using AngleSharp.Html.Dom;
using Xunit.Sdk;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public static partial class AssertEx
{
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

    public static async Task HtmlResponseHasError(
        HttpResponseMessage response,
        string fieldName,
        string expectedMessage,
        int expectedStatusCode = 400)
    {
        Assert.Equal(expectedStatusCode, (int)response.StatusCode);

        var doc = await response.GetDocument();
        HtmlDocumentHasError(doc, fieldName, expectedMessage);
    }
}

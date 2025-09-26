using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Http;
using Moq;
using TeachingRecordSystem.Core.Services.Files;
using Xunit;
using Xunit.Sdk;

namespace TeachingRecordSystem.UiTestCommon;

#pragma warning disable CA1711
public static partial class AssertEx
#pragma warning restore CA1711
{
    public static void ResponseIsRedirectTo(HttpResponseMessage response, string expectedUrl)
    {
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        Assert.Equal(expectedUrl, location);
    }

    public static async Task<IHtmlDocument> HtmlResponseAsync(HttpResponseMessage response, int expectedStatusCode = 200)
    {
        Assert.Equal(expectedStatusCode, (int)response.StatusCode);

        return await response.GetDocumentAsync();
    }

    public static void HtmlDocumentHasError(IHtmlDocument doc, string fieldName, string expectedMessage)
    {
        var errorElementId = $"{fieldName.Replace('.', '_')}-error";
        var errorElement = doc.GetElementById(errorElementId);

        if (errorElement == null)
        {
            throw new XunitException($"No error found for field '{fieldName}'.");
        }

        var vht = errorElement.GetElementsByTagName("span")[0];
        var errorMessage = errorElement.InnerHtml.Replace(vht.OuterHtml, "").Trim();
        Assert.Equal(expectedMessage, errorMessage);
    }

    public static void HtmlDocumentHasSummaryError(IHtmlDocument doc, string expectedMessage)
    {
        var errorElement = doc.QuerySelectorAll(".govuk-error-summary .govuk-error-summary__list li")
            .SingleOrDefault(item => item.TrimmedText() == expectedMessage);

        if (errorElement == null)
        {
            throw new XunitException($"Error message '{expectedMessage}' was not found within the error summary.");
        }
    }

    public static void HtmlDocumentHasFlashSuccess(IHtmlDocument doc, string? expectedHeading = null, string? expectedMessage = null)
    {
        var banner = doc.GetElementsByClassName("govuk-notification-banner--success").SingleOrDefault();

        if (banner is null)
        {
            throw new XunitException("No notification banner found.");
        }

        if (expectedHeading != null)
        {
            var heading = banner.GetElementsByClassName("govuk-notification-banner__heading").SingleOrDefault();

            Assert.Equal(expectedHeading, heading?.TrimmedText());
        }

        if (expectedMessage != null)
        {
            var message = string.Join("\n", banner.QuerySelectorAll(".govuk-notification-banner p").Select(e => e.TrimmedText()));

            Assert.Equal(expectedMessage, message);
        }
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

    public static async Task HtmlResponseHasSummaryErrorAsync(
        HttpResponseMessage response,
        string expectedMessage,
        int expectedStatusCode = 400)
    {
        var doc = await HtmlResponseAsync(response, expectedStatusCode);
        HtmlDocumentHasSummaryError(doc, expectedMessage);
    }

    public static void AssertRowContentMatches(this IElement doc, string keyContent, string? expected)
    {
        doc.AssertRow(keyContent, el => Assert.Equal(expected, el.TrimmedText()));
    }

    public static void AssertRowContentMatches(this IHtmlDocument doc, string keyContent, string? expected)
    {
        doc.Body!.AssertRowContentMatches(keyContent, expected);
    }

    public static void AssertRowContentContains(this IElement doc, string keyContent, string expected)
    {
        doc.AssertRow(keyContent, el => Assert.Contains(expected, el.TrimmedText()));
    }

    public static void AssertRowContentContains(this IHtmlDocument doc, string keyContent, string expected)
    {
        doc.Body!.AssertRowContentContains(keyContent, expected);
    }

    public static void AssertRowContentMatches(this IElement doc, string keyContent, IEnumerable<string> expected)
    {
        doc.AssertRow(keyContent, el =>
        {
            var values = el.QuerySelectorAll("li").Select(d => d.TrimmedText());
            Assert.NotEmpty(values);
            Assert.Equal(expected, values);
        });
    }

    public static void AssertRowContentMatches(this IHtmlDocument doc, string keyContent, IEnumerable<string> expected)
    {
        doc.Body!.AssertRowContentMatches(keyContent, expected);
    }

    public static void AssertRowContentMatches(this IElement doc, string containerTestId, string keyContent, string? expected)
    {
        doc.AssertRow(containerTestId, keyContent, el => Assert.Equal(expected, el.TrimmedText()));
    }

    public static void AssertRowContentMatches(this IHtmlDocument doc, string containerTestId, string keyContent, string? expected)
    {
        doc.Body!.AssertRowContentMatches(containerTestId, keyContent, expected);
    }

    public static void AssertRowContentContains(this IElement doc, string containerTestId, string keyContent, string expected)
    {
        doc.AssertRow(containerTestId, keyContent, el => Assert.Contains(expected, el.TrimmedText()));
    }

    public static void AssertRowContentContains(this IHtmlDocument doc, string containerTestId, string keyContent, string expected)
    {
        doc.Body!.AssertRowContentContains(containerTestId, keyContent, expected);
    }

    public static void AssertRowContentMatches(this IElement doc, string containerTestId, string keyContent, IEnumerable<string> expected)
    {
        doc.AssertRow(containerTestId, keyContent, el =>
        {
            var values = el.QuerySelectorAll("li").Select(d => d.TrimmedText());
            Assert.NotEmpty(values);
            Assert.Equal(expected, values);
        });
    }

    public static void AssertRowContentMatches(this IHtmlDocument doc, string containerTestId, string keyContent, IEnumerable<string> expected)
    {
        doc.Body!.AssertRowContentMatches(containerTestId, keyContent, expected);
    }

    public static void AssertRow(this IElement doc, string keyContent, Action<IElement> valueAssertion)
    {
        doc.AssertRowCore(null, keyContent, valueAssertion);
    }

    public static void AssertRow(this IHtmlDocument doc, string keyContent, Action<IElement> valueAssertion)
    {
        doc.Body!.AssertRow(keyContent, valueAssertion);
    }

    public static void AssertRow(this IElement doc, string containerTestId, string keyContent, Action<IElement> valueAssertion)
    {
        doc.AssertRowCore(containerTestId, keyContent, valueAssertion);
    }

    public static void AssertRow(this IHtmlDocument doc, string containerTestId, string keyContent, Action<IElement> valueAssertion)
    {
        doc.Body!.AssertRow(containerTestId, keyContent, valueAssertion);
    }

    public static void AssertRow<T>(this IElement doc, string keyContent, Action<T> valueAssertion)
    {
        doc.AssertRowCore(null, keyContent, valueAssertion);
    }

    public static void AssertRow<T>(this IHtmlDocument doc, string keyContent, Action<T> valueAssertion)
    {
        doc.Body!.AssertRow(keyContent, valueAssertion);
    }

    public static void AssertRow<T>(this IElement doc, string containerTestId, string keyContent, Action<T> valueAssertion)
    {
        doc.AssertRowCore(containerTestId, keyContent, valueAssertion);
    }

    public static void AssertRow<T>(this IHtmlDocument doc, string containerTestId, string keyContent, Action<T> valueAssertion)
    {
        doc.Body!.AssertRow(containerTestId, keyContent, valueAssertion);
    }

    public static void AssertRows(this IElement doc, string keyContent, params Action<IElement>[] valueAssertions)
    {
        doc.AssertRowsCore(null, keyContent, valueAssertions);
    }

    public static void AssertRows(this IHtmlDocument doc, string keyContent, params Action<IElement>[] valueAssertions)
    {
        doc.Body!.AssertRows(keyContent, valueAssertions);
    }

    public static void AssertRows(this IElement doc, string containerTestId, string keyContent, params Action<IElement>[] valueAssertions)
    {
        doc.AssertRowsCore(containerTestId, keyContent, valueAssertions);
    }

    public static void AssertRows(this IHtmlDocument doc, string containerTestId, string keyContent, params Action<IElement>[] valueAssertions)
    {
        doc.Body!.AssertRows(containerTestId, keyContent, valueAssertions);
    }

    public static void AssertRows<T>(this IElement doc, string keyContent, params Action<T>[] valueAssertions)
    {
        doc.AssertRowsCore(null, keyContent, valueAssertions);
    }

    public static void AssertRows<T>(this IHtmlDocument doc, string keyContent, params Action<T>[] valueAssertions)
    {
        doc.Body!.AssertRows(keyContent, valueAssertions);
    }

    public static void AssertRows<T>(this IElement doc, string containerTestId, string keyContent, params Action<T>[] valueAssertions)
    {
        doc.AssertRowsCore(containerTestId, keyContent, valueAssertions);
    }

    public static void AssertRows<T>(this IHtmlDocument doc, string containerTestId, string keyContent, params Action<T>[] valueAssertions)
    {
        doc.Body!.AssertRows(containerTestId, keyContent, valueAssertions);
    }

    public static void AssertRowDoesNotExist(this IElement doc, string keyContent)
    {
        doc.AssertRowDoesNotExistCore(null, keyContent);
    }

    public static void AssertRowDoesNotExist(this IHtmlDocument doc, string keyContent)
    {
        doc.Body!.AssertRowDoesNotExist(keyContent);
    }

    public static void AssertRowDoesNotExist(this IElement doc, string containerTestId, string keyContent)
    {
        doc.AssertRowDoesNotExistCore(containerTestId, keyContent);
    }

    public static void AssertRowDoesNotExist(this IHtmlDocument doc, string containerTestId, string keyContent)
    {
        doc.Body!.AssertRowDoesNotExist(containerTestId, keyContent);
    }

    public static void AssertMatchRowHasExpectedHighlight(this IElement doc, string detailsId, string summaryListKey, bool expectHighlight)
    {
        var details = doc.GetAllElementsByTestId(detailsId).First();
        var valueElement = details.GetSummaryListValueElementForKey(summaryListKey);
        Assert.NotNull(valueElement);
        var highlightElement = valueElement.GetElementsByClassName("hods-highlight").SingleOrDefault();

        if (expectHighlight)
        {
            Assert.NotNull(highlightElement);
        }
        else
        {
            Assert.Null(highlightElement);
        }
    }

    public static void AssertMatchRowHasExpectedHighlight(this IHtmlDocument doc, string detailsId, string summaryListKey, bool expectHighlight)
    {
        doc.Body!.AssertMatchRowHasExpectedHighlight(detailsId, summaryListKey, expectHighlight);
    }

    public static void AssertChangeLinkExists(this IElement doc, string keyContent)
    {
        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == keyContent);
        Assert.NotNull(label);
        var value = label.NextElementSibling;
        Assert.NotNull(value!.NextElementSibling!.GetElementsByTagName("a").First());
    }

    public static void AssertChangeLinkExists(this IHtmlDocument doc, string keyContent)
    {
        doc.Body!.AssertChangeLinkExists(keyContent);
    }

    public static void AssertNoChangeLink(this IElement doc, string keyContent)
    {
        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == keyContent);
        Assert.NotNull(label);
        var value = label.NextElementSibling;
        Assert.Null(value!.NextElementSibling);
    }

    public static void AssertNoChangeLink(this IHtmlDocument doc, string keyContent)
    {
        doc.Body!.AssertNoChangeLink(keyContent);
    }

    public static async Task<Guid> AssertFileWasUploadedAsync(this Mock<IFileService> fileServiceMock)
    {
        fileServiceMock.Verify(mock => mock.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string?>(), null), Times.Once);
        return await Assert.IsType<Task<Guid>>(fileServiceMock.Invocations.FirstOrDefault(i => i.Method.Name == "UploadFileAsync")?.ReturnValue);
    }

    public static void AssertFileWasNotUploaded(this Mock<IFileService> fileServiceMock)
    {
        fileServiceMock.Verify(mock => mock.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string?>(), null), Times.Never);
    }

    public static void AssertFileWasDeleted(this Mock<IFileService> fileServiceMock, Guid fileId)
    {
        fileServiceMock.Verify(mock => mock.DeleteFileAsync(fileId));
    }

    private static void AssertRowCore<T>(this IElement doc, string? containerTestId, string keyContent, Action<T> valueAssertion)
    {
        IParentNode? container = containerTestId is null ? doc : doc.GetAllElementsByTestId(containerTestId).SingleOrDefault();
        Assert.NotNull(container);

        var label = container.QuerySelectorAll(".govuk-summary-list__key").SingleOrDefault(e => e.TrimmedText() == keyContent);
        AssertRowValueCore(label, valueAssertion);
    }

    private static void AssertRowsCore<T>(this IElement doc, string? containerTestId, string keyContent, params Action<T>[] valueAssertions)
    {
        IParentNode? container = containerTestId is null ? doc : doc.GetAllElementsByTestId(containerTestId).SingleOrDefault();
        Assert.NotNull(container);

        var labels = container.QuerySelectorAll(".govuk-summary-list__key").Where(e => e.TrimmedText() == keyContent);
        Assert.Collection(labels, valueAssertions
                .AsEnumerable()
                .Select<Action<T>, Action<IElement>>(assertion => (label => AssertRowValueCore(label, assertion)))
                .ToArray());
    }

    private static void AssertRowValueCore<T>(IElement? label, Action<T> valueAssertion)
    {
        Assert.NotNull(label);
        var element = label.NextElementSibling;
        Assert.NotNull(element);
        var value = Assert.IsAssignableFrom<T>(element);
        valueAssertion(value);
    }

    private static void AssertRowDoesNotExistCore(this IElement doc, string? containerTestId, string keyContent)
    {
        IParentNode? container = containerTestId is null ? doc : doc.GetAllElementsByTestId(containerTestId).SingleOrDefault();
        Assert.NotNull(container);

        var label = container.QuerySelectorAll(".govuk-summary-list__key").SingleOrDefault(e => e.TrimmedText() == keyContent);
        Assert.Null(label);
    }
}

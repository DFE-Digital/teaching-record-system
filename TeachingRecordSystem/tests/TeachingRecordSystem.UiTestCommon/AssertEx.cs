using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Moq;
using TeachingRecordSystem.Core.Services.Files;
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

    public static void AssertRowContentMatches(this IHtmlDocument doc, string keyContent, string expected)
    {
        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == keyContent);
        Assert.NotNull(label);
        var value = label.NextElementSibling;
        Assert.Equal(expected, value!.TrimmedText());
    }

    public static void AssertRowContentMatches(this IHtmlDocument doc, string keyContent, IEnumerable<string> expected)
    {
        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == keyContent);
        Assert.NotNull(label);
        var value = label.NextElementSibling!.QuerySelectorAll("li").Select(d => d.TrimmedText());
        Assert.NotEmpty(value);
        Assert.Equal(expected, value);
    }

    public static void AssertChangeLinkExists(this IHtmlDocument doc, string keyContent)
    {
        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == keyContent);
        Assert.NotNull(label);
        var value = label.NextElementSibling;
        Assert.NotNull(value!.NextElementSibling!.GetElementsByTagName("a").First());
    }

    public static void AssertNoChangeLink(this IHtmlDocument doc, string keyContent)
    {
        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == keyContent);
        Assert.NotNull(label);
        var value = label.NextElementSibling;
        Assert.Null(value!.NextElementSibling);
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

    public static void AssertSummaryListValue(this IHtmlDocument doc, string keyContent, Action<IElement> valueAssertion)
    {
        doc.AssertSummaryListValueCore(null, keyContent, valueAssertion);
    }

    public static void AssertSummaryListValue(this IHtmlDocument doc, string? containerTestId, string keyContent, Action<IElement> valueAssertion)
    {
        doc.AssertSummaryListValueCore(containerTestId, keyContent, valueAssertion);
    }

    public static void AssertSummaryListValue<T>(this IHtmlDocument doc, string keyContent, Action<T> valueAssertion)
    {
        doc.AssertSummaryListValueCore(null, keyContent, valueAssertion);
    }

    public static void AssertSummaryListValue<T>(this IHtmlDocument doc, string? containerTestId, string keyContent, Action<T> valueAssertion)
    {
        doc.AssertSummaryListValueCore(containerTestId, keyContent, valueAssertion);
    }

    public static void AssertSummaryListValues(this IHtmlDocument doc, string keyContent, params Action<IElement>[] valueAssertions)
    {
        doc.AssertSummaryListValuesCore(null, keyContent, valueAssertions);
    }

    public static void AssertSummaryListValues(this IHtmlDocument doc, string containerTestId, string keyContent, params Action<IElement>[] valueAssertions)
    {
        doc.AssertSummaryListValuesCore(containerTestId, keyContent, valueAssertions);
    }

    public static void AssertSummaryListValues<T>(this IHtmlDocument doc, string keyContent, params Action<T>[] valueAssertions)
    {
        doc.AssertSummaryListValuesCore(null, keyContent, valueAssertions);
    }

    public static void AssertSummaryListValues<T>(this IHtmlDocument doc, string containerTestId, string keyContent, params Action<T>[] valueAssertions)
    {
        doc.AssertSummaryListValuesCore(containerTestId, keyContent, valueAssertions);
    }

    public static void AssertSummaryListRowDoesNotExist(this IHtmlDocument doc, string keyContent)
    {
        doc.AssertSummaryListRowDoesNotExistCore(null, keyContent);
    }

    public static void AssertSummaryListRowDoesNotExist(this IHtmlDocument doc, string containerTestId, string keyContent)
    {
        doc.AssertSummaryListRowDoesNotExistCore(containerTestId, keyContent);
    }

    private static void AssertSummaryListValueCore<T>(this IHtmlDocument doc, string? containerTestId, string keyContent, Action<T> valueAssertion)
    {
        IParentNode? container = containerTestId is null ? doc : doc.GetAllElementsByTestId(containerTestId).SingleOrDefault();
        Assert.NotNull(container);

        var label = container.QuerySelectorAll(".govuk-summary-list__key").SingleOrDefault(e => e.TrimmedText() == keyContent);
        AssertRowValueCore<T>(label, valueAssertion);
    }

    private static void AssertSummaryListValuesCore<T>(this IHtmlDocument doc, string? containerTestId, string keyContent, params Action<T>[] valueAssertions)
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

    private static void AssertSummaryListRowDoesNotExistCore(this IHtmlDocument doc, string? containerTestId, string keyContent)
    {
        IParentNode? container = containerTestId is null ? doc : doc.GetAllElementsByTestId(containerTestId).SingleOrDefault();
        Assert.NotNull(container);

        var label = container.QuerySelectorAll(".govuk-summary-list__key").SingleOrDefault(e => e.TrimmedText() == keyContent);
        Assert.Null(label);
    }
}

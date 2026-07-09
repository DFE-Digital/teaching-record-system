using AngleSharp.Html.Dom;
using Xunit.Sdk;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public partial class ChangeHistoryTests(HostFixture hostFixture) : TestBase(hostFixture)
{
}


internal static class ChangeHistoryTestExtensions
{
    public static void AssertHasChangeHistoryEntry(
        this IHtmlDocument doc,
        Guid processId,
        string expectedTitle,
        string expectedUserName,
        DateTime expectedTimestamp,
        params (string Key, string? Value)[] expectedSummaryListRows)
    {
        AssertHasChangeHistoryEntry(
            doc,
            processId,
            expectedTitle,
            expectedUserName,
            expectedTimestamp,
            expectedSummaryListRows,
            expectedPreviousDataSummaryListRows: []);
    }

    public static void AssertHasChangeHistoryEntry(
        this IHtmlDocument doc,
        Guid processId,
        string expectedTitle,
        string expectedUserName,
        DateTime expectedTimestamp,
        IReadOnlyCollection<(string Key, string? Value)> expectedSummaryListRows,
        IReadOnlyCollection<(string Key, string? Value)> expectedPreviousDataSummaryListRows)
    {
        var changeHistoryItem = doc.GetElementByDataAttribute("data-process-id", processId.ToString());
        if (changeHistoryItem is null)
        {
            throw new XunitException($"Element with data-process-id=\"{processId}\" not found.");
        }

        var title = changeHistoryItem.GetElementsByClassName("moj-timeline__title").SingleOrDefault();
        Assert.Equal(expectedTitle, title?.TrimmedText());

        var date = changeHistoryItem.GetElementsByClassName("moj-timeline__date").SingleOrDefault();
        var expectedDateBlock = $"By {expectedUserName} on {expectedTimestamp:d MMMMM yyyy 'at' h:mm tt}";
        Assert.Equal(expectedDateBlock, date?.TrimmedText().ReplaceLineEndings(" "), ignoreAllWhiteSpace: true);

        if (expectedSummaryListRows.Count > 0)
        {
            var description = changeHistoryItem.GetElementsByClassName("moj-timeline__description").SingleOrDefault()?.FirstElementChild;
            if (description is null)
            {
                throw new XunitException("Element with class=\"moj-timeline__description\" not found.");
            }

            description.AssertSummaryListHasRows(
                expectedSummaryListRows.Select(e => e with { Value = e.Value ?? WebConstants.EmptyFallbackContent }).ToArray());
        }

        if (expectedPreviousDataSummaryListRows.Count > 0)
        {
            var previousDetails = changeHistoryItem.GetElementByTestId("previous-data")?.GetElementsByClassName("govuk-summary-list").SingleOrDefault();
            if (previousDetails is null)
            {
                throw new XunitException("Cannot find Previous Details summary list.");
            }

            previousDetails.AssertSummaryListHasRows(
                expectedPreviousDataSummaryListRows.Select(e => e with { Value = e.Value ?? WebConstants.EmptyFallbackContent }).ToArray());
        }
    }
}

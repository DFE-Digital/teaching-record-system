using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests;

public static class ProcessTimelineTestHelper
{
    public static string[] GetProcessTypes(IHtmlDocument doc) =>
        doc.QuerySelectorAll(".moj-timeline__title").Select(e => e.TrimmedText()).ToArray();

    public static string[] GetEventNames(IHtmlDocument doc) =>
        doc.GetAllElementsByTestId("timeline-item-event-name").Select(e => e.TrimmedText()).ToArray();

    // An unordered query over a process's events comes back in event ID order; these give a test rows whose
    // IDs sort the opposite way to the order the page should show them in, so an assertion on that order
    // can only pass if the page is really sorting by timestamp
    public static Guid LowSortingId() => MakeId('0');

    public static Guid HighSortingId() => MakeId('f');

    private static Guid MakeId(char firstDigit) => new(firstDigit + Guid.NewGuid().ToString("N")[1..]);
}

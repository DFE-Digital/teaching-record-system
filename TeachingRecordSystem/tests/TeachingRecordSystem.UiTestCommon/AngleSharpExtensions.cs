using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Xunit;

namespace TeachingRecordSystem.UiTestCommon;

public static class AngleSharpExtensions
{
    public static T? As<T>(this IElement element)
        where T : class, IElement
    {
        return element as T;
    }

    public static IReadOnlyList<IElement> GetAllElementsByTestId(this IElement element, params string[] testIds) =>
        //element.QuerySelectorAll(string.Join(',', testIds.Select(testId => $"*[data-testid='{testId}']"))).ToList();
        testIds.SelectMany(testId => element.QuerySelectorAll($"*[data-testid='{testId}']")).ToList();

    public static IElement? GetElementByTestId(this IElement element, string testId) =>
        element.GetAllElementsByTestId(testId).SingleOrDefault();

    public static IReadOnlyList<IElement> GetAllElementsByTestId(this IHtmlDocument doc, params string[] testIds) =>
        doc.Body!.GetAllElementsByTestId(testIds);

    public static IElement? GetElementByLabel(this IHtmlDocument doc, string label)
    {
        var allLabels = doc.QuerySelectorAll("label");

        foreach (var l in allLabels)
        {
            if (l.TrimmedText() == label)
            {
                var @for = l.GetAttribute("for");
                return @for is not null ? doc.GetElementById(@for) : null;
            }
        }

        return null;
    }

    public static IElement? GetElementByTestId(this IHtmlDocument doc, string testId) =>
        doc.Body!.GetElementByTestId(testId);

    public static IReadOnlyList<IElement> GetSummaryListActionsForKey(this IHtmlDocument doc, string key)
    {
        var row = doc.GetSummaryListRowForKey(key);
        return row?.QuerySelectorAll(".govuk-summary-list__actions>*").ToArray() ?? Array.Empty<IElement>();
    }

    public static int GetSummaryListRowCountForKey(this IHtmlDocument doc, string key)
    {
        var count = 0;
        var allRows = doc.QuerySelectorAll(".govuk-summary-list__row");

        foreach (var row in allRows)
        {
            var rowKey = row.QuerySelector(".govuk-summary-list__key");

            if (rowKey?.TrimmedText() == key)
            {
                count++;
            }
        }

        return count;
    }

    public static IElement? GetSummaryListRowForKey(this IDocument doc, string key) =>
        doc.Body?.GetSummaryListRowForKey(key);

    public static IElement? GetSummaryListRowForKey(this IElement element, string key)
    {
        var allRows = element.QuerySelectorAll(".govuk-summary-list__row");

        foreach (var row in allRows)
        {
            var rowKey = row.QuerySelector(".govuk-summary-list__key");

            if (rowKey?.TrimmedText() == key)
            {
                return row;
            }
        }

        return null;
    }

    public static string? GetSummaryListValueForKey(this IDocument doc, string key) =>
        doc.Body?.GetSummaryListValueForKey(key);

    public static string? GetSummaryListValueForKey(this IElement element, string key) =>
        GetSummaryListValueElementForKey(element, key)?.TrimmedText();

    public static IElement? GetSummaryListValueElementForKey(this IDocument doc, string key) =>
        doc.Body?.GetSummaryListValueElementForKey(key);

    public static IElement? GetSummaryListValueElementForKey(this IElement element, string key)
    {
        var row = element.GetSummaryListRowForKey(key);
        var rowValue = row?.QuerySelector(".govuk-summary-list__value");
        return rowValue;
    }

    public static string TrimmedText(this INode node) => node.Text().Trim();

    public static T GetChildElementOfTestId<T>(this IHtmlDocument doc, string testId, string childSelector) where T : IElement
    {
        var parent = doc.GetElementByTestId(testId);
        Assert.NotNull(parent);
        var child = parent.QuerySelector(childSelector);
        Assert.NotNull(child);
        Assert.IsAssignableFrom<T>(child);
        return (T)child;
    }

    public static IEnumerable<T> GetChildElementsOfTestId<T>(this IHtmlDocument doc, string testId, string childSelector) where T : IElement
    {
        var parent = doc.GetElementByTestId(testId);
        Assert.NotNull(parent);
        var children = parent.QuerySelectorAll(childSelector);
        Assert.All(children, c => Assert.IsAssignableFrom<T>(c));
        return children.Cast<T>();
    }

    public static string GetHiddenInputValue(this IHtmlDocument doc, string name)
    {
        var element = doc.QuerySelector($@"input[type=""hidden""][name=""{name}""]");
        var input = Assert.IsAssignableFrom<IHtmlInputElement>(element);

        return input.Value;
    }
}

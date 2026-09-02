using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Xunit;

namespace TeachingRecordSystem.UiTestCommon;

public static class AngleSharpExtensions
{
    extension(IElement element)
    {
        public T? As<T>()
            where T : class, IElement
        {
            return element as T;
        }

        public IReadOnlyList<IElement> GetAllElementsByTestId(params string[] testIds) =>
            testIds.SelectMany(testId => element.QuerySelectorAll($"*[data-testid='{testId}']")).ToList();
    }

    extension(IHtmlDocument doc)
    {
        public IReadOnlyList<IElement> GetAllElementsByTestId(params string[] testIds) =>
            doc.Body!.GetAllElementsByTestId(testIds);

        public IElement? GetElementByLabel(string label)
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
    }

    public static IElement? GetElementByDataAttribute(this IElement element, string attributeName, string attributeValue) =>
        element.QuerySelector($"[{attributeName}='{attributeValue}']");

    public static IElement? GetElementByDataAttribute(this IHtmlDocument doc, string attributeName, string attributeValue) =>
        doc.Body!.GetElementByDataAttribute(attributeName, attributeValue);

    public static IElement? GetElementByTestId(this IElement element, string testId) =>
        element.GetAllElementsByTestId(testId).SingleOrDefault();

    extension(IHtmlDocument doc)
    {
        public IElement? GetElementByTestId(string testId) =>
            doc.Body!.GetElementByTestId(testId);

        public IReadOnlyList<IElement> GetSummaryListActionsByKey(string key)
        {
            var row = doc.GetSummaryListRowByKey(key);
            return row?.QuerySelectorAll(".govuk-summary-list__actions>*").ToArray() ?? [];
        }

        public int GetSummaryListRowCountByKey(string key)
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
    }

    public static IElement? GetSummaryListRowByKey(this IDocument doc, string key) =>
        doc.Body?.GetSummaryListRowByKey(key);

    public static IElement? GetSummaryListRowByKey(this IElement element, string key)
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

    public static string? GetSummaryListValueByKey(this IDocument doc, string key) =>
        doc.Body?.GetSummaryListValueByKey(key);

    public static string? GetSummaryListValueByKey(this IElement element, string key) =>
        GetSummaryListValueElementByKey(element, key)?.TrimmedText();

    public static IElement? GetSummaryListValueElementByKey(this IDocument doc, string key) =>
        doc.Body?.GetSummaryListValueElementByKey(key);

    public static IElement? GetSummaryListValueElementByKey(this IElement element, string key)
    {
        var row = element.GetSummaryListRowByKey(key);
        var rowValue = row?.QuerySelector(".govuk-summary-list__value");
        return rowValue;
    }

    public static string TrimmedText(this INode node) => node.Text().Trim();

    extension(IHtmlDocument doc)
    {
        public T GetChildElementOfTestId<T>(string testId, string childSelector) where T : IElement
        {
            var parent = doc.GetElementByTestId(testId);
            Assert.NotNull(parent);
            var child = parent.QuerySelector(childSelector);
            Assert.NotNull(child);
            Assert.IsAssignableFrom<T>(child);
            return (T)child;
        }

        public IEnumerable<T> GetChildElementsOfTestId<T>(string testId, string childSelector) where T : IElement
        {
            var parent = doc.GetElementByTestId(testId);
            Assert.NotNull(parent);
            var children = parent.QuerySelectorAll(childSelector);
            Assert.All(children, c => Assert.IsAssignableFrom<T>(c));
            return children.Cast<T>();
        }

        public string GetHiddenInputValue(string name)
        {
            var element = doc.QuerySelector($@"input[type=""hidden""][name=""{name}""]");
            var input = Assert.IsAssignableFrom<IHtmlInputElement>(element);

            return input.Value;
        }
    }
}

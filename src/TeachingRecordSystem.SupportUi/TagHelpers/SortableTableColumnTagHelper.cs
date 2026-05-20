using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("th", Attributes = "sort-direction")]
public class SortableTableColumnTagHelper(SupportUiSortableTableContext tableContext) : TagHelper
{
    // Text to be added to table caption for screen readers when sorting headers are present.
    // Screen reader behaviour for sortable column headers is modelled on this example in the
    // ARIA Authoring Practices Guide (APG):
    // https://www.w3.org/WAI/ARIA/apg/patterns/table/examples/sortable-table/
    public const string ScreenReaderCaptionText = " (column headers with buttons are sortable)";

    [HtmlAttributeName("sort-direction")]
    public SortDirection? Direction { get; set; }

    [HtmlAttributeName("link-template")]
    public Func<SortDirection, string>? LinkTemplate { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        tableContext.HasSortableColumn = true;

        var content = await output.GetChildContentAsync();

        var ariaSort = Direction switch
        {
            null => "none",
            SortDirection.Ascending => "ascending",
            SortDirection.Descending => "descending",
            _ => throw new InvalidOperationException("Direction is not valid.")
        };

        var directionOnClick = Direction is SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
        var linkTemplate = LinkTemplate!(directionOnClick);

        var form = new TagBuilder("form");
        form.Attributes.Add("method", "get");

        string formAction;
        Dictionary<string, StringValues> queryString;

        if (linkTemplate.IndexOf('?') is var qsp && qsp >= 0)
        {
            formAction = linkTemplate[..qsp];
            queryString = QueryHelpers.ParseQuery(linkTemplate[qsp..]);
        }
        else
        {
            formAction = linkTemplate;
            queryString = new Dictionary<string, StringValues>();
        }

        // As we're submitting a GET form, we need all the query params to be inputs
        foreach (var kvp in queryString)
        {
            var input = new TagBuilder("input");
            input.Attributes.Add("type", "hidden");
            input.Attributes.Add("name", kvp.Key);
            input.Attributes.Add("value", kvp.Value);

            form.InnerHtml.AppendHtml(input);
        }

        form.Attributes.Add("action", formAction);

        var button = new TagBuilder("button");
        button.Attributes.Add("type", "submit");
        button.InnerHtml.AppendHtml(content);

        // Contains sort direction indicators ▼ and ▲
        button.InnerHtml.AppendHtml("<span aria-hidden=\"true\"></span>");

        form.InnerHtml.AppendHtml(button);

        output.Attributes.Add("aria-sort", ariaSort);

        output.Content.SetHtmlContent(form);
    }
}

// If a table contains sorting headers we need to add text visible only to screen readers to the
// table caption. However, we don't the table contains sorting headers until we've rendered all
// the child elements of the table, and there may already be a caption defined on the table.
// So we need to hijack any existing <caption> element, suppressing its output and saving its
// content within the SupportUiSortableTableContext so the <table> element can render it instead.
[HtmlTargetElement("caption")]
public class SortableTableCaptionTagHelper(SupportUiSortableTableContext tableContext) : TagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Make sure we're actually in a table and not another type of element with a caption.
        if (tableContext.IsWithinTableContext)
        {
            tableContext.HasCaption = true;

            var content = await output.GetChildContentAsync();
            tableContext.CaptionAttributes = context.AllAttributes.ToDictionary(a => a.Name, a => a.Value.ToString());
            tableContext.CaptionContent = new HtmlString(content.GetContent());
            output.SuppressOutput();
        }
    }
}

// This tag helper soelely exists to add text to the table caption for screen readers
[HtmlTargetElement("table")]
public class SortableTableTagHelper(SupportUiSortableTableContext tableContext) : TagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Start a new table context scope
        tableContext.IsWithinTableContext = true;

        // Render child elements and see if they update the context
        var content = await output.GetChildContentAsync();

        if (tableContext.HasCaption || tableContext.HasSortableColumn)
        {
            var caption = new TagBuilder("caption");

            // If there was already a caption defined, now we render its content/attributes
            if (tableContext.HasCaption)
            {
                if (tableContext.CaptionAttributes is Dictionary<string, string?> captionAttributes)
                {
                    foreach (var attribute in captionAttributes)
                    {
                        caption.Attributes.Add(attribute.Key, attribute.Value);
                    }
                }

                if (tableContext.CaptionContent is HtmlString captionContent)
                {
                    caption.InnerHtml.AppendHtml(captionContent);
                }
            }

            // If the table has sortable columns, add the screen reader text to the caption
            if (tableContext.HasSortableColumn)
            {
                if (tableContext.HasCaption)
                {
                    // Append to already existing caption
                    caption.InnerHtml.AppendHtml($"<span class=\"govuk-visually-hidden\">{SortableTableColumnTagHelper.ScreenReaderCaptionText}</span>");
                }
                else
                {
                    // If there wasn't already a caption defined, we can make the whole caption
                    // visible only to screen readers
                    caption.Attributes.Add("class", "govuk-visually-hidden");
                    caption.InnerHtml.Append(SortableTableColumnTagHelper.ScreenReaderCaptionText);
                }
            }

            output.Content.AppendHtml(caption);
        }

        output.Content.AppendHtml(content);

        // Reset context for the next table in the page
        tableContext.IsWithinTableContext = false;
        tableContext.HasSortableColumn = false;
        tableContext.HasCaption = false;
        tableContext.CaptionAttributes = null;
        tableContext.CaptionContent = null;
    }
}

// Context class for SortableTable tag helpers to communicate with each other.
// Unfortunately TagHelperContext.Items cannot be used for this purpose as any
// partial view or view component within the hierarchy interrupts the TagHelperContext
// and causes a new context to be created, making it unusable for this purpose
// This must be injected in request scope using .AddScoped()
public class SupportUiSortableTableContext
{
    public bool IsWithinTableContext { get; set; }
    public bool HasSortableColumn { get; set; }
    public bool HasCaption { get; set; }
    public Dictionary<string, string?>? CaptionAttributes { get; set; }
    public HtmlString? CaptionContent { get; set; }
}

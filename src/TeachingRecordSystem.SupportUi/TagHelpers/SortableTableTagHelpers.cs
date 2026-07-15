using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

// These tag helpers reproduce, server-side, the markup that the MOJ Frontend "sortable-table"
// JavaScript component would otherwise build on the client:
// https://github.com/ministryofjustice/moj-frontend/blob/main/src/moj/components/sortable-table/sortable-table.mjs
//
// The MOJ component sorts table rows in the browser. We instead sort server-side, so rather than
// enhancing the table with JavaScript (which would require a `data-module="moj-sortable-table"`
// attribute) each sortable column header renders a GET <form> whose submit button reloads the page
// with the requested sort applied. Everything else - the <button> inside the header, the SVG sort
// indicators, the visually-hidden caption text and the live-region status box - is rendered to match
// the enhanced component so the shared MOJ CSS (`[aria-sort] button`) and screen reader experience
// still apply.

// Applied to a sortable column header. Wraps the header content in a GET form + submit button (so
// sorting happens on the server) and renders the aria-sort state and SVG direction indicator that
// the MOJ component would add on the client.
[HtmlTargetElement("th", Attributes = "sort-direction")]
public class SortableTableColumnTagHelper : TagHelper
{
    // SVG direction indicators, copied verbatim from the MOJ Frontend sortable-table component so the
    // rendered arrows are identical to the JavaScript-enhanced version. `fill="currentColor"` means
    // they inherit the button's colour (including the focus state defined by the MOJ CSS).
    private const string UpArrowSvg =
        """<svg width="22" height="22" focusable="false" aria-hidden="true" role="img" viewBox="0 0 22 22" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M6.5625 15.5L11 6.63125L15.4375 15.5H6.5625Z" fill="currentColor"/></svg>""";
    private const string DownArrowSvg =
        """<svg width="22" height="22" focusable="false" aria-hidden="true" role="img" viewBox="0 0 22 22" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M15.4375 7L11 15.8687L6.5625 7L15.4375 7Z" fill="currentColor"/></svg>""";
    private const string UpDownArrowSvg =
        """<svg width="22" height="22" focusable="false" aria-hidden="true" role="img" viewBox="0 0 22 22" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M8.1875 9.5L10.9609 3.95703L13.7344 9.5H8.1875Z" fill="currentColor"/><path d="M13.7344 12.0781L10.9609 17.6211L8.1875 12.0781H13.7344Z" fill="currentColor"/></svg>""";

    [HtmlAttributeName("sort-direction")]
    public SortDirection? Direction { get; set; }

    [HtmlAttributeName("link-template")]
    public Func<SortDirection, string>? LinkTemplate { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        // The MOJ component uses aria-sort to convey the sort state and picks the matching arrow:
        // ascending -> up, descending -> down, unsorted -> up/down.
        var (ariaSort, directionIndicator) = Direction switch
        {
            null => ("none", UpDownArrowSvg),
            SortDirection.Ascending => ("ascending", UpArrowSvg),
            SortDirection.Descending => ("descending", DownArrowSvg),
            _ => throw new InvalidOperationException("Direction is not valid.")
        };

        // Toggle to the opposite direction when already sorted ascending, otherwise sort ascending -
        // this mirrors the toggle behaviour of the MOJ component's onSortButtonClick.
        var directionOnClick = Direction is SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
        var link = LinkTemplate!(directionOnClick);

        var form = new TagBuilder("form");
        form.Attributes.Add("method", "get");

        string formAction;
        Dictionary<string, StringValues> queryString;

        if (link.IndexOf('?') is var qsp and >= 0)
        {
            formAction = link[..qsp];
            queryString = QueryHelpers.ParseQuery(link[qsp..]);
        }
        else
        {
            formAction = link;
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
        button.InnerHtml.AppendHtml(directionIndicator);

        form.InnerHtml.AppendHtml(button);

        output.Attributes.SetAttribute("aria-sort", ariaSort);

        output.Content.SetHtmlContent(form);
    }
}

// Assigned to a <table> to opt it in to sortable-table behaviour. Adds the caption text and the
// visually-hidden live-region status box that the MOJ Frontend sortable-table JavaScript would add
// on the client. Individual sortable columns are declared with SortableTableColumnTagHelper.
[HtmlTargetElement("table", Attributes = SortableAttributeName)]
public class SortableTableTagHelper : TagHelper
{
    private const string SortableAttributeName = "sortable";

    // Text added to the table caption for screen readers. Screen reader behaviour for sortable
    // column headers is modelled on this example in the ARIA Authoring Practices Guide (APG) and
    // matches the text used by the MOJ Frontend component:
    // https://www.w3.org/WAI/ARIA/apg/patterns/table/examples/sortable-table/
    private const string ScreenReaderCaptionText = " (column headers with buttons are sortable).";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Remove the marker attribute so it doesn't leak into the rendered HTML.
        output.Attributes.RemoveAll(SortableAttributeName);

        // Add a visually-hidden caption explaining to screen readers that the column headers are
        // sortable. Our sortable tables don't declare their own <caption>, so we always create one;
        // it must be the table's first child.
        var caption = new TagBuilder("caption");
        caption.Attributes.Add("class", "govuk-visually-hidden");
        caption.InnerHtml.Append(ScreenReaderCaptionText);
        output.PreContent.AppendHtml(caption);

        // The MOJ component inserts a visually-hidden live region immediately after the table to
        // announce sort changes to screen readers. We render the same element (as a sibling of the
        // table) to keep the DOM structure aligned with the enhanced component.
        var status = new TagBuilder("div");
        status.Attributes.Add("aria-atomic", "true");
        status.Attributes.Add("aria-live", "polite");
        status.Attributes.Add("class", "govuk-visually-hidden");
        status.Attributes.Add("role", "status");
        output.PostElement.AppendHtml(status);
    }
}

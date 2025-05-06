using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("th", Attributes = "sort-direction")]
public class SortableTableColumnTagHelper : TagHelper
{
    [HtmlAttributeName("sort-direction")]
    public SortDirection? Direction { get; set; }

    [HtmlAttributeName("link-template")]
    public Func<SortDirection, string>? LinkTemplate { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
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

        form.InnerHtml.AppendHtml(button);

        output.Attributes.Add("aria-sort", ariaSort);

        output.Content.SetHtmlContent(form);
    }
}

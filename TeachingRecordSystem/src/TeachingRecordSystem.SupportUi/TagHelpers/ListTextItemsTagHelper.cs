using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

public class ListTextItemsTagHelper : TagHelper
{
    public IReadOnlyCollection<string>? TextItems { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (TextItems?.Count > 0)
        {
            output.TagName = "ul";
            output.AddClass("govuk-list", HtmlEncoder.Default);
            output.AddClass("govuk-!-margin-bottom-0", HtmlEncoder.Default);

            foreach (var item in TextItems!)
            {
                output.Content.AppendHtml("<li>");
                output.Content.Append(item);
                output.Content.AppendHtml("</li>");
            }
            output.TagMode = TagMode.StartTagAndEndTag;
        }
        else
        {
            output.SuppressOutput();
        }
    }
}

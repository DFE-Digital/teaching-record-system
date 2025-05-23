using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

public class ListTextItemsTagHelper : TagHelper
{
    public string[]? TextItems { get; set; } = [];

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (TextItems != null && TextItems.Any())
        {
            output.TagName = "ul";
            output.AddClass("govuk-list", HtmlEncoder.Default);

            foreach (var item in TextItems!)
            {
                output.Content.AppendHtml("<li>");
                output.Content.Append(item);
                output.Content.AppendHtml("</li>");
            }
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("highlight")]
[HtmlTargetElement("*", Attributes = "highlight")]
public class HighlightTagHelper : TagHelper
{
    [HtmlAttributeName("highlight")]
    public bool Highlight { get; set; } = true;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (Highlight)
        {
            var content = await output.GetChildContentAsync();

            if (output.IsContentModified)
            {
                content = output.Content;
            }

            var mark = new TagBuilder("mark");
            mark.AddCssClass("hods-highlight");

            var strong = new TagBuilder("strong");
            strong.InnerHtml.Append(content.ToHtmlString(HtmlEncoder.Default));
            mark.InnerHtml.AppendHtml(strong);

            output.Content.SetHtmlContent(mark);
        }
    }
}


using Microsoft.AspNetCore.Html;
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
            var content = (await output.GetChildContentAsync()).GetContent();

            if (output.IsContentModified)
            {
                content = output.Content.GetContent();
            }

            var mark = new TagBuilder("mark");
            mark.AddCssClass("hods-highlight");

            var strong = new TagBuilder("strong");
            strong.InnerHtml.SetHtmlContent(content);
            mark.InnerHtml.SetHtmlContent(strong);

            output.Content.SetHtmlContent(mark);
        }
    }
}


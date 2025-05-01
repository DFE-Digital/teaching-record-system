using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("highlight")]
public class HighlightTagHelper : TagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        output.TagMode = TagMode.StartTagAndEndTag;
        output.TagName = "mark";
        output.AddClass("hods-highlight", HtmlEncoder.Default);

        var wrapper = new TagBuilder("strong");
        wrapper.InnerHtml.AppendHtml(content);

        output.Content.SetHtmlContent(wrapper);
    }
}


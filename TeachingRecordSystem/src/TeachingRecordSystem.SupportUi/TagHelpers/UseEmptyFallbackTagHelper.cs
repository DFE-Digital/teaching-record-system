using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("*", Attributes = "use-empty-fallback")]
public class UseEmptyFallbackTagHelper : TagHelper
{
    public override int Order => -1;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();
        output.Attributes.RemoveAll("use-empty-fallback");

        if (content.IsEmptyOrWhiteSpace)
        {
            output.AddClass("trs-subtle-emphasis", HtmlEncoder.Default);
            output.Content.SetContent(UiDefaults.EmptyDisplayContent);
        }
    }
}

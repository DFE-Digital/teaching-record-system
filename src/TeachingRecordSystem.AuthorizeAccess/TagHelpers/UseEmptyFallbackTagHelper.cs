using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.AuthorizeAccess.TagHelpers;

[HtmlTargetElement("*", Attributes = "use-empty-fallback")]
public class UseEmptyFallbackTagHelper : TagHelper
{
    public override int Order => -1;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        IHtmlContent fallbackText = new HtmlString(HtmlEncoder.Default.Encode(WebConstants.EmptyFallbackContent));

        if (output.Attributes.TryGetAttribute("use-empty-fallback", out var useEmptyFallbackAttribute) &&
            useEmptyFallbackAttribute.ValueStyle != HtmlAttributeValueStyle.Minimized)
        {
            fallbackText = useEmptyFallbackAttribute.Value switch
            {
                IHtmlContent html => html,
                var obj => new HtmlString(HtmlEncoder.Default.Encode(obj.ToString() ?? string.Empty))
            };
        }

        output.Attributes.RemoveAll("use-empty-fallback");

        if (content.IsEmptyOrWhiteSpace)
        {
            output.AddClass("trs-empty-fallback", HtmlEncoder.Default);
            output.Content.SetHtmlContent(fallbackText);
        }
    }
}

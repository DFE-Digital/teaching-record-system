using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement(Attributes = "show-if")]
public class ShowIfTagHelper : TagHelper
{
    [HtmlAttributeName("show-if")]
    public bool RenderContent { get; set; } = true;

    //public override int Order => int.MinValue;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (!RenderContent)
        {
            output.TagName = null;
            output.SuppressOutput();
        }
    }
}

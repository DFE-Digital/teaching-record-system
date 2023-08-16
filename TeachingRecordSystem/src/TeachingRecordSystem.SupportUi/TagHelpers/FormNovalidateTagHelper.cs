using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("form")]
public class FormNovalidateTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.Add("novalidate", string.Empty);
    }
}

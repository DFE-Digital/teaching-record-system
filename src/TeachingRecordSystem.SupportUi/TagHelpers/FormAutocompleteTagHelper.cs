using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("form")]
public class FormAutocompleteTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!output.Attributes.ContainsName("autocomplete"))
        {
            output.Attributes.Add("autocomplete", "off");
        }
    }
}

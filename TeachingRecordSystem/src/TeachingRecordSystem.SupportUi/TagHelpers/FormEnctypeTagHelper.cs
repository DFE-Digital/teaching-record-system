using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("form")]
public class FormEnctypeTagHelper(SupportUiFormContext formContext) : TagHelper
{
    public override int Order => -899;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await output.GetChildContentAsync();

        if (formContext.HasFileUpload && !output.Attributes.ContainsName("enctype"))
        {
            output.Attributes.Add("enctype", "multipart/form-data");
        }

        // Reset for the next form
        formContext.HasFileUpload = false;
    }
}

[HtmlTargetElement("govuk-file-upload")]
public class FileUploadTagHelper(SupportUiFormContext formContext) : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        formContext.HasFileUpload = true;
    }
}

public class SupportUiFormContext
{
    public bool HasFileUpload { get; set; }
}

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

        // Reset context for the next form in the page
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

// Context class for form tag helpers to communicate with each other.
// Unfortunately TagHelperContext.Items cannot be used for this purpose as any
// partial view or view component within the hierarchy interrupts the TagHelperContext
// and causes a new context to be created, making it unusable for this purpose
// This must be injected in request scope using .AddScoped()
public class SupportUiFormContext
{
    public bool HasFileUpload { get; set; }
}

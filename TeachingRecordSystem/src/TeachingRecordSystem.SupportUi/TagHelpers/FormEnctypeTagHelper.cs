using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("form")]
public class FormEnctypeTagHelper : TagHelper
{
    public override int Order => -899;

    public override void Init(TagHelperContext context)
    {
        context.Items.Add(typeof(SupportUiFormContext), new SupportUiFormContext());
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var supportUiFormContext = (SupportUiFormContext)context.Items[typeof(SupportUiFormContext)];

        await output.GetChildContentAsync();

        if (output.Attributes.ContainsName("enctype") || !supportUiFormContext.HasFileUpload)
        {
            return;
        }

        output.Attributes.Add("enctype", "multipart/form-data");
    }
}

[HtmlTargetElement("govuk-file-upload")]
public class FileUploadTagHelper : TagHelper
{
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var supportUiFormContext = (SupportUiFormContext)context.Items[typeof(SupportUiFormContext)];
        supportUiFormContext.HasFileUpload = true;
    }
}

public class SupportUiFormContext
{
    public bool HasFileUpload { get; set; }
}

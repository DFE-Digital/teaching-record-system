using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

public class CaptureContentTagHelper : TagHelper
{
    public IHtmlContentBuilder? To { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (To is null)
        {
            throw new InvalidOperationException("To must be specified.");
        }

        var content = await output.GetChildContentAsync();

        if (!content.IsEmptyOrWhiteSpace)
        {
            To.AppendHtml(content);
        }

        output.TagName = null;
    }
}

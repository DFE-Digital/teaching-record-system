using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

public class MultiLineTextTagHelper : TagHelper
{
    public string Text { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.AddClass("govuk-body", HtmlEncoder.Default);

        if (!string.IsNullOrWhiteSpace(Text))
        {
            var lines = Text.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
            for (var i = 0; i < lines.Length; i++)
            {
                output.Content.Append(lines[i]);
                if (i < lines.Length - 1)
                {
                    output.Content.AppendHtml("<br />");
                }
            }
        }

        output.TagMode = TagMode.StartTagAndEndTag;
    }
}

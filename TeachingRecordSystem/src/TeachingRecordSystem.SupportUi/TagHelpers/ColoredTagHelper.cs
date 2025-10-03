using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("colored-tag")]
[OutputElementHint("strong")]
public class ColoredTagHelper : TagHelper
{
    private static readonly string[] _tagClasses =
    [
        "govuk-tag--green",
        "govuk-tag--turquoise",
        "govuk-tag--blue",
        "govuk-tag--light-blue",
        "govuk-tag--purple",
        "govuk-tag--pink",
        "govuk-tag--red",
        "govuk-tag--orange",
        "govuk-tag--yellow"
    ];

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        var colorClass = _tagClasses[
            Math.Abs(
                BitConverter.ToInt64(SHA3_256.HashData(Encoding.UTF8.GetBytes(content.ToHtmlString(HtmlEncoder.Default))).AsSpan()[..^8])
                % _tagClasses.Length)];

        output.TagMode = TagMode.StartTagAndEndTag;
        output.TagName = "strong";
        output.AddClass("govuk-tag", HtmlEncoder.Default);
        output.AddClass(colorClass, HtmlEncoder.Default);
    }
}


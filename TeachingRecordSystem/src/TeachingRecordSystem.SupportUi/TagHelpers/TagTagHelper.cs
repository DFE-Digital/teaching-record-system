using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("tag")]
[OutputElementHint("strong")]
public class TagTagHelper : TagHelper
{
    private static readonly TagColor[] NeutralColors = [TagColor.Purple, TagColor.Turquoise, TagColor.Blue, TagColor.LightBlue];

    [HtmlAttributeName("color")]
    public TagColor? Color { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        var color = Color switch
        {
            TagColor.Auto => GenerateColorFromContent(content),
            TagColor c => c,
            _ => TagColor.Blue
        };

        output.TagMode = TagMode.StartTagAndEndTag;
        output.TagName = "strong";
        output.AddClass("govuk-tag", HtmlEncoder.Default);
        output.AddClass(color.GetCssClass(), HtmlEncoder.Default);
    }

    private TagColor GenerateColorFromContent(TagHelperContent content) => NeutralColors[
        Math.Abs(
            BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes(content.ToHtmlString(HtmlEncoder.Default))).AsSpan()[..^8])
            % NeutralColors.Length)];
}

public enum TagColor
{
    Auto,
    Blue,
    Green,
    Grey,
    LightBlue,
    Orange,
    Pink,
    Purple,
    Red,
    Turquoise,
    Yellow,
}

public static class TagColorExtensions
{
    public static string GetCssClass(this TagColor tagColor) => tagColor switch
    {
        TagColor.Green => "govuk-tag--green",
        TagColor.Grey => "govuk-tag--grey",
        TagColor.LightBlue => "govuk-tag--light-blue",
        TagColor.Orange => "govuk-tag--orange",
        TagColor.Pink => "govuk-tag--pink",
        TagColor.Purple => "govuk-tag--purple",
        TagColor.Red => "govuk-tag--red",
        TagColor.Turquoise => "govuk-tag--turquoise",
        TagColor.Yellow => "govuk-tag--yellow",
        _ => "govuk-tag--blue",
    };
}

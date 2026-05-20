using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TeachingRecordSystem.SupportUi;

public static class HtmlHelperExtensions
{
    public static IHtmlContent ConvertNewlinesToLineBreaks(this IHtmlHelper htmlHelper, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return HtmlString.Empty;
        }

        var htmlContent = HtmlEncoder.Default.Encode(text).Replace("&#xD;&#xA;", "<br>");
        return new HtmlString(htmlContent);
    }
}

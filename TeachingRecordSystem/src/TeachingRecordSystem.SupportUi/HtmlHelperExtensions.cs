using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TeachingRecordSystem.SupportUi;

public static partial class HtmlHelperExtensions
{
    public static HtmlString ShyEmail(this IHtmlHelper htmlHelper, string email)
    {
        var escaped = new HtmlString(email).Value!;
        return new HtmlString(string.Join("&shy;", ShyEmailSplitPattern().Matches(escaped)));
    }

    [GeneratedRegex("\\W?\\w+")]
    private static partial Regex ShyEmailSplitPattern();
}

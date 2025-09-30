using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TeachingRecordSystem.SupportUi;

public static class LinkTagBuilder
{
    public static Action<IHtmlContentBuilder> BuildLink(string link)
    {
        return htmlBuilder =>
            {
                var linkTag = new TagBuilder("a");
                linkTag.AddCssClass("govuk-link");
                linkTag.MergeAttribute("href", link);
                linkTag.MergeAttribute("target", "_blank");
                linkTag.MergeAttribute("rel", "noopener noreferrer");
                linkTag.InnerHtml.Append("View record (opens in a new tab)");
                htmlBuilder.AppendHtml(linkTag);
            };
    }
}

using Microsoft.AspNetCore.Razor.TagHelpers;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

// Turns a GovUk.Frontend navigation link into one that swaps part of the page with htmx instead of
// navigating to it. The URL that gets requested - including anything hx-include adds to it (see
// HtmxLinkOptions.Include) - is pushed into history, so going back to it restores the page as it was.
//
// The link keeps its href, so without JavaScript it's still an ordinary link. Passing null options
// leaves it as one.
[HtmlTargetElement(PaginationItemTagName, Attributes = HtmxAttributeName)]
[HtmlTargetElement(PaginationPreviousTagName, Attributes = HtmxAttributeName)]
[HtmlTargetElement(PaginationNextTagName, Attributes = HtmxAttributeName)]
[HtmlTargetElement("govuk-back-link", Attributes = HtmxAttributeName)]
public class HtmxLinkTagHelper : TagHelper
{
    private const string PaginationItemTagName = "govuk-pagination-item";
    private const string PaginationPreviousTagName = "govuk-pagination-previous";
    private const string PaginationNextTagName = "govuk-pagination-next";
    private const string HtmxAttributeName = "htmx";

    // Run before the GovUk.Frontend tag helper for the element; it copies the attributes we add here
    // onto the markup it renders.
    public override int Order => int.MinValue;

    [HtmlAttributeName(HtmxAttributeName)]
    public HtmxLinkOptions? Htmx { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Htmx is null || !context.AllAttributes.TryGetAttribute("href", out var href))
        {
            return;
        }

        if (href.Value?.ToString() is not string link)
        {
            return;
        }

        // The pagination previous and next links render their anchor inside a container element, and
        // it's the container that these attributes end up on. Boost the anchor within it rather than
        // having the container issue the request itself - otherwise the browser would follow the link
        // as well as htmx requesting it.
        if (output.TagName is PaginationPreviousTagName or PaginationNextTagName)
        {
            output.Attributes.SetAttribute("hx-boost", "true");
        }
        else
        {
            output.Attributes.SetAttribute("hx-get", link);
        }

        output.Attributes.SetAttribute("hx-push-url", "true");
        output.Attributes.SetAttribute("hx-select", Htmx.Select);
        output.Attributes.SetAttribute("hx-target", Htmx.Target);
        output.Attributes.SetAttribute("hx-swap", Htmx.Swap);

        if (Htmx.Include is string include)
        {
            output.Attributes.SetAttribute("hx-include", include);
        }
    }
}

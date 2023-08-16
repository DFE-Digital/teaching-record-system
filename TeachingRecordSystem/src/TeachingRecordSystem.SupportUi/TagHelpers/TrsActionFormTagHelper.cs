using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("form", Attributes = "trs-action")]
public class TrsActionFormTagHelper : TagHelper
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly TrsLinkGenerator _linkGenerator;

    public TrsActionFormTagHelper(IHtmlGenerator htmlGenerator, TrsLinkGenerator linkGenerator)
    {
        _htmlGenerator = htmlGenerator;
        _linkGenerator = linkGenerator;
    }

    [HtmlAttributeName("trs-action")]
    [DisallowNull]
    public Func<TrsLinkGenerator, string>? Action { get; set; }

    [HtmlAttributeName("method")]
    public string? Method { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    [DisallowNull]
    public ViewContext? ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Action is null)
        {
            throw new InvalidOperationException("trs-action must be specified.");
        }

        var action = Action(_linkGenerator);
        output.Attributes.Add("action", action);

        var method = Method;
        if (method is null)
        {
            method = "post";
            output.Attributes.Add("method", method);
        }

        if (!string.Equals(method, "get", StringComparison.OrdinalIgnoreCase))
        {
            var antiforgeryTag = _htmlGenerator.GenerateAntiforgery(ViewContext);
            output.PostContent.AppendHtml(antiforgeryTag);
        }
    }
}

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("filter-layout")]
[RestrictChildren("filter-layout-filter", "filter-layout-content")]
[OutputElementHint("div")]
public class FilterLayoutTagHelper : TagHelper
{
    [HtmlAttributeName("form-action")]
    public string? FormAction { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var formTag = new TagBuilder("form");
        formTag.Attributes.Add("method", "get");
        if (FormAction is not null)
        {
            formTag.Attributes.Add("action", FormAction);
        }
        output.PreContent.AppendHtml(formTag.RenderStartTag());

        output.TagName = "div";
        output.AddClass("moj-filter-layout", HtmlEncoder.Default);
        output.AddClass("trs-filter-layout", HtmlEncoder.Default);

        output.PostContent.AppendHtml(formTag.RenderEndTag());
    }
}

[HtmlTargetElement("filter-layout-filter", ParentTag = "filter-layout")]
[OutputElementHint("div")]
public class FilterLayoutFilterTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.AddClass("moj-filter-layout__filter", HtmlEncoder.Default);
    }
}

[HtmlTargetElement("filter", ParentTag = "filter-layout-filter")]
[RestrictChildren("filter-options")]
[OutputElementHint("div")]
public class FilterTagHelper : TagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.AddClass("moj-filter", HtmlEncoder.Default);

        output.Content.AppendHtml(
            """
            <div class="moj-filter__header">
                <div class="moj-filter__header-title">
                    <h2 class="govuk-heading-m">Filter</h2>
                </div>
            </div>
            """);

        output.Content.AppendHtml(await output.GetChildContentAsync());
    }
}

[HtmlTargetElement("filter-options", ParentTag = "filter")]
[OutputElementHint("div")]
public class FilterOptionsTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.AddClass("moj-filter__options", HtmlEncoder.Default);
    }
}

[HtmlTargetElement("filter-layout-content", ParentTag = "filter-layout")]
[OutputElementHint("div")]
public class FilterLayoutContentTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.AddClass("moj-filter-layout__content", HtmlEncoder.Default);
    }
}

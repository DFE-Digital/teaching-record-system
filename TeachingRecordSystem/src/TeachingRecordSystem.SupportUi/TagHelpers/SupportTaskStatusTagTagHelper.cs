using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

[HtmlTargetElement("support-task-status-tag", Attributes = "status", TagStructure = TagStructure.WithoutEndTag)]
public class SupportTaskStatusTagTagHelper : TagTagHelper
{
    [HtmlAttributeName("status")]
    public SupportTaskStatus Status { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var tagClass = Status switch
        {
            SupportTaskStatus.Open => "govuk-tag--yellow",
            SupportTaskStatus.InProgress => "govuk-tag--blue",
            SupportTaskStatus.Closed => "govuk-tag--green",
            _ => throw new NotSupportedException()
        };

        var tag = new TagBuilder("strong");
        tag.AddCssClass("govuk-tag");
        tag.AddCssClass(tagClass);
        tag.InnerHtml.Append(Status.GetDisplayName()!);

        output.TagName = null;
        output.Content.AppendHtml(tag);

        return Task.CompletedTask;
    }
}

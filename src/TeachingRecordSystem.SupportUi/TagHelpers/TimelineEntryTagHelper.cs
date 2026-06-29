using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TeachingRecordSystem.SupportUi.TagHelpers;

public class TimelineEntryTagHelper : TagHelper
{
    public DateTime Timestamp { get; set; }

    public string? By { get; set; }

    public string? Title { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var descriptionHtml = await output.GetChildContentAsync();

        var timeline = CreateTimelineItem(descriptionHtml);

        output.TagName = null;
        output.Content.SetHtmlContent(timeline);
    }

    private TagBuilder CreateTimelineItem(IHtmlContent descriptionHtml)
    {
        var timelineItem = new TagBuilder("div");
        timelineItem.AddCssClass("moj-timeline__item");
        timelineItem.AddCssClass("govuk-!-padding-bottom-2");

        var header = new TagBuilder("div");
        header.AddCssClass("moj-timeline__header");

        var title = new TagBuilder("h2");
        title.AddCssClass("moj-timeline__title");
        title.InnerHtml.Append(Title!);
        header.InnerHtml.AppendHtml(title);

        timelineItem.InnerHtml.AppendHtml(header);

        var date = new TagBuilder("p");
        date.AddCssClass("moj-timeline__date");

        var by = new TagBuilder("span");
        by.InnerHtml.Append("By ");
        by.InnerHtml.Append(By ?? "");
        by.InnerHtml.Append(" on ");

        var time = new TagBuilder("time");
        time.MergeAttribute("timestamp", Timestamp.ToString("O"));
        time.InnerHtml.Append(Timestamp.ToString("d MMMMM yyyy 'at' h:mm tt"));
        by.InnerHtml.AppendHtml(time);

        date.InnerHtml.AppendHtml(by);

        var description = new TagBuilder("div");
        description.AddCssClass("moj-timeline__description");
        description.InnerHtml.AppendHtml(descriptionHtml);
        timelineItem.InnerHtml.AppendHtml(description);

        return timelineItem;
    }
}

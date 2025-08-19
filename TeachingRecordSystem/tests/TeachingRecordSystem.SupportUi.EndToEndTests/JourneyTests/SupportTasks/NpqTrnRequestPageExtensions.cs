using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public static class NpqTrnRequestPageExtensions
{
    public static Task AssertOnListPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests");
    }

    public static Task AssertOnDetailsPageAsync(this IPage page, string taskReference)
    {
        return page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests/{taskReference}/details");
    }

    public static Task AssertOnMatchesPageAsync(this IPage page, string taskReference)
    {
        return page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests/{taskReference}/matches");
    }

    public static Task AssertOnMergePageAsync(this IPage page, string taskReference)
    {
        return page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests/{taskReference}/merge");
    }

    public static Task AssertOnMatchesCheckYourAnswersPageAsync(this IPage page, string taskReference)
    {
        return page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests/{taskReference}/check-answers");
    }

    public static void AssertOnAPersonDetailPage(this IPage page)
    {
        var asUri = new Uri(page.Url);
        var parts = asUri.LocalPath.Split("/persons/", StringSplitOptions.RemoveEmptyEntries);
        var guid = parts[0];
        Assert.True(Guid.TryParse(guid, out _));
    }

    public static async Task AssertSuccessBannerAsync(this IPage page, string name)
    {
        var bannerTitle = page.Locator("h2.govuk-notification-banner__title");
        var bannerText = page.Locator("h3.govuk-notification-banner__heading");

        Assert.Equal("Success", await bannerTitle.TextContentAsync());
        Assert.Equal("NPQ request completed", await bannerText.TextContentAsync());
        var link = page.GetByRole(AriaRole.Link, new() { Name = $"Record created for {name}" });
        Assert.NotNull(link);
    }

    public static Task ClickChangeLink(this IPage page)
    {
        return page.GetByTestId("change-link").ClickAsync();
    }

    public static Task FollowBannerLink(this IPage page)
    {
        var link = page.Locator("div.govuk-notification-banner__content >> a.govuk-link");
        return link.ClickAsync();
    }
}

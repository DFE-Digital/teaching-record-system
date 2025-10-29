using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public static class NpqTrnRequestPageExtensions
{
    public static Task AssertOnListPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests");

    public static Task AssertOnDetailsPageAsync(this IPage page, string taskReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests/{taskReference}/details");

    public static Task AssertOnMatchesPageAsync(this IPage page, string taskReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests/{taskReference}/resolve/matches");

    public static Task AssertOnMergePageAsync(this IPage page, string taskReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests/{taskReference}/resolve/merge");

    public static Task AssertOnMatchesCheckYourAnswersPageAsync(this IPage page, string taskReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests/{taskReference}/resolve/check-answers");

    public static Task AssertOnNoMatchesCheckYourAnswersPageAsync(this IPage page, string taskReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests/{taskReference}/no-matches/check-answers");

    public static Task AssertOnRejectionReasonPageAsync(this IPage page, string taskReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests/{taskReference}/reject/reason");

    public static Task AssertOnRejectCheckYourAnswersPageAsync(this IPage page, string taskReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/npq-trn-requests/{taskReference}/reject/check-answers");

    public static void AssertOnAPersonDetailPage(this IPage page)
    {
        var asUri = new Uri(page.Url);
        var parts = asUri.LocalPath.Split("/persons/", StringSplitOptions.RemoveEmptyEntries);
        var guid = parts[0];
        Assert.True(Guid.TryParse(guid, out _));
    }
}

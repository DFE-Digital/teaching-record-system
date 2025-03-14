using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public static class RouteToProfessionalStatusPageExtensions
{
    public static Task AssertOnRouteEditStartDatePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/start-date");
    }

    public static Task AssertOnRouteEditEndDatePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/end-date");
    }

    public static Task AssertOnRouteEditAwardDatePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/award-date");
    }

    public static Task AssertOnRouteDetailPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/detail");
    }

    public static Task AssertOnRouteEditDegreeTypePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/degree-type");
    }

    public static Task AssertOnRouteChangeReasonPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/change-reason");
    }

    public static Task AssertOnRouteCheckYourAnswersPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/check-answers");
    }
}

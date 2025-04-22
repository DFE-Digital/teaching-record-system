using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public static class RouteToProfessionalStatusPageExtensions
{
    public static Task SelectAgeTypeAsync(this IPage page, TrainingAgeSpecialismType ageType)
    {
        var checkbox = page.Locator($"input[type='radio'][value='{ageType.ToString()}']");
        return checkbox.Locator("xpath=following-sibling::label").ClickAsync();
    }

    public static Task SelectStatusAsync(this IPage page, ProfessionalStatusStatus status)
    {
        var radioButton = page.Locator($"input[type='radio'][value='{status.ToString()}']");
        return radioButton.Locator("xpath=following-sibling::label").ClickAsync();
    }

    public static Task AssertOnRouteEditStatusPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/status");
    }

    public static Task AssertOnRouteEditStartDatePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/start-date");
    }

    public static Task AssertOnRouteEditEndDatePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/end-date");
    }

    public static Task AssertOnRouteEditAwardDatePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/award-date");
    }

    public static Task AssertOnRouteDetailPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/detail");
    }

    public static Task AssertOnRouteEditDegreeTypePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/degree-type");
    }

    public static Task AssertOnRouteEditAgeRangePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/age-range");
    }

    public static Task AssertOnRouteEditCountryPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/country");
    }

    public static Task AssertOnRouteEditTrainingProviderPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/training-provider");
    }

    public static Task AssertOnRouteEditSubjectsPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/subjects");
    }

    public static Task AssertOnRouteEditInductionExemptionPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/induction-exemption");
    }

    public static Task AssertOnRouteChangeReasonPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/change-reason");
    }

    public static Task AssertOnRouteCheckYourAnswersPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/check-answers");
    }

    public static Task AssertOnRouteAddRoutePageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/routes/add/route");
    }

    public static Task AssertOnRouteAddStatusPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/routes/add/status");
    }

    public static Task AssertOnRouteAddStartDatePageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/routes/add/start-date");
    }

    public static Task AssertOnRouteAddEndDatePageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/routes/add/end-date");
    }

    public static Task AssertOnRouteAddTrainingProviderAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/routes/add/training-provider");
    }

    public static Task AssertOnRouteAddAwardedDatePageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/routes/add/award-date");
    }

    public static Task AssertOnRouteAddInductionExemptionPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/routes/add/induction-exemption");
    }
}

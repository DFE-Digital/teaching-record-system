using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public static class RouteToProfessionalStatusPageExtensions
{
    public static Task SelectStatusAsync(this IPage page, RouteToProfessionalStatusStatus status)
    {
        var radioButton = page.Locator($"input[type='radio'][value='{status}']");
        return radioButton.Locator("xpath=following-sibling::label").ClickAsync();
    }

    public static Task SelectAgeRangeAsync(this IPage page, TrainingAgeSpecialismType ageRangeType)
    {
        var radioButton = page.Locator($"input[type='radio'][value='{ageRangeType}']");
        return radioButton.Locator("xpath=following-sibling::label").ClickAsync();
    }

    public static Task SelectRouteChangeReasonOption(this IPage page, string reason)
    {
        var radioButton = page.Locator($"input[type='radio'][value='{reason}']");
        return radioButton.Locator("xpath=following-sibling::label").ClickAsync();
    }

    public static Task EnterDegreeTypeAsync(this IPage page, string name)
    {
        return page.FillAsync("#DegreeTypeId", name);
    }

    public static Task EnterCountryAsync(this IPage page, string name)
    {
        return page.FillAsync("#TrainingCountryId", name);
    }

    public static Task EnterSubjectAsync(this IPage page, string name)
    {
        return page.FillAsync("#SubjectId1", name);
    }

    public static Task EnterTrainingProviderAsync(this IPage page, string name)
    {
        return page.FillAsync("#TrainingProviderId", name);
    }

    public static Task AssertOnRouteEditStatusPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/status");
    }

    public static Task AssertOnRouteEditStartAndEndDatePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/start-and-end-date");
    }

    public static Task AssertOnRouteEditHoldsFromPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/holds-from");
    }

    public static Task AssertOnRouteDetailPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/detail");
    }

    public static Task AssertOnRouteEditDegreeTypePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/degree-type");
    }

    public static Task AssertOnRouteEditAgeRangePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/age-range");
    }

    public static Task AssertOnRouteEditCountryPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/country");
    }

    public static Task AssertOnRouteEditTrainingProviderPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/training-provider");
    }

    public static Task AssertOnRouteEditSubjectsPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/subjects");
    }

    public static Task AssertOnRouteEditInductionExemptionPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/induction-exemption");
    }

    public static Task AssertOnRouteChangeReasonPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/change-reason");
    }

    public static Task AssertOnRouteCheckYourAnswersPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/check-answers");
    }

    public static Task AssertOnRouteAddRoutePageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/route/add/route");
    }

    public static Task AssertOnRouteAddStatusPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/route/add/status");
    }

    public static Task AssertOnRouteAddStartAndEndDatePageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/route/add/start-and-end-date");
    }

    public static Task AssertOnRouteAddTrainingProviderAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/route/add/training-provider");
    }

    public static Task AssertOnRouteAddHoldsFromPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/route/add/holds-from");
    }

    public static Task AssertOnRouteAddInductionExemptionPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/route/add/induction-exemption");
    }

    public static Task AssertOnRouteAddDegreeTypePageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/route/add/degree-type");
    }

    public static Task AssertOnRouteAddCountryAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/route/add/country");
    }

    public static Task AssertOnRouteAddAgeRangeAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/route/add/age-range");
    }

    public static Task AssertOnRouteAddSubjectsPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/route/add/subjects");
    }

    public static Task AssertOnRouteAddChangeReasonPage(this IPage page)
    {
        return page.WaitForUrlPathAsync("/route/add/change-reason");
    }

    public static Task AssertOnRouteAddCheckYourAnswersPage(this IPage page)
    {
        return page.WaitForUrlPathAsync("/route/add/check-answers");
    }

    public static Task AssertOnRouteDeleteChangeReasonPage(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/delete/change-reason");
    }

    public static Task AssertOnRouteDeleteCheckYourAnswersPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/delete/check-answers");
    }

}

using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.RoutesToProfessionalStatus;

public static class RoutesToProfessionalStatusPageExtensions
{
    extension(IPage page)
    {
        public Task SelectStatusAsync(RouteToProfessionalStatusStatus status)
        {
            var radioButton = page.Locator($"input[type='radio'][value='{status}']");
            return radioButton.Locator("xpath=following-sibling::label").ClickAsync();
        }

        public Task SelectAgeRangeAsync(TrainingAgeSpecialismType ageRangeType)
        {
            var radioButton = page.Locator($"input[type='radio'][value='{ageRangeType}']");
            return radioButton.Locator("xpath=following-sibling::label").ClickAsync();
        }

        public async Task SelectRouteChangeReasonOption(string reason, string? changeReasonDetails = null)
        {
            var radioButton = page.Locator($"input[type='radio'][value='{reason}']");
            await radioButton.Locator("xpath=following-sibling::label").ClickAsync();
            if (changeReasonDetails != null)
            {
                await page.FillAsync($"label{TestBase.TextIsSelector("Enter a reason")}", changeReasonDetails);
            }
        }

        public Task EnterDegreeTypeAsync(string name) =>
            page.FillAutocompleteAsync("DegreeTypeId", name);

        public Task EnterCountryAsync(string name) =>
            page.FillAutocompleteAsync("TrainingCountryId", name);

        public Task EnterSubjectAsync(string name) =>
            page.FillAutocompleteAsync("SubjectId1", name);

        public Task EnterTrainingProviderAsync(string name) =>
            page.FillAutocompleteAsync("TrainingProviderId", name);

        public Task AssertOnRouteEditStatusPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/status");

        public Task AssertOnRouteEditStartAndEndDatePageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/start-and-end-date");

        public Task AssertOnRouteEditHoldsFromPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/holds-from");

        public Task AssertOnRouteEditDegreeTypePageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/degree-type");

        public Task AssertOnRouteEditAgeRangePageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/age-range");

        public Task AssertOnRouteEditCountryPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/country");

        public Task AssertOnRouteEditTrainingProviderPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/training-provider");

        public Task AssertOnRouteEditSubjectsPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/subjects");

        public Task AssertOnRouteEditInductionExemptionPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/induction-exemption");

        public Task AssertOnRouteChangeReasonPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/reason");

        public Task AssertOnRouteCheckYourAnswersPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/check-answers");

        public Task AssertOnRouteAddRoutePageAsync() =>
            page.WaitForUrlPathAsync("/routes/add/route");

        public Task AssertOnRouteAddStatusPageAsync() =>
            page.WaitForUrlPathAsync("/routes/add/status");

        public Task AssertOnRouteAddStartAndEndDatePageAsync() =>
            page.WaitForUrlPathAsync("/routes/add/start-and-end-date");

        public Task AssertOnRouteAddTrainingProviderAsync() =>
            page.WaitForUrlPathAsync("/routes/add/training-provider");

        public Task AssertOnRouteAddHoldsFromPageAsync() =>
            page.WaitForUrlPathAsync("/routes/add/holds-from");

        public Task AssertOnRouteAddInductionExemptionPageAsync() =>
            page.WaitForUrlPathAsync("/routes/add/induction-exemption");

        public Task AssertOnRouteAddDegreeTypePageAsync() =>
            page.WaitForUrlPathAsync("/routes/add/degree-type");

        public Task AssertOnRouteAddCountryAsync() =>
            page.WaitForUrlPathAsync("/routes/add/country");

        public Task AssertOnRouteAddAgeRangeAsync() =>
            page.WaitForUrlPathAsync("/routes/add/age-range");

        public Task AssertOnRouteAddSubjectsPageAsync() =>
            page.WaitForUrlPathAsync("/routes/add/subjects");

        public Task AssertOnRouteAddChangeReasonPage() =>
            page.WaitForUrlPathAsync("/routes/add/reason");

        public Task AssertOnRouteAddCheckYourAnswersPage() =>
            page.WaitForUrlPathAsync("/routes/add/check-answers");

        public Task AssertOnRouteDeleteChangeReasonPage(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/delete/reason");

        public Task AssertOnRouteDeleteCheckYourAnswersPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/delete/check-answers");

        public Task AssertOnRouteDetailPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/routes/{qualificationId}/edit/detail");
    }
}

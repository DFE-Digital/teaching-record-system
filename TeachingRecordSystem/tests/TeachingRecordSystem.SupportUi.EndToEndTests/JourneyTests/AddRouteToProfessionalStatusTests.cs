using TeachingRecordSystem.SupportUi.Pages.Common;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class AddRouteToProfessionalStatusTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Route_AddJourneyToCyaPageAndBack()
    {
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(true))
            .Single(r => r.Name == "Apprenticeship");
        var status = RouteToProfessionalStatusStatus.InTraining;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddMonths(1);
        var setDegreeType = "BSc (Hons) with Intercalated PGCE";
        var setProviderName = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync(true))
            .RandomOne()
            .Name;
        var setCountry = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne()
            .Name;

        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickButtonAsync("Add a route");

        await page.AssertOnRouteAddRoutePageAsync();
        await page.FillAsync($"label:text-is('Route type')", route.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStatusPageAsync();
        await page.SelectStatusAsync(status);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStartAndEndDatePageAsync();
        await page.FillDateInputAsync("TrainingStartDate", startDate);
        await page.FillDateInputAsync("TrainingEndDate", endDate);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddTrainingProviderAsync();
        await page.EnterTrainingProviderAsync(setProviderName);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddDegreeTypePageAsync();
        await page.EnterDegreeTypeAsync(setDegreeType);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteAddCountryAsync();
        await page.EnterCountryAsync(setCountry);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddAgeRangeAsync();
        await page.SelectAgeRangeAsync(AgeSpecializationOption.FoundationStage);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddSubjectsPageAsync();
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddChangeReasonPage();
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickBackLink();

        await page.AssertOnRouteAddChangeReasonPage();
        await page.ClickBackLink();

        await page.AssertOnRouteAddSubjectsPageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddAgeRangeAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddCountryAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddDegreeTypePageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddTrainingProviderAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddStartAndEndDatePageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddStatusPageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddRoutePageAsync();
        await page.ClickBackLink();

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
    }

    [Fact]
    public async Task Route_AddJourney()
    {
        var setRoute = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(true))
            .Single(r => r.Name == "Apprenticeship");
        var status = RouteToProfessionalStatusStatus.InTraining;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddMonths(1);
        var setDegreeType = "BSc (Hons) with Intercalated PGCE";
        var setProviderName = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync(true))
            .RandomOne()
            .Name;
        var setCountry = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne()
            .Name;
        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickButtonAsync("Add a route");

        await page.AssertOnRouteAddRoutePageAsync();
        await page.FillAsync($"label:text-is('Route type')", setRoute.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStatusPageAsync();
        await page.SelectStatusAsync(status);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStartAndEndDatePageAsync();
        await page.FillDateInputAsync("TrainingStartDate", startDate);
        await page.FillDateInputAsync("TrainingEndDate", endDate);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddTrainingProviderAsync();
        await page.EnterTrainingProviderAsync(setProviderName);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddDegreeTypePageAsync();
        await page.EnterDegreeTypeAsync(setDegreeType);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteAddCountryAsync();
        await page.EnterCountryAsync(setCountry);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddAgeRangeAsync();
        await page.SelectAgeRangeAsync(AgeSpecializationOption.None);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddSubjectsPageAsync();
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddChangeReasonPage();
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickButtonAsync("Confirm and add route");

        await page.AssertOnPersonQualificationsPageAsync(personId);
    }

    [Fact]
    public async Task Route_AddJourney_OnlyCountryApplicable()
    {
        var setRoute = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(true))
            .Single(r => r.Name == "Apprenticeship");
        var status = RouteToProfessionalStatusStatus.Deferred;
        var setCountry = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne()
            .Name;
        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickButtonAsync("Add a route");

        await page.AssertOnRouteAddRoutePageAsync();
        await page.FillAsync($"label:text-is('Route type')", setRoute.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStatusPageAsync();
        await page.SelectStatusAsync(status);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddCountryAsync();
        await page.EnterCountryAsync(setCountry);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddChangeReasonPage();
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickBackLink();

        await page.AssertOnRouteAddChangeReasonPage();
        await page.ClickBackLink();

        await page.AssertOnRouteAddCountryAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddStatusPageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddRoutePageAsync();
        await page.ClickBackLink();

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
    }

    [Fact]
    public async Task Route_Add_HoldsJourney()
    {
        var setRoute = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(true))
            .Where(r => r.InductionExemptionRequired == FieldRequirement.Mandatory
                && r.InductionExemptionReason is not null
                && r.InductionExemptionReason.RouteImplicitExemption == false
                && r.TrainingProviderRequired == FieldRequirement.NotApplicable
                && r.DegreeTypeRequired == FieldRequirement.NotApplicable
                && r.TrainingCountryRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = RouteToProfessionalStatusStatus.Holds;
        var holdsFrom = new DateOnly(2021, 1, 1);
        var setCountry = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne()
            .Name;

        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickButtonAsync("Add a route");

        await page.AssertOnRouteAddRoutePageAsync();
        await page.FillAsync($"label:text-is('Route type')", setRoute.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStatusPageAsync();
        await page.SelectStatusAsync(status);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddHoldsFromPageAsync();
        await page.FillDateInputAsync(holdsFrom);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddInductionExemptionPageAsync();
        await page.SetCheckedAsync($"label:text-is('Yes')", true);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteAddCountryAsync();
        await page.EnterCountryAsync(setCountry);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddAgeRangeAsync();
        await page.SelectAgeRangeAsync(AgeSpecializationOption.None);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddSubjectsPageAsync();
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddChangeReasonPage();
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickButtonAsync("Confirm and add route");

        await page.AssertOnPersonQualificationsPageAsync(personId);
    }

    [Fact]
    public async Task Route_AddAwardedJourneyToCyaPageAndBack()
    {
        var setRoute = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(true))
            .Where(r => r.InductionExemptionRequired == FieldRequirement.Mandatory
                && r.InductionExemptionReason is not null
                && r.InductionExemptionReason.RouteImplicitExemption == false
                // && r.HoldsFromRequired != FieldRequirement.NotApplicable
                && r.TrainingProviderRequired == FieldRequirement.NotApplicable
                && r.DegreeTypeRequired == FieldRequirement.NotApplicable
                && r.TrainingCountryRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = RouteToProfessionalStatusStatus.Holds;
        var holdsFrom = new DateOnly(2021, 1, 1);
        var setCountry = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne()
            .Name;

        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickButtonAsync("Add a route");

        await page.AssertOnRouteAddRoutePageAsync();
        await page.FillAsync($"label:text-is('Route type')", setRoute.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStatusPageAsync();
        await page.SelectStatusAsync(status);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddHoldsFromPageAsync();
        await page.FillDateInputAsync(holdsFrom);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddInductionExemptionPageAsync();
        await page.SetCheckedAsync($"label:text-is('Yes')", true);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteAddCountryAsync();
        await page.EnterCountryAsync(setCountry);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddAgeRangeAsync();
        await page.SelectAgeRangeAsync(AgeSpecializationOption.None);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddSubjectsPageAsync();
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddChangeReasonPage();
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickBackLink();

        await page.AssertOnRouteAddChangeReasonPage();
        await page.ClickBackLink();

        await page.AssertOnRouteAddSubjectsPageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddAgeRangeAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddCountryAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddInductionExemptionPageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddHoldsFromPageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddStatusPageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddRoutePageAsync();
        await page.ClickBackLink();

        await page.AssertOnPersonQualificationsPageAsync(personId);
    }

    [Fact]
    public async Task Route_AddJourneyToCyaPage_EditFields_BackToCyaPage()
    {
        var setRoute = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(true))
            .Single(r => r.Name == "Apprenticeship");
        var status = RouteToProfessionalStatusStatus.InTraining;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddMonths(1);
        var setDegreeType = "BSc (Hons) with Intercalated PGCE";
        var setProviderName = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync(true))
            .RandomOne()
            .Name;
        var setCountry = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne()
            .Name;
        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickButtonAsync("Add a route");

        await page.AssertOnRouteAddRoutePageAsync();
        await page.FillAsync($"label:text-is('Route type')", setRoute.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStatusPageAsync();
        await page.SelectStatusAsync(status);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddStartAndEndDatePageAsync();
        await page.FillDateInputAsync("TrainingStartDate", startDate);
        await page.FillDateInputAsync("TrainingEndDate", endDate);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddTrainingProviderAsync();
        await page.EnterTrainingProviderAsync(setProviderName);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddDegreeTypePageAsync();
        await page.EnterDegreeTypeAsync(setDegreeType);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteAddCountryAsync();
        await page.EnterCountryAsync(setCountry);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddAgeRangeAsync();
        await page.SelectAgeRangeAsync(AgeSpecializationOption.None);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddSubjectsPageAsync();
        // test that page is optional so don't fill it in
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddChangeReasonPage();
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteAddCheckYourAnswersPage();

        // check each edit link from cya page
        var editEndDate = endDate.AddDays(1);
        var editStartDate = startDate.AddDays(1);
        var editAwardDate = editEndDate.AddDays(1);
        var editDegreeType = await TestData.ReferenceDataCache.GetDegreeTypeByIdAsync(new Guid("c584eb2f-1419-4870-a230-5d81ae9b5f77"));
        var editAgeRange = AgeSpecializationOption.KeyStage2;
        var editCountry = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync("XQZ");
        var editSubject = await TestData.ReferenceDataCache.GetTrainingSubjectByIdAsync(new Guid("4b574f13-25c8-4d72-9bcb-1b36dca347e3"));
        var editTrainingProvider = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync())
            .RandomOne();

        await page.ClickLinkForElementWithTestIdAsync("add-start-date-link");
        await page.AssertOnRouteAddStartAndEndDatePageAsync();
        await page.FillDateInputAsync("TrainingStartDate", editStartDate);
        await page.ClickButtonAsync("Continue");
        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-start-date-link");
        await page.AssertOnRouteAddStartAndEndDatePageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-end-date-link");
        await page.AssertOnRouteAddStartAndEndDatePageAsync();
        await page.FillDateInputAsync("TrainingEndDate", editEndDate);
        await page.ClickButtonAsync("Continue");
        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-end-date-link");
        await page.AssertOnRouteAddStartAndEndDatePageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-training-provider-link");
        await page.AssertOnRouteAddTrainingProviderAsync();
        await page.EnterTrainingProviderAsync(editTrainingProvider.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");
        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-training-provider-link");
        await page.AssertOnRouteAddTrainingProviderAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-degree-type-link");
        await page.AssertOnRouteAddDegreeTypePageAsync();
        await page.EnterDegreeTypeAsync(editDegreeType.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");
        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-degree-type-link");
        await page.AssertOnRouteAddDegreeTypePageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-country-link");
        await page.AssertOnRouteAddCountryAsync();
        await page.EnterCountryAsync(editCountry.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");
        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-country-link");
        await page.AssertOnRouteAddCountryAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-age-range-link");
        await page.AssertOnRouteAddAgeRangeAsync();
        await page.SelectAgeRangeAsync(editAgeRange);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");
        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-age-range-link");
        await page.AssertOnRouteAddAgeRangeAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-subjects-link");
        await page.AssertOnRouteAddSubjectsPageAsync();
        await page.EnterSubjectAsync(editSubject.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickButtonAsync("Continue");
        await page.AssertOnRouteAddCheckYourAnswersPage();
        await page.ClickLinkForElementWithTestIdAsync("add-subjects-link");
        await page.AssertOnRouteAddSubjectsPageAsync();
        await page.ClickBackLink();

        await page.AssertOnRouteAddCheckYourAnswersPage();

        await page.AssertContentEquals(editStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), "Start date");
        await page.AssertContentEquals(editEndDate.ToString(UiDefaults.DateOnlyDisplayFormat), "End date");
        await page.AssertContentContains(editDegreeType.Name, "Degree type");
        await page.AssertContentEquals(editAgeRange.GetDisplayName()!, "Age range");
        await page.AssertContentContains(editCountry.Name, "Country of training");
        await page.AssertContentContains(editSubject.Name, "Subjects");
        await page.AssertContentContains(editTrainingProvider.Name, "Training provider");
    }
}

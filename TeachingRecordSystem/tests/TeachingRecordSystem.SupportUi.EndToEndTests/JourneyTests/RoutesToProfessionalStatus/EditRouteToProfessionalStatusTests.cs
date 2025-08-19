using TeachingRecordSystem.SupportUi.Pages.Common;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.RoutesToProfessionalStatus;

public class EditRouteToProfessionalStatusTests : TestBase
{
    private static string _countryCode = "AG";

    public EditRouteToProfessionalStatusTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task EditEachField_Cya_ShowsEditedContent()
    {
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r =>
                r.TrainingStartDateRequired != FieldRequirement.NotApplicable
                && r.TrainingEndDateRequired != FieldRequirement.NotApplicable
                && r.TrainingCountryRequired != FieldRequirement.NotApplicable
                && r.HoldsFromRequired != FieldRequirement.NotApplicable
                && r.InductionExemptionRequired != FieldRequirement.NotApplicable
                && r.DegreeTypeRequired != FieldRequirement.NotApplicable
                && r.TrainingSubjectsRequired != FieldRequirement.NotApplicable
                && r.TrainingProviderRequired != FieldRequirement.NotApplicable
                && r.TrainingAgeSpecialismTypeRequired != FieldRequirement.NotApplicable)
            .First();
        var status = RouteToProfessionalStatusStatus.Holds;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(30);
        var setEndDate = endDate.AddDays(1);
        var setStartDate = startDate.AddDays(1);
        var setHoldsFrom = setEndDate.AddDays(1);
        var setDegreeType = await TestData.ReferenceDataCache.GetDegreeTypeByIdAsync(new Guid("2f7a914f-f95f-421a-a55e-60ed88074cf2"));
        var setAgeRange = TrainingAgeSpecialismType.KeyStage1;
        var setCountry = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync(_countryCode);
        var setSubject = await TestData.ReferenceDataCache.GetTrainingSubjectByIdAsync(new Guid("015d862e-2aed-49df-9e5f-d17b0d426972"));
        var setTrainingProvider = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync())
            .RandomOne();

        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRouteType(route.RouteToProfessionalStatusTypeId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                    .WithTrainingCountryId(setCountry.CountryId)
                    .WithHoldsFrom(endDate)
                    .WithInductionExemption(true)
                ));

        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-start-date-link");

        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync("TrainingStartDate", setStartDate);
        await page.FillDateInputAsync("TrainingEndDate", setEndDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-end-date-link");

        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-holds-from-link");

        await page.AssertOnRouteEditHoldsFromPageAsync(qualificationId);
        await page.FillDateInputAsync(setHoldsFrom);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-degree-type-link");

        await page.AssertOnRouteEditDegreeTypePageAsync(qualificationId);
        await page.EnterDegreeTypeAsync(setDegreeType.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-age-range-link");

        await page.AssertOnRouteEditAgeRangePageAsync(qualificationId);
        await page.SelectAgeRangeAsync(setAgeRange);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-country-link");

        await page.AssertOnRouteEditCountryPageAsync(qualificationId);
        await page.EnterCountryAsync(setCountry.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-subjects-link");

        await page.AssertOnRouteEditSubjectsPageAsync(qualificationId);
        await page.EnterSubjectAsync(setSubject.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-training-provider-link");

        await page.AssertOnRouteEditTrainingProviderPageAsync(qualificationId);
        await page.EnterTrainingProviderAsync(setTrainingProvider.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.AssertContentEquals(setStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), "Start date");
        await page.AssertContentEquals(setEndDate.ToString(UiDefaults.DateOnlyDisplayFormat), "End date");
        await page.AssertContentContains(setDegreeType.Name, "Degree type");
        await page.AssertContentEquals(setAgeRange.GetDisplayName()!, "Age range");
        await page.AssertContentContains(setCountry.Name, "Country of training");
        await page.AssertContentContains(setSubject.Name, "Subjects");
        await page.AssertContentContains(setTrainingProvider.Name, "Training provider");

        // check each edit link from cya page
        var editEndDate = setEndDate.AddDays(1);
        var editStartDate = setStartDate.AddDays(1);
        var editHoldDate = editEndDate.AddDays(1);
        var editDegreeType = await TestData.ReferenceDataCache.GetDegreeTypeByIdAsync(new Guid("c584eb2f-1419-4870-a230-5d81ae9b5f77"));
        var editAgeRange = TrainingAgeSpecialismType.KeyStage2;
        var editCountry = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync("XQZ");
        var editSubject = await TestData.ReferenceDataCache.GetTrainingSubjectByIdAsync(new Guid("4b574f13-25c8-4d72-9bcb-1b36dca347e3"));
        var editTrainingProvider = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync())
            .RandomOne();

        await page.ClickLinkForElementWithTestIdAsync("edit-start-date-link");
        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync("TrainingStartDate", editStartDate);
        await page.FillDateInputAsync("TrainingEndDate", editEndDate);
        await page.ClickContinueButtonAsync();
        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-start-date-link");
        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-end-date-link");
        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-end-date-link");
        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-training-provider-link");
        await page.AssertOnRouteEditTrainingProviderPageAsync(qualificationId);
        await page.EnterTrainingProviderAsync(editTrainingProvider.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();
        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-training-provider-link");
        await page.AssertOnRouteEditTrainingProviderPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-degree-type-link");
        await page.AssertOnRouteEditDegreeTypePageAsync(qualificationId);
        await page.EnterDegreeTypeAsync(editDegreeType.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();
        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-degree-type-link");
        await page.AssertOnRouteEditDegreeTypePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-country-link");
        await page.AssertOnRouteEditCountryPageAsync(qualificationId);
        await page.EnterCountryAsync(editCountry.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();
        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-country-link");
        await page.AssertOnRouteEditCountryPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-age-range-link");
        await page.AssertOnRouteEditAgeRangePageAsync(qualificationId);
        await page.SelectAgeRangeAsync(editAgeRange);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();
        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-age-range-link");
        await page.AssertOnRouteEditAgeRangePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-subjects-link");
        await page.AssertOnRouteEditSubjectsPageAsync(qualificationId);
        await page.EnterSubjectAsync(editSubject.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();
        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-subjects-link");
        await page.AssertOnRouteEditSubjectsPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.AssertContentEquals(editStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), "Start date");
        await page.AssertContentEquals(editEndDate.ToString(UiDefaults.DateOnlyDisplayFormat), "End date");
        await page.AssertContentContains(editDegreeType.Name, "Degree type");
        await page.AssertContentEquals(editAgeRange.GetDisplayName()!, "Age range");
        await page.AssertContentContains(editCountry.Name, "Country of training");
        await page.AssertContentContains(editSubject.Name, "Subjects");
        await page.AssertContentContains(editTrainingProvider.Name, "Training provider");

        await page.ClickButtonAsync("Confirm and update route");
        await page.AssertOnPersonQualificationsPageAsync(personId);
    }

    [Fact]
    public async Task Details_BackLink_QualificationPage()
    {
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(1);
        var setEndDate = startDate.AddDays(2);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRouteType(route.RouteToProfessionalStatusTypeId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditEndDate_ToCya_EditEndDate_Continue()
    {
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r =>
                r.TrainingEndDateRequired != FieldRequirement.NotApplicable
                && r.TrainingProviderRequired == FieldRequirement.Optional
                && r.DegreeTypeRequired == FieldRequirement.Optional
                && r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional
                && r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .First();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var country = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync(_countryCode);
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(1);
        var setEndDate = startDate.AddDays(2);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRouteType(route.RouteToProfessionalStatusTypeId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                    .WithTrainingCountryId(country.CountryId)
                ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-end-date-link");

        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync("TrainingEndDate", setEndDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-end-date-link");

        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync("TrainingEndDate", setEndDate.AddDays(1));
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Confirm and update route");
    }

    [Fact]
    public async Task EditStartDate_ToCya_EditStartDate_Continue()
    {
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r =>
                r.TrainingEndDateRequired != FieldRequirement.NotApplicable
                && r.TrainingProviderRequired == FieldRequirement.Optional
                && r.DegreeTypeRequired == FieldRequirement.Optional
                && r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional
                && r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .First();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(30);
        var setStartDate = startDate.AddDays(2);
        var country = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync(_countryCode);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRouteType(route.RouteToProfessionalStatusTypeId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                    .WithTrainingCountryId(country.CountryId)
                ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-start-date-link");

        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-start-date-link");

        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setStartDate.AddDays(1));
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Confirm and update route");

        await page.AssertOnPersonQualificationsPageAsync(personId);
    }

    [Fact]
    public async Task EditEndDate_BackLinks()
    {
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r =>
                r.TrainingEndDateRequired != FieldRequirement.NotApplicable
                && r.TrainingProviderRequired == FieldRequirement.Optional
                && r.DegreeTypeRequired == FieldRequirement.Optional
                && r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional
                && r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .First();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(1);
        var country = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync(_countryCode);
        var setEndDate = startDate.AddDays(2);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRouteType(route.RouteToProfessionalStatusTypeId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                    .WithTrainingCountryId(country.CountryId)
                ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-end-date-link");

        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-end-date-link");

        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync("TrainingEndDate", setEndDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-end-date-link");

        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Confirm and update route");
    }

    [Fact]
    public async Task EditStartDate_BackLinks()
    {
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r =>
                r.TrainingEndDateRequired != FieldRequirement.NotApplicable
                && r.TrainingProviderRequired == FieldRequirement.Optional
                && r.DegreeTypeRequired == FieldRequirement.Optional
                && r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional
                && r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .First();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(30);
        var setStartDate = startDate.AddDays(2);
        var country = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync(_countryCode);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRouteType(route.RouteToProfessionalStatusTypeId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                    .WithTrainingCountryId(country.CountryId)
                ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-start-date-link");

        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-start-date-link");

        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-start-date-link");

        await page.AssertOnRouteEditStartAndEndDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Confirm and update route");
    }

    [Fact]
    public async Task EditHoldsFrom_BackLinks()
    {
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = RouteToProfessionalStatusStatus.Holds;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(30);
        var setHoldsFrom = endDate.AddDays(1);
        var country = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync(_countryCode);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRouteType(route.RouteToProfessionalStatusTypeId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                    .WithHoldsFrom(setHoldsFrom)
                    .WithTrainingCountryId(country.CountryId)
                    .WithInductionExemption(false)
                ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-holds-from-link");

        await page.AssertOnRouteEditHoldsFromPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-holds-from-link");

        await page.AssertOnRouteEditHoldsFromPageAsync(qualificationId);
        await page.FillDateInputAsync(setHoldsFrom);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-holds-from-link");

        await page.AssertOnRouteEditHoldsFromPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Confirm and update route");

        await page.AssertOnPersonQualificationsPageAsync(personId);
    }

    [Fact]
    public async Task EditDegreeType_CanClearField_BackLinkReturnsToDetails()
    {
        // this route-status combo makes the degree type field mandatory
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.DegreeTypeRequired == FieldRequirement.Mandatory)
            .First();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var setDegreeType = "BSc (Hons) with Intercalated PGCE";
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRouteType(route.RouteToProfessionalStatusTypeId)
                    .WithStatus(status)
                ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-degree-type-link");

        await page.AssertOnRouteEditDegreeTypePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-degree-type-link");

        await page.AssertOnRouteEditDegreeTypePageAsync(qualificationId);
        await page.EnterDegreeTypeAsync(setDegreeType);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.AssertContentContains(setDegreeType, "Degree type");
        await page.ClickLinkForElementWithTestIdAsync("edit-degree-type-link");

        await page.AssertOnRouteEditDegreeTypePageAsync(qualificationId);
        await page.EnterDegreeTypeAsync("");
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();
        page.AssertErrorSummary();
        await page.EnterDegreeTypeAsync(setDegreeType);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
    }

    [Fact]
    public async Task EditCountry_CanClearField_BackLinkReturnsToDetails()
    {
        // this route - status combo makes the country field mandatory
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingCountryRequired == FieldRequirement.Mandatory)
            .First();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var setCountry = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne();
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRouteType(route.RouteToProfessionalStatusTypeId)
                    .WithStatus(status)
                ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-country-link");

        await page.AssertOnRouteEditCountryPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-country-link");

        await page.AssertOnRouteEditCountryPageAsync(qualificationId);
        await page.EnterCountryAsync(setCountry.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.AssertContentContains(setCountry.Name, "Country");
        await page.ClickLinkForElementWithTestIdAsync("edit-country-link");

        await page.AssertOnRouteEditCountryPageAsync(qualificationId);
        await page.EnterCountryAsync("");
        await page.ClickContinueButtonAsync();
        page.AssertErrorSummary();
        await page.EnterCountryAsync(setCountry.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.AssertContentContains(setCountry.Name, "Country");
    }

    [Fact]
    public async Task EditAgeRangeSpecialism_IncompleteInformation_ShowsError()
    {
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Single(r => r.Name == "Early Years Teacher Degree Apprenticeship");
        var status = RouteToProfessionalStatusStatus.InTraining;
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRouteType(route.RouteToProfessionalStatusTypeId)
                    .WithStatus(status)
                ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-age-range-link");

        await page.AssertOnRouteEditAgeRangePageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditAgeRangePageAsync(qualificationId);
        page.AssertErrorSummary();
        await page.SelectAgeRangeAsync(TrainingAgeSpecialismType.Range);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditAgeRangePageAsync(qualificationId);
        page.AssertErrorSummary();
        await page.FillAsync($"label:text-is('From')", "6");
        await page.FillAsync($"label:text-is('To')", "1");
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditAgeRangePageAsync(qualificationId);
        page.AssertErrorSummary();
        await page.FillAsync($"label:text-is('To')", "11");
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
    }

    [Fact]
    public async Task EditTrainingProvider_CanClearField_BackLinkReturnsToDetails()
    {
        // this route - status combo makes the provider field optional
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingProviderRequired == FieldRequirement.Optional)
            .First();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var newTrainingProvider = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRouteType(route.RouteToProfessionalStatusTypeId)
                    .WithStatus(status)
                ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-training-provider-link");

        await page.AssertOnRouteEditTrainingProviderPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-training-provider-link");

        await page.AssertOnRouteEditTrainingProviderPageAsync(qualificationId);
        await page.EnterTrainingProviderAsync(newTrainingProvider.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.AssertContentContains(newTrainingProvider.Name, "Training provider");
        await page.ClickLinkForElementWithTestIdAsync("edit-training-provider-link");

        await page.AssertOnRouteEditTrainingProviderPageAsync(qualificationId);
        await page.EnterTrainingProviderAsync("");
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.AssertContentContains("Not provided", "Training provider");
    }

    [Fact]
    public async Task EditSubjectSpecialisms_CanClearField_BackLinkReturnsToDetails()
    {
        // this route-status combo makes the subjects field optional
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .First();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var country = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne();
        var newSubject = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
            .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingStartDate(new DateOnly(2021, 2, 1))
                .WithTrainingEndDate(new DateOnly(2021, 2, 2))
                .WithHoldsFrom(new DateOnly(2021, 2, 2))
                .WithTrainingCountryId(country.CountryId)
            ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-subjects-link");

        await page.AssertOnRouteEditSubjectsPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-subjects-link");

        await page.AssertOnRouteEditSubjectsPageAsync(qualificationId);
        await page.EnterSubjectAsync(newSubject.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();
        await page.ClickLinkForElementWithTestIdAsync("edit-subjects-link");

        await page.AssertOnRouteEditSubjectsPageAsync(qualificationId);
        await page.EnterSubjectAsync("");
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-subjects-link");

        await page.AssertOnRouteEditSubjectsPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
    }

    [Fact]
    public async Task EditInductionExemption_BackLinks()
    {
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.Name == "Northern Irish Recognition")
            .First();
        var country = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne();
        var status = RouteToProfessionalStatusStatus.Holds;
        var newSubject = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
            .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingStartDate(new DateOnly(2021, 2, 1))
                .WithTrainingEndDate(new DateOnly(2021, 2, 2))
                .WithHoldsFrom(new DateOnly(2021, 2, 2))
                .WithTrainingCountryId(country.CountryId)
            ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-induction-exemption-link");

        await page.AssertOnRouteEditInductionExemptionPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-induction-exemption-link");

        await page.AssertOnRouteEditInductionExemptionPageAsync(qualificationId);
        await page.SetCheckedAsync($"label:text-is('Yes')", true);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-induction-exemption-link");

        await page.AssertOnRouteEditInductionExemptionPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Confirm and update route");
    }

    [Fact]
    public async Task EditStatus_Holds_Continue_Exemption_Continue()
    {
        var holdsFrom = new DateOnly(2021, 1, 1);
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionReasonId.HasValue)
            .Join(
                (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync()).Where(e => e.RouteImplicitExemption == false),
                r => r.InductionExemptionReasonId,
                e => e.InductionExemptionReasonId,
                (r, e) => r
            )
            .RandomOne();

        var status = RouteToProfessionalStatusStatus.InTraining;
        var newSubject = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
            .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
            ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-status-link");

        await page.AssertOnRouteEditStatusPageAsync(qualificationId);
        await page.ClickRadioAsync("Holds");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditHoldsFromPageAsync(qualificationId);
        await page.FillDateInputAsync(holdsFrom);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditInductionExemptionPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteEditHoldsFromPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteEditStatusPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditHoldsFromPageAsync(qualificationId);
        await page.FillDateInputAsync(holdsFrom);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditInductionExemptionPageAsync(qualificationId);
        await page.SetCheckedAsync($"label:text-is('Yes')", true);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
    }

    [Fact]
    public async Task EditStatusRouteWithImplicitExemption_Holds_Continue_Cya_EditStatus_Continue_Cya()
    {
        var holdsFrom = new DateOnly(2021, 1, 1);
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionReason is not null && r.InductionExemptionReason.RouteImplicitExemption == true)
            .RandomOne();
        var country = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var newSubject = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync()).RandomOne();
        var provider = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
            .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingStartDate(new DateOnly(2021, 2, 1))
                .WithTrainingEndDate(new DateOnly(2021, 2, 2))
                .WithHoldsFrom(new DateOnly(2021, 2, 2))
                .WithTrainingCountryId(country.CountryId)
                .WithTrainingProviderId(provider.TrainingProviderId)
            ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-status-link");

        await page.AssertOnRouteEditStatusPageAsync(qualificationId);
        await page.ClickRadioAsync("Holds");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditHoldsFromPageAsync(qualificationId);
        await page.FillDateInputAsync(holdsFrom);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectRouteChangeReasonOption(ChangeReasonOption.AnotherReason.ToString());
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.AssertContentEquals(holdsFrom.ToString(UiDefaults.DateOnlyDisplayFormat), "Professional status date");
        await page.AssertContentEquals("Yes", "Induction exemption");

        await page.ClickLinkForElementWithTestIdAsync("edit-status-link");
        await page.AssertOnRouteEditStatusPageAsync(qualificationId);
        await page.ClickRadioAsync("Deferred");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.AssertContentEquals("Deferred", "Status");
        await page.ClickButtonAsync("Confirm and update route");
    }

    [Theory]
    [InlineData("Deferred")]
    [InlineData("Holds")]
    public async Task EditStatus_StatusAlreadyHolds_Continue_Details(string status)
    {
        var holdsFrom = new DateOnly(2021, 1, 1);
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionRequired == FieldRequirement.NotApplicable)
            .RandomOne();
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
            .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(holdsFrom)
            ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-status-link");

        await page.AssertOnRouteEditStatusPageAsync(qualificationId);
        await page.ClickRadioAsync(status);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
    }

    [Fact]
    public async Task EditStatus_Holds_Continue_Exemption_Back()
    {
        var holdsFrom = new DateOnly(2021, 1, 1);
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionReasonId.HasValue)
            .Join(
                (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync()).Where(e => e.RouteImplicitExemption == false),
                r => r.InductionExemptionReasonId,
                e => e.InductionExemptionReasonId,
                (r, e) => r
            )
            .RandomOne();

        var status = RouteToProfessionalStatusStatus.InTraining;
        var newSubject = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
            .WithRouteToProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
            ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-status-link");

        await page.AssertOnRouteEditStatusPageAsync(qualificationId);
        await page.ClickRadioAsync("Holds");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditHoldsFromPageAsync(qualificationId);
        await page.FillDateInputAsync(holdsFrom);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditInductionExemptionPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteEditHoldsFromPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteEditStatusPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
    }
}

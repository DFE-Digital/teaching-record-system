using TeachingRecordSystem.SupportUi.Pages.Common;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

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
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = ProfessionalStatusStatus.Awarded;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(30);
        var setEndDate = endDate.AddDays(1);
        var setStartDate = startDate.AddDays(1);
        var setAwardDate = setEndDate.AddDays(1);
        var setDegreeType = await TestData.ReferenceDataCache.GetDegreeTypeByIdAsync(new Guid("2f7a914f-f95f-421a-a55e-60ed88074cf2"));
        var setAgeRange = TrainingAgeSpecialismType.KeyStage1;
        var setCountry = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync(_countryCode);
        var setSubject = await TestData.ReferenceDataCache.GetTrainingSubjectsByIdAsync(new Guid("015d862e-2aed-49df-9e5f-d17b0d426972"));
        //var setTrainingProvider = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync())
        //    .RandomOne();
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRoute(route.RouteToProfessionalStatusId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                    .WithTrainingCountry(setCountry)
                    .WithAwardedDate(endDate)
                    .WithExemptFromInduction(true)
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

        await page.AssertOnRouteEditStartDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-end-date-link");

        await page.AssertOnRouteEditEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setEndDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-award-date-link");

        await page.AssertOnRouteEditAwardDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setAwardDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-degree-type-link");

        await page.AssertOnRouteEditDegreeTypePageAsync(qualificationId);
        await page.FillAsync("label:text-is('Enter the degree type awarded as part of this route')", setDegreeType.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-age-range-type-link");

        await page.AssertOnRouteEditAgeRangePageAsync(qualificationId);
        await page.SelectAgeTypeAsync(setAgeRange);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-country-link");

        await page.AssertOnRouteEditCountryPageAsync(qualificationId);
        await page.FillAsync("label:text-is('Enter the country associated with their route')", setCountry.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-subjects-link");

        await page.AssertOnRouteEditSubjectsPageAsync(qualificationId);
        await page.FillAsync("label:text-is('Enter the subject they specialise in teaching')", setSubject.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        //await page.AssertOnRouteDetailPageAsync(qualificationId);
        //await page.ClickLinkForElementWithTestIdAsync("edit-training-provider-link");

        //await page.AssertOnRouteEditTrainingProviderPageAsync(qualificationId);
        //await page.FillAsync("label:text-is('Enter the training provider for this route')", setTrainingProvider.Name);
        //await page.FocusAsync("button:text-is('Continue')");
        //await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
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
        //await page.AssertContentContains(setTrainingProvider.Name, "Training provider");
        await page.ClickButtonAsync("Confirm and commit changes");

        await page.AssertOnPersonQualificationsPageAsync(personId);
    }

    [Fact]
    public async Task Details_BackLink_QualificationPage()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();

        var status = ProfessionalStatusStatus.Approved;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(1);
        var setEndDate = startDate.AddDays(2);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRoute(route.RouteToProfessionalStatusId)
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
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = ProfessionalStatusStatus.InTraining;
        var country = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync(_countryCode);
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(1);
        var setEndDate = startDate.AddDays(2);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRoute(route.RouteToProfessionalStatusId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                    .WithTrainingCountry(country)
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

        await page.AssertOnRouteEditEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setEndDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-end-date-link");

        await page.AssertOnRouteEditEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setEndDate.AddDays(1));
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Confirm and commit changes");
    }

    [Fact]
    public async Task EditStartDate_ToCya_EditStartDate_Continue()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = ProfessionalStatusStatus.InTraining;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(30);
        var setStartDate = startDate.AddDays(2);
        var country = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync(_countryCode);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRoute(route.RouteToProfessionalStatusId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                    .WithTrainingCountry(country)
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

        await page.AssertOnRouteEditStartDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-start-date-link");

        await page.AssertOnRouteEditStartDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setStartDate.AddDays(1));
        await page.ClickButtonAsync("Continue");

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Confirm and commit changes");

        await page.AssertOnPersonQualificationsPageAsync(personId);
    }

    [Fact]
    public async Task EditStartDate_InvalidatesEndDate_GoesToEndDate_Continue()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = ProfessionalStatusStatus.Approved;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(30);
        var setStartDate = endDate.AddDays(2);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRoute(route.RouteToProfessionalStatusId)
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
        await page.ClickLinkForElementWithTestIdAsync("edit-start-date-link");

        await page.AssertOnRouteEditStartDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditEndDatePageAsync(qualificationId);
        await page.ClickContinueButtonAsync();
        await page.AssertOnRouteEditEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setStartDate.AddDays(1));
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
    }

    [Fact]
    public async Task EditEndDate_BackLinks()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = ProfessionalStatusStatus.InTraining;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(1);
        var country = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync(_countryCode);
        var setEndDate = startDate.AddDays(2);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRoute(route.RouteToProfessionalStatusId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                    .WithTrainingCountry(country)
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

        await page.AssertOnRouteEditEndDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-end-date-link");

        await page.AssertOnRouteEditEndDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setEndDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-end-date-link");

        await page.AssertOnRouteEditEndDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Confirm and commit changes");
    }

    [Fact]
    public async Task EditStartDate_BackLinks()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = ProfessionalStatusStatus.InTraining;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(30);
        var setStartDate = startDate.AddDays(2);
        var country = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync(_countryCode);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRoute(route.RouteToProfessionalStatusId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                    .WithTrainingCountry(country)
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

        await page.AssertOnRouteEditStartDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-start-date-link");

        await page.AssertOnRouteEditStartDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-start-date-link");

        await page.AssertOnRouteEditStartDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Confirm and commit changes");
    }

    [Fact]
    public async Task EditAwardDate_BackLinks()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = ProfessionalStatusStatus.Approved;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(30);
        var setAwardDate = endDate.AddDays(1);
        var country = await TestData.ReferenceDataCache.GetTrainingCountryByIdAsync(_countryCode);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRoute(route.RouteToProfessionalStatusId)
                    .WithStatus(status)
                    .WithTrainingStartDate(startDate)
                    .WithTrainingEndDate(endDate)
                    .WithTrainingCountry(country)
                    .WithExemptFromInduction(false)
                ));
        var personId = person.PersonId;
        var qualificationId = person.ProfessionalStatuses.Single().QualificationId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonQualificationsPageAsync(person.PersonId);

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
        await page.ClickLinkForElementWithTestIdAsync($"edit-route-link-{qualificationId}");

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-award-date-link");

        await page.AssertOnRouteEditAwardDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-award-date-link");

        await page.AssertOnRouteEditAwardDatePageAsync(qualificationId);
        await page.FillDateInputAsync(setAwardDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-award-date-link");

        await page.AssertOnRouteEditAwardDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Confirm and commit changes");

        await page.AssertOnPersonQualificationsPageAsync(personId);
    }

    [Fact]
    public async Task EditDegreeType_BackLinks()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = ProfessionalStatusStatus.Approved;
        var setDegreeType = "BSc (Hons) with Intercalated PGCE";
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRoute(route.RouteToProfessionalStatusId)
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
        await page.FillAsync($"label:text-is('Enter the degree type awarded as part of this route')", setDegreeType);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
    }

    [Fact]
    public async Task EditCountry_BackLinks()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingCountryRequired == FieldRequirement.Optional)
            .First();
        var status = ProfessionalStatusStatus.Approved;
        var setCountry = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne();
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRoute(route.RouteToProfessionalStatusId)
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
        await page.FillAsync($"label:text-is('Enter the country associated with their route')", setCountry.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
    }

    [Fact]
    public async Task EditAgeRangeSpecialism_IncompleteInformation_ShowsError()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = ProfessionalStatusStatus.Approved;
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRoute(route.RouteToProfessionalStatusId)
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
        await page.ClickLinkForElementWithTestIdAsync("edit-age-range-type-link");

        await page.AssertOnRouteEditAgeRangePageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditAgeRangePageAsync(qualificationId);
        page.AssertErrorSummary();
        await page.SelectAgeTypeAsync(TrainingAgeSpecialismType.None);
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

    [Fact(Skip = "Waiting for training_provider table to be populated")]
    public async Task EditTrainingProvider_BackLinks()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingProviderRequired == FieldRequirement.Optional)
            .First();
        var status = ProfessionalStatusStatus.Approved;
        var newTrainingProvider = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                    .WithRoute(route.RouteToProfessionalStatusId)
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

        await page.AssertOnRouteEditDegreeTypePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-training-provider-link");

        await page.AssertOnRouteEditDegreeTypePageAsync(qualificationId);
        await page.FillAsync($"label:text-is('Enter the training provider for this route')", newTrainingProvider.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
    }

    [Fact]
    public async Task EditSubjectSpecialisms_BackLinks()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .First();
        var status = ProfessionalStatusStatus.InTraining;
        var country = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne();
        var newSubject = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
            .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(status)
                .WithTrainingStartDate(new DateOnly(2021, 2, 1))
                .WithTrainingEndDate(new DateOnly(2021, 2, 2))
                .WithAwardedDate(new DateOnly(2021, 2, 2))
                .WithTrainingCountry(country)
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
        await page.FillAsync($"label:text-is('Enter the subject they specialise in teaching')", newSubject.Name);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
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
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.Name == "NI R")
            .First();
        var country = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne();
        var status = ProfessionalStatusStatus.Approved;
        var newSubject = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
            .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(status)
                .WithTrainingStartDate(new DateOnly(2021, 2, 1))
                .WithTrainingEndDate(new DateOnly(2021, 2, 2))
                .WithAwardedDate(new DateOnly(2021, 2, 2))
                .WithTrainingCountry(country)
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
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickLinkForElementWithTestIdAsync("edit-induction-exemption-link");

        await page.AssertOnRouteEditInductionExemptionPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.ClickButtonAsync("Confirm and commit changes");
    }

    [Fact]
    public async Task EditStatus_Awarded_Continue_Exemption_Continue()
    {
        var awardDate = new DateOnly(2021, 1, 1);
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.InductionExemptionReasonId.HasValue)
            .Join(
                (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync()).Where(e => e.RouteImplicitExemption == false),
                r => r.InductionExemptionReasonId,
                e => e.InductionExemptionReasonId,
                (r, e) => r
            )
            .RandomOne();

        var status = ProfessionalStatusStatus.InTraining;
        var newSubject = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
            .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                .WithRoute(route.RouteToProfessionalStatusId)
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
        await page.ClickRadioAsync("Awarded");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditAwardDatePageAsync(qualificationId);
        await page.FillDateInputAsync(awardDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditInductionExemptionPageAsync(qualificationId);
        await page.SetCheckedAsync($"label:text-is('Yes')", true);
        await page.FocusAsync("button:text-is('Continue')");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
    }

    [Fact]
    public async Task EditStatusRouteWithImplicitExemption_Awarded_Continue_Cya_EditStatus_Continue_Cya()
    {
        var awardDate = new DateOnly(2021, 1, 1);
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.InductionExemptionReasonId.HasValue)
            .Join(
                (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync()).Where(e => e.RouteImplicitExemption == true),
                r => r.InductionExemptionReasonId,
                e => e.InductionExemptionReasonId,
                (r, e) => r
            )
            .RandomOne();
        var country = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne();
        var status = ProfessionalStatusStatus.InTraining;
        var newSubject = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
            .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(status)
                .WithTrainingStartDate(new DateOnly(2021, 2, 1))
                .WithTrainingEndDate(new DateOnly(2021, 2, 2))
                .WithAwardedDate(new DateOnly(2021, 2, 2))
                .WithTrainingCountry(country)
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
        await page.ClickRadioAsync("Awarded");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditAwardDatePageAsync(qualificationId);
        await page.FillDateInputAsync(awardDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteChangeReasonPageAsync(qualificationId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.AssertContentEquals(awardDate.ToString(UiDefaults.DateOnlyDisplayFormat), "Award date");
        await page.AssertContentEquals("Yes", "Has exemption");

        await page.ClickLinkForElementWithTestIdAsync("edit-status-link");
        await page.AssertOnRouteEditStatusPageAsync(qualificationId);
        await page.ClickRadioAsync("Deferred");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteCheckYourAnswersPageAsync(qualificationId);
        await page.AssertContentEquals("Deferred", "Status");
        await page.ClickButtonAsync("Confirm and commit changes");
    }

    [Theory]
    [InlineData("Deferred")]
    [InlineData("Awarded")]
    public async Task EditStatus_StatusAlreadyAwarded_Continue_Details(string status)
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.InductionExemptionRequired == FieldRequirement.NotApplicable)
            .RandomOne();
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
            .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Awarded)
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
    public async Task EditStatus_Awarded_Continue_Exemption_Back()
    {
        var awardDate = new DateOnly(2021, 1, 1);
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.InductionExemptionReasonId.HasValue)
            .Join(
                (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync()).Where(e => e.RouteImplicitExemption == false),
                r => r.InductionExemptionReasonId,
                e => e.InductionExemptionReasonId,
                (r, e) => r
            )
            .RandomOne();

        var status = ProfessionalStatusStatus.InTraining;
        var newSubject = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
            .WithProfessionalStatus(professionalStatusBuilder => professionalStatusBuilder
                .WithRoute(route.RouteToProfessionalStatusId)
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
        await page.ClickRadioAsync("Awarded");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditAwardDatePageAsync(qualificationId);
        await page.FillDateInputAsync(awardDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRouteEditInductionExemptionPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteEditAwardDatePageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteEditStatusPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnRouteDetailPageAsync(qualificationId);
        await page.ClickBackLink();

        await page.AssertOnPersonQualificationsPageAsync(person.PersonId);
    }
}

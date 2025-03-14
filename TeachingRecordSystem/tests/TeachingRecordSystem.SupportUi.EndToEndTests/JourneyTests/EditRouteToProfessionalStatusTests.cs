using TeachingRecordSystem.SupportUi.Pages.Common;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class EditRouteToProfessionalStatusTests : TestBase
{
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
        var status = ProfessionalStatusStatus.Approved;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(30);
        var setEndDate = endDate.AddDays(1);
        var setStartDate = startDate.AddDays(1);
        var setAwardDate = setEndDate.AddDays(1);
        var setDegreeType = "BSc (Hons) with Intercalated PGCE";
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
        await page.FillAsync("label:text-is('Enter the degree type awarded as part of this route')", setDegreeType);
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
        await page.AssertContentEquals(setStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), "Start date");
        await page.AssertContentEquals(setEndDate.ToString(UiDefaults.DateOnlyDisplayFormat), "End date");
        await page.AssertContentEquals(setDegreeType, "Degree type");
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

        await page.AssertOnPersonQualificationsPageAsync(personId);
    }

    [Fact]
    public async Task EditStartDate_ToCya_EditStartDate_Continue()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = ProfessionalStatusStatus.Approved;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(30);
        var setStartDate = startDate.AddDays(2);
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

        await page.AssertOnPersonQualificationsPageAsync(personId);
    }

    [Fact]
    public async Task EditStartDate_BackLinks()
    {
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .First();
        var status = ProfessionalStatusStatus.Approved;
        var startDate = new DateOnly(2021, 1, 1);
        var endDate = startDate.AddDays(30);
        var setStartDate = startDate.AddDays(2);
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
}

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
    public async Task Cya_ShowsEditedContent()
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
        await page.AssertContentEquals(setEndDate.ToString(UiDefaults.DateOnlyDisplayFormat), "End date");
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
}

using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class InductionTests : TestBase
{
    public InductionTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Fact]
    public async Task EditInductionStatus_InductionStatusPassed()
    {
        var startDate = new DateOnly(2021, 1, 1);
        var completedDate = startDate.AddDays(1);
        var setStartDate = startDate.AddDays(1).AddMonths(1).AddYears(1);
        var setCompletedDate = setStartDate.AddDays(1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.RequiredToComplete)));
        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.ClickEditInductionStatusPageAsync();

        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);
        await page.SelectStatusAsync(InductionStatus.Passed);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setCompletedDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickButtonAsync("Confirm induction details");
        await page.AssertOnPersonInductionPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditInductionStatus_InductionStatusExempt()
    {
        var exemptionReasonId = InductionExemptionReason.ExemptId;
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.RequiredToComplete)));
        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.ClickEditInductionStatusPageAsync();

        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);
        await page.SelectStatusAsync(InductionStatus.Exempt);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionExemptionReasonPageAsync(person.PersonId);
        await page.SelectExemptionReasonAsync(exemptionReasonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditInductionStatusExempt_NavigateBack()
    {
        var exemptionReasonId = InductionExemptionReason.ExemptDataLossOrErrorCriteriaId;
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.RequiredToComplete)));
        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.ClickEditInductionStatusPageAsync();

        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);
        await page.SelectStatusAsync(InductionStatus.Exempt);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionExemptionReasonPageAsync(person.PersonId);
        await page.SelectExemptionReasonAsync(exemptionReasonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionExemptionReasonPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditInductionStatus_NavigateBack()
    {
        var inductionStatusToSelect = InductionStatus.Failed;
        var startDate = new DateOnly(2021, 1, 1);
        var completedDate = startDate.AddDays(1);
        var setStartDate = startDate.AddDays(1).AddMonths(1).AddYears(1);
        var setCompletedDate = setStartDate.AddDays(1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.Passed)
                    .WithStartDate(startDate)
                    .WithCompletedDate(completedDate)
                ));
        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.ClickEditInductionStatusPageAsync();

        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);
        await page.SelectStatusAsync(inductionStatusToSelect);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setCompletedDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.AssertDateInputAsync(setCompletedDate);
        await page.ClickBackLink();

        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.AssertDateInputAsync(setStartDate);
        await page.ClickBackLink();

        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);
        await page.AssertInductionStatusSelected(inductionStatusToSelect);
    }

    [Fact]
    public async Task EditInductionStartDate_NavigateBack()
    {
        var startDate = new DateOnly(2021, 1, 1);
        var completedDate = startDate.AddDays(1);
        var setStartDate = startDate.AddDays(1).AddMonths(1).AddYears(1);
        var setCompletedDate = setStartDate.AddDays(1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.Passed)
                    .WithStartDate(startDate)
                    .WithCompletedDate(completedDate)
                ));
        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.ClickEditInductionStartDatePageAsync();

        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setCompletedDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.AssertDateInputAsync(setCompletedDate);
        await page.ClickBackLink();

        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.AssertDateInputAsync(setStartDate);
        await page.ClickBackLink();

        await page.AssertOnPersonInductionPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditInductionCompletedDate_NavigateBack()
    {
        var startDate = new DateOnly(2021, 1, 1);
        var completedDate = startDate.AddDays(1);
        var setCompletedDate = startDate.AddDays(1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.Passed)
                    .WithStartDate(startDate)
                    .WithCompletedDate(completedDate)
                ));
        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.ClickEditInductionCompletedDatePageAsync();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setCompletedDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(true, TestData.GenerateLoremIpsum());
        await page.SelectReasonFileUploadAsync(true, "document.jpeg");
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.AssertDateInputAsync(setCompletedDate);
        await page.ClickBackLink();

        await page.AssertOnPersonInductionPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditStartDate_CYA_ChangeStartDateThatInvalidatesCompletedDate_ContinueToCompletedDate()
    {
        var startDate = new DateOnly(2021, 1, 1);
        var completedDate = startDate.AddYears(1);
        var setStartDate = completedDate.AddDays(1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.Passed)
                    .WithStartDate(startDate)
                    .WithCompletedDate(completedDate)));
        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.ClickEditInductionStartDatePageAsync();

        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditInductionStatus_CYA_ChangeAnyFieldOtherThanStatus_ContinueToCYA()
    {
        var startDate = new DateOnly(2021, 1, 1);
        var completedDate = startDate.AddYears(1);
        var setStartDate = startDate.AddDays(1);
        var setCompletedDate = completedDate.AddDays(1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.RequiredToComplete)));
        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.ClickEditInductionStatusPageAsync();

        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);
        await page.SelectStatusAsync(InductionStatus.Passed);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setCompletedDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Induction started on");
        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Induction completed on");
        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Reason details");
        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditInductionStatus_CYA_ChangeStatus_ContinueThroughJourneyToCYA()
    {
        var startDate = new DateOnly(2021, 1, 1);
        var completedDate = startDate.AddYears(1);
        var setStartDate = startDate.AddDays(1);
        var setCompletedDate = completedDate.AddDays(1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.RequiredToComplete)));

        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.ClickEditInductionStatusPageAsync();

        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);
        await page.SelectStatusAsync(InductionStatus.Passed);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setCompletedDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Induction status");

        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditInductionStatus_CYA_ChangeSomething_NavigatesBackToCYA()
    {
        var startDate = new DateOnly(2021, 1, 1);
        var completedDate = startDate.AddYears(1);
        var setStartDate = startDate.AddDays(1);
        var setCompletedDate = completedDate.AddDays(1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.RequiredToComplete)));
        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.ClickEditInductionStatusPageAsync();

        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);

        await page.SelectStatusAsync(InductionStatus.Passed);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(startDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(completedDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Induction status");
        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Induction started on");
        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Induction completed on");
        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Reason details");
        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
    }

    [Fact]
    public async Task PersonHasRouteInductionExemption_Edit_NavigateBack()
    {
        var holdsFromDate = new DateOnly(2024, 1, 1);
        var exemptionReasonId = InductionExemptionReason.PassedInductionInNorthernIrelandId;
        var routeType = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionReasonId == exemptionReasonId)
            .Single();
        var person = await TestData.CreatePersonAsync(personBuilder => personBuilder
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(routeType.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithInductionExemption(true)
                .WithHoldsFrom(holdsFromDate))
            .WithInductionStatus(inductionBuilder => inductionBuilder
                .WithStatus(InductionStatus.Exempt)
                .WithExemptionReasons(exemptionReasonId)));
        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.AssertContentContains(routeType.InductionExemptionReason!.Name, "Route induction exemption reason");

        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Route induction exemption reason");

        await page.AssertOnRouteDetailPageAsync(person.ProfessionalStatuses.Single().QualificationId);
        await page.ClickBackLink();

        await page.AssertOnPersonInductionPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditInductionExemptionReason_NavigateBack()
    {
        var exemptionReasonId = InductionExemptionReason.PassedInductionInIsleOfManId;

        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.Exempt)
                    .WithExemptionReasons(exemptionReasonId))
                );
        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.ClickEditInductionExemptionReasonPageAsync();

        await page.AssertOnEditInductionExemptionReasonPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionExemptionReasonPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnPersonInductionPageAsync(person.PersonId);
    }
}

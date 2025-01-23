using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class InductionTests : TestBase
{
    public InductionTests(HostFixture hostFixture)
        : base(hostFixture)
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
                    .WithStatus(InductionStatus.RequiredToComplete)
                    .WithStartDate(startDate)
                    .WithCompletedDate(completedDate)));
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
        await page.AssertDateInputEmptyAsync();
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
        var exemptionReasonId = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .Select(e => e.InductionExemptionReasonId)
            .RandomOne();
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
        var exemptionReasonId = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .Select(e => e.InductionExemptionReasonId)
            .RandomOne();
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
    public async Task EditInductionCompletedDate_CYA_ChangeCompletedDate_CYA()
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
        await page.ClickEditInductionCompletedDatePageAsync();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setCompletedDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.SelectChangeReasonAsync(InductionChangeReasonOption.AnotherReason);
        await page.SelectReasonMoreDetailsAsync(false);
        await page.SelectReasonFileUploadAsync(false);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Induction completed on");

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setCompletedDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditInductionStatus_CYA_ChangeSomething_NavigatesBackToCYA()
    {
        var startDate = new DateOnly(2021, 1, 1);
        var completedDate = startDate.AddDays(1);
        var setStartDate = startDate.AddDays(1).AddMonths(1).AddYears(1);
        var setCompletedDate = setStartDate.AddDays(1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.RequiredToComplete)
                    .WithStartDate(startDate)
                    .WithCompletedDate(completedDate)));
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
        await page.AssertDateInputEmptyAsync();
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
    }

    [Fact]
    public async Task EditInductionExemptionReason_NavigateBack()
    {
        var exemptionReasonId = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .Select(e => e.InductionExemptionReasonId)
            .RandomOne();

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

        await page.AssertOnPersonInductionPageAsync(person.PersonId);
    }
}

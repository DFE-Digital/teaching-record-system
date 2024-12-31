namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class InductionTests : TestBase
{
    public InductionTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task EditInduction()
    {
        var setStartDate = new DateOnly(2021, 1, 1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.RequiredToComplete)
                ));
        var personId = person.ContactId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonInductionPageAsync(personId);
        await page.ClickEditInductionStatusPageAsync();

        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);

        await page.SelectStatusAsync(InductionStatus.Failed);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.FillDateInputAsync(setStartDate);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
    }

    [Fact]
    public async Task EditInduction_NavigateBack()
    {
        var inductionStatusToSelect = InductionStatus.Failed;
        var setStartDate = new DateOnly(2021, 1, 1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.RequiredToComplete)
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
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionStartDatePageAsync(person.PersonId);
        await page.ClickBackLink();

        await page.AssertOnEditInductionStatusPageAsync(person.PersonId);
        await page.AssertInductionStatusSelected(inductionStatusToSelect);
    }
}

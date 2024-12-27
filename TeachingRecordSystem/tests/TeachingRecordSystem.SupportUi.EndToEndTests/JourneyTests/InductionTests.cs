namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class InductionTests : TestBase 
{
    public InductionTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task EditInductionStatus()
    {
        var setStartDate = new DateOnly(2021, 1, 1);
        var person = await TestData.CreatePersonAsync(
                personBuilder => personBuilder
                .WithQts()
                .WithInductionStatus(inductionBuilder => inductionBuilder
                    .WithStatus(InductionStatus.InProgress)
                    .WithStartDate(setStartDate)
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
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCompletedDatePageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionChangeReasonPageAsync(person.PersonId);
        await page.ClickContinueButtonAsync();

        await page.AssertOnEditInductionCheckYourAnswersPageAsync(person.PersonId);
    }
}

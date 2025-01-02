namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class PersonTests : TestBase
{
    public PersonTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task EditName()
    {
        var person = await TestData.CreatePersonAsync();
        var personId = person.ContactId;
        var newFirstName = TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = TestData.GenerateChangedMiddleName(person.MiddleName);
        var newLastName = TestData.GenerateChangedLastName(person.LastName);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(personId);

        await page.AssertOnPersonDetailPageAsync(personId);

        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Name");

        await page.AssertOnPersonEditNamePageAsync(personId);

        await page.FillNameInputsAsync(newFirstName, newMiddleName, newLastName);

        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditNameConfirmPageAsync(personId);

        await page.ClickConfirmButtonAsync();

        await page.AssertOnPersonDetailPageAsync(personId);

        await page.AssertFlashMessageAsync("Record has been updated");
    }

    [Fact]
    public async Task EditDateOfBirth()
    {
        var person = await TestData.CreatePersonAsync();
        var personId = person.ContactId;
        var newDateOfBirth = TestData.GenerateChangedDateOfBirth(person.DateOfBirth);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(personId);

        await page.AssertOnPersonDetailPageAsync(personId);

        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Date of birth");

        await page.AssertOnPersonEditDateOfBirthPageAsync(personId);

        await page.FillDateInputAsync(newDateOfBirth);

        await page.ClickContinueButtonAsync();

        await page.AssertOnPersonEditDateOfBirthConfirmPageAsync(personId);

        await page.ClickConfirmButtonAsync();

        await page.AssertOnPersonDetailPageAsync(personId);

        await page.AssertFlashMessageAsync("Record has been updated");
    }
}

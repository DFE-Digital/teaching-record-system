namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public class PersonTests : TestBase
{
    public PersonTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task EditName()
    {
        var person = await TestData.CreatePerson();
        var personId = person.ContactId;
        var newFirstName = TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = TestData.GenerateChangedMiddleName(person.MiddleName);
        var newLastName = TestData.GenerateChangedLastName(person.LastName);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPage(personId);

        await page.AssertOnPersonDetailPage(personId);

        await page.ClickLinkForElementWithTestId("name-change-link");

        await page.AssertOnPersonEditNamePage(personId);

        await page.FillNameInputs(newFirstName, newMiddleName, newLastName);

        await page.ClickContinueButton();

        await page.AssertOnPersonEditNameConfirmPage(personId);

        await page.ClickConfirmButton();

        await page.AssertOnPersonDetailPage(personId);

        await page.AssertFlashMessage("Record has been updated");
    }

    [Fact]
    public async Task EditDateOfBirth()
    {
        var person = await TestData.CreatePerson();
        var personId = person.ContactId;
        var newDateOfBirth = TestData.GenerateChangedDateOfBirth(person.DateOfBirth);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPage(personId);

        await page.AssertOnPersonDetailPage(personId);

        await page.ClickLinkForElementWithTestId("date-of-birth-change-link");

        await page.AssertOnPersonEditDateOfBirthPage(personId);

        await page.FillDateInput(newDateOfBirth);

        await page.ClickContinueButton();

        await page.AssertOnPersonEditDateOfBirthConfirmPage(personId);

        await page.ClickConfirmButton();

        await page.AssertOnPersonDetailPage(personId);

        await page.AssertFlashMessage("Record has been updated");
    }
}

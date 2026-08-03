namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public class ChangeRequestTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SelectChangeRequestAndApprove(bool isNameChange)
    {
        var person = await TestData.CreatePersonAsync();
        string supportTaskReference;
        if (isNameChange)
        {
            var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
                person.PersonId,
                b => b.WithLastName(TestData.GenerateChangedLastName([person.FirstName, person.MiddleName, person.LastName])));
            supportTaskReference = supportTask.SupportTaskReference;
        }
        else
        {
            var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                person.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth!.Value)));
            supportTaskReference = supportTask.SupportTaskReference;
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/change-requests");

        await page.AssertOnChangeRequestsPageAsync();

        await page.ClickAsync($"a{TextIsSelector($"{person.FirstName} {person.MiddleName} {person.LastName}")}");

        await page.AssertOnChangeRequestDetailPageAsync(supportTaskReference);

        await page.ClickAcceptChangeButtonAsync();

        await page.AssertOnAcceptChangeRequestPageAsync(supportTaskReference);

        await page.ClickConfirmButtonAsync();

        await page.AssertOnChangeRequestsPageAsync();

        await page.AssertFlashMessageAsync("The request has been accepted");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SelectChangeRequestAndReject(bool isNameChange)
    {
        var person = await TestData.CreatePersonAsync();
        string supportTaskReference;
        if (isNameChange)
        {
            var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
                person.PersonId,
                b => b.WithLastName(TestData.GenerateChangedLastName([person.FirstName, person.MiddleName, person.LastName])));
            supportTaskReference = supportTask.SupportTaskReference;
        }
        else
        {
            var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                person.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth!.Value)));
            supportTaskReference = supportTask.SupportTaskReference;
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/change-requests");

        await page.AssertOnChangeRequestsPageAsync();

        await page.ClickAsync($"a{TextIsSelector($"{person.FirstName} {person.MiddleName} {person.LastName}")}");

        await page.AssertOnChangeRequestDetailPageAsync(supportTaskReference);

        await page.ClickRejectChangeButtonAsync();

        await page.AssertOnRejectChangeRequestPageAsync(supportTaskReference);

        await page.CheckAsync("label:text-is('Request and proof don’t match')");

        await page.ClickRejectButtonAsync();

        await page.AssertOnChangeRequestsPageAsync();

        await page.AssertFlashMessageAsync("The request has been rejected");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SelectChangeRequestAndCancel(bool isNameChange)
    {
        var person = await TestData.CreatePersonAsync();
        string supportTaskReference;
        if (isNameChange)
        {
            var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
                person.PersonId,
                b => b.WithLastName(TestData.GenerateChangedLastName([person.FirstName, person.MiddleName, person.LastName])));
            supportTaskReference = supportTask.SupportTaskReference;
        }
        else
        {
            var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                person.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth!.Value)));
            supportTaskReference = supportTask.SupportTaskReference;
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/change-requests");

        await page.ClickAsync($"a{TextIsSelector($"{person.FirstName} {person.MiddleName} {person.LastName}")}");

        await page.AssertOnChangeRequestDetailPageAsync(supportTaskReference);

        await page.ClickRejectChangeButtonAsync();

        await page.AssertOnRejectChangeRequestPageAsync(supportTaskReference);

        await page.CheckAsync("label:text-is('Change no longer required')");

        await page.ClickRejectButtonAsync();

        await page.AssertOnChangeRequestsPageAsync();

        await page.AssertFlashMessageAsync("The request has been cancelled");
    }
}

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public class SupportTaskTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task ConnectOneLoginUser_WithSuggestion()
    {
        var person = await TestData.CreatePerson(p => p.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTask(oneLoginUser.Subject);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks");
        await page.ClickAsync($"a:text-is('{supportTask.SupportTaskReference}')");

        await page.WaitForUrlPathAsync($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}");
        await page.ClickAsync($"label:text-is('{person.FirstName} {person.MiddleName} {person.LastName}')");
        await page.ClickContinueButton();

        await page.WaitForUrlPathAsync($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect");
        await page.ClickButton("Connect record");

        await page.WaitForUrlPathAsync("/support-tasks");
    }

    [Fact]
    public async Task ConnectOneLoginUser_WithoutSuggestions()
    {
        var person = await TestData.CreatePerson(p => p.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: null, verifiedInfo: ([TestData.GenerateFirstName(), TestData.GenerateLastName()], TestData.GenerateDateOfBirth()));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTask(oneLoginUser.Subject);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks");
        await page.ClickAsync($"a:text-is('{supportTask.SupportTaskReference}')");

        await page.WaitForUrlPathAsync($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}");
        await page.FillAsync($"label:text-is('Connect to TRN (optional)')", person.Trn!);
        await page.ClickContinueButton();

        await page.WaitForUrlPathAsync($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect");
        await page.ClickButton("Connect record");

        await page.WaitForUrlPathAsync("/support-tasks");
    }
}

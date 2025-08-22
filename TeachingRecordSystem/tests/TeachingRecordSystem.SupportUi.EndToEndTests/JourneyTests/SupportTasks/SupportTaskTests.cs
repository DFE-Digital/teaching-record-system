namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public class SupportTaskTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task ConnectOneLoginUser_WithSuggestion()
    {
        var person = await TestData.CreatePersonAsync(p => p.WithTrn().WithLastName("O'Reilly"));
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks");
        await page.ClickAsync($"a{TextIsSelector(supportTask.SupportTaskReference)}");

        await page.WaitForUrlPathAsync($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}");
        await page.ClickAsync($"label{TextIsSelector($"{person.FirstName} {person.MiddleName} {person.LastName}")}");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect");
        await page.ClickButtonAsync("Connect record");

        await page.WaitForUrlPathAsync("/support-tasks");
    }

    [Test]
    public async Task ConnectOneLoginUser_WithoutSuggestions()
    {
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([TestData.GenerateFirstName(), TestData.GenerateLastName()], TestData.GenerateDateOfBirth()));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks");
        await page.ClickAsync($"a{TextIsSelector(supportTask.SupportTaskReference)}");

        await page.WaitForUrlPathAsync($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}");
        await page.FillAsync($"label:text-is('Connect to TRN (optional)')", person.Trn!);
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect");
        await page.ClickButtonAsync("Connect record");

        await page.WaitForUrlPathAsync("/support-tasks");
    }
}

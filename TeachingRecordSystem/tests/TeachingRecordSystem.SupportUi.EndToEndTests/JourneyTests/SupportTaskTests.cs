namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class SupportTaskTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task ConnectOneLoginUser_WithSuggestion()
    {
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks");
        await page.ClickAsync($"a:text-is('{supportTask.SupportTaskReference.Replace("'", "\\'")}')");

        await page.WaitForUrlPathAsync($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference.Replace("'", "\\'")}");
        await page.ClickAsync($"label:text-is('{person.FirstName.Replace("'", "\\'")} {person.MiddleName.Replace("'", "\\'")} {person.LastName.Replace("'", "\\'")}')");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect");
        await page.ClickButtonAsync("Connect record");

        await page.WaitForUrlPathAsync("/support-tasks");
    }

    [Fact]
    public async Task ConnectOneLoginUser_WithoutSuggestions()
    {
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([TestData.GenerateFirstName(), TestData.GenerateLastName()], TestData.GenerateDateOfBirth()));
        var supportTask = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks");
        await page.ClickAsync($"a:text-is('{supportTask.SupportTaskReference.Replace("'", "\\'")}')");

        await page.WaitForUrlPathAsync($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}");
        await page.FillAsync($"label:text-is('Connect to TRN (optional)')", person.Trn!);
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/connect-one-login-user/{supportTask.SupportTaskReference}/connect");
        await page.ClickButtonAsync("Connect record");

        await page.WaitForUrlPathAsync("/support-tasks");
    }
}

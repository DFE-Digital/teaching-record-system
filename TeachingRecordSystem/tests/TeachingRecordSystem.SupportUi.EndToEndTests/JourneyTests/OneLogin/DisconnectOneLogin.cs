using System.Text.RegularExpressions;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.OneLogin;

public class DisconnectOneLogin(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task DisconnectOneLogin_CanAccessPage()
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToDisconnectOneLoginAsync(person.PersonId, oneLogin.Subject);
        await page.AssertOnDisconnectOneLoginIndexPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickContinueButtonAsync();
    }

    [Fact]
    public async Task DisconnectOneLogin_ClickCancelReturnsToPersonRecord()
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToDisconnectOneLoginAsync(person.PersonId, oneLogin.Subject);
        await page.AssertOnDisconnectOneLoginIndexPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickButtonAsync("Cancel and return to record");
        await page.WaitForURLAsync(new Regex(@"/persons/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"));
    }
}

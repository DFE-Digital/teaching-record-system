using System.Text.RegularExpressions;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.OneLogin;

public class DisconnectOneLoginVerified(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task DisconnectOneLoginVerify_CanAccessPage()
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GoToDisconnectOneLoginAsync(person.PersonId, oneLogin.Subject);
        await page.AssertOnDisconnectOneLoginIndexPageAsync(person.PersonId, oneLogin.Subject);
    }

    [Theory]
    [InlineData(DisconnectOneLoginStayVerified.Yes)]
    [InlineData(DisconnectOneLoginStayVerified.No)]
    public async Task DisconnectOneLoginVerify_SelectOption_RedirectsToCheckAnswersPage(DisconnectOneLoginStayVerified stayVerified)
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GoToDisconnectOneLoginAsync(person.PersonId, oneLogin.Subject);
        await page.AssertOnDisconnectOneLoginIndexPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickRadioAsync(nameof(DisconnectOneLoginReason.NewInformation));
        await page.ClickContinueButtonAsync();
        await page.AssertOnDisconnectOneLoginVerifiedPageAsync(person.PersonId, oneLogin.Subject);

        await page.ClickRadioAsync(stayVerified.ToString());
        await page.ClickContinueButtonAsync();
        await page.AssertOnDisconnectOneLoginCheckYourAnswersPageAsync(person.PersonId, oneLogin.Subject);
    }

    [Fact]
    public async Task DisconnectOneLoginVerified_ClickCancelReturnsToPersonRecord()
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GoToDisconnectOneLoginAsync(person.PersonId, oneLogin.Subject);
        await page.AssertOnDisconnectOneLoginIndexPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickRadioAsync(nameof(DisconnectOneLoginReason.NewInformation));
        await page.ClickContinueButtonAsync();
        await page.AssertOnDisconnectOneLoginVerifiedPageAsync(person.PersonId, oneLogin.Subject);

        await page.ClickButtonAsync("Cancel and return to record");
        await page.WaitForURLAsync(new Regex(@"/persons/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"));
    }

}

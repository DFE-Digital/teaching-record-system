using System.Text.RegularExpressions;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public class DisconnectOneLogin(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task DisconnectOneLoginVerify_StayVerified()
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
        await page.ClickRadioAsync(nameof(DisconnectOneLoginStayVerified.Yes));
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectOneLoginCheckYourAnswersPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickButtonAsync("Confirm and disconnect account");
        await page.WaitForURLAsync(new Regex(@"/persons/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"));

        await page.AssertFlashMessageAsync($"GOV.UK One Login disconnected from {person.FirstName} {person.LastName}`s record");
    }

    [Fact]
    public async Task DisconnectOneLoginVerify_DoNotStayVerified()
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
        await page.ClickRadioAsync(nameof(DisconnectOneLoginStayVerified.Yes));
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectOneLoginCheckYourAnswersPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickButtonAsync("Confirm and disconnect account");
        await page.WaitForURLAsync(new Regex(@"/persons/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"));

        await page.AssertFlashMessageAsync($"GOV.UK One Login disconnected from {person.FirstName} {person.LastName}`s record");
    }
}

using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.OneLogins;

public class DisconnectPersonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(DisconnectPersonReason.NewInformation, null)]
    [InlineData(DisconnectPersonReason.AnotherReason, "Test disconnection reason details")]
    public async Task DisconnectPerson_Success(DisconnectPersonReason reason, string? reasonDetail)
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToDisconnectPersonAsync(oneLogin.Subject, person.PersonId);
        await page.AssertOnDisconnectPersonIndexPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickRadioAsync(reason.ToString());

        if (reasonDetail is not null)
        {
            await page.FillAsync("textarea[name='ReasonDetail']", reasonDetail);
        }

        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectPersonVerifiedPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickRadioAsync(nameof(DisconnectPersonStayVerified.Yes));
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectPersonCheckYourAnswersPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickButtonAsync("Confirm and disconnect");
        await page.WaitForUrlPathAsync($"/one-logins/{oneLogin.Subject}");

        await page.AssertFlashMessageAsync($"{person.FirstName} {person.LastName}\u2019s record disconnected from GOV.UK One Login");
    }

    [Fact]
    public async Task DisconnectPerson_NavigateBack()
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToDisconnectPersonAsync(oneLogin.Subject, person.PersonId);
        await page.AssertOnDisconnectPersonIndexPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickRadioAsync(nameof(DisconnectPersonReason.ConnectedIncorrectly));
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectPersonVerifiedPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickRadioAsync(nameof(DisconnectPersonStayVerified.Yes));
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectPersonCheckYourAnswersPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnDisconnectPersonVerifiedPageAsync(oneLogin.Subject, person.PersonId);
        var stayVerifiedValue = await page.IsCheckedAsync($"input[value='{nameof(DisconnectPersonStayVerified.Yes)}']");
        Assert.True(stayVerifiedValue);

        await page.ClickBackLinkAsync();

        await page.AssertOnDisconnectPersonIndexPageAsync(oneLogin.Subject, person.PersonId);
        var reasonValue = await page.IsCheckedAsync($"input[value='{nameof(DisconnectPersonReason.ConnectedIncorrectly)}']");
        Assert.True(reasonValue);
    }
}

using Optional;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public class ConnectOneLoginTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConnectOneLogin_Success(bool isVerified)
    {
        var person = await TestData.CreatePersonAsync();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var oneLoginUser = isVerified
            ? await TestData.CreateOneLoginUserAsync(
                personId: null,
                email: Option.Some<string?>(email),
                verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)))
            : await TestData.CreateOneLoginUserAsync(
                email: Option.Some<string?>(email),
                verified: false);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickButtonAsync("Connect record to GOV.UK One Login");

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}/connect-one-login");
        await page.FillAsync("input[name='EmailAddress']", oneLoginUser.EmailAddress!);
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}/connect-one-login/match");
        await page.ClickButtonAsync("Connect record to GOV.UK One Login");

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}/connect-one-login/reason");
        await page.ClickRadioAsync("SystemCouldNotMatch");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}/connect-one-login/check-answers");

        if (!isVerified)
        {
            await page.CheckAsync("input[name='IdentityConfirmed']");
        }

        await page.ClickButtonAsync("Confirm and connect account");

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}");
        var expectedFlashMessage = $"Record connected to {StringHelper.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName)}’s GOV.UK One Login";
        await page.AssertFlashMessageAsync(expectedHeader: expectedFlashMessage);
    }

    [Fact]
    public async Task ConnectOneLogin_NavigateBack()
    {
        var person = await TestData.CreatePersonAsync();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            email: Option.Some<string?>(email),
            verified: false);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickButtonAsync("Connect record to GOV.UK One Login");

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}/connect-one-login");
        await page.FillAsync("input[name='EmailAddress']", oneLoginUser.EmailAddress!);
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}/connect-one-login/match");
        await page.ClickButtonAsync("Connect record to GOV.UK One Login");

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}/connect-one-login/reason");
        await page.ClickRadioAsync("SystemCouldNotMatch");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}/connect-one-login/check-answers");
        await page.ClickBackLinkAsync();

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}/connect-one-login/reason");
        await page.ClickBackLinkAsync();

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}/connect-one-login/match");
        await page.ClickBackLinkAsync();

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}/connect-one-login");
        var emailValue = await page.InputValueAsync("input[name='EmailAddress']");
        Assert.Equal(oneLoginUser.EmailAddress, emailValue);
    }
}

using Optional;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.OneLogins;

public class ConnectPersonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConnectPerson_Success(bool isVerified)
    {
        var person = await TestData.CreatePersonAsync();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var oneLoginUser = isVerified
            ? await TestData.CreateOneLoginUserAsync(
                personId: null,
                email: Option.Some<string?>(email),
                verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)))
            : await TestData.CreateOneLoginUserAsync(
                personId: null,
                email: Option.Some<string?>(email),
                verifiedInfo: null);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToOneLoginDetailPageAsync(oneLoginUser.Subject);
        await page.ClickButtonAsync("Connect to a record");

        await page.WaitForUrlPathAsync($"/one-logins/{oneLoginUser.Subject}/connect-person");
        await page.FillAsync("input[name='Trn']", person.Trn);
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/one-logins/{oneLoginUser.Subject}/connect-person/match");
        await page.ClickButtonAsync("Connect record to GOV.UK One Login");

        await page.WaitForUrlPathAsync($"/one-logins/{oneLoginUser.Subject}/connect-person/reason");
        await page.ClickRadioAsync("DataLossOrIncompleteInformation");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers");

        if (!isVerified)
        {
            await page.CheckAsync("input[name='IdentityConfirmed']");
        }

        await page.ClickButtonAsync("Confirm and connect");

        await page.WaitForUrlPathAsync($"/persons/{person.PersonId}");
        var expectedFlashMessage = $"Record connected to {StringHelper.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName)}’s GOV.UK One Login";
        await page.AssertFlashMessageAsync(expectedHeader: expectedFlashMessage);
    }

    [Fact]
    public async Task ConnectPerson_NavigateBack()
    {
        var person = await TestData.CreatePersonAsync();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>(email),
            verifiedInfo: null);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToOneLoginDetailPageAsync(oneLoginUser.Subject);
        await page.ClickButtonAsync("Connect to a record");

        await page.WaitForUrlPathAsync($"/one-logins/{oneLoginUser.Subject}/connect-person");
        await page.FillAsync("input[name='Trn']", person.Trn);
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/one-logins/{oneLoginUser.Subject}/connect-person/match");
        await page.ClickButtonAsync("Connect record to GOV.UK One Login");

        await page.WaitForUrlPathAsync($"/one-logins/{oneLoginUser.Subject}/connect-person/reason");
        await page.ClickRadioAsync("DataLossOrIncompleteInformation");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers");
        await page.ClickBackLinkAsync();

        await page.WaitForUrlPathAsync($"/one-logins/{oneLoginUser.Subject}/connect-person/reason");
        await page.ClickBackLinkAsync();

        await page.WaitForUrlPathAsync($"/one-logins/{oneLoginUser.Subject}/connect-person/match");
        await page.ClickBackLinkAsync();

        await page.WaitForUrlPathAsync($"/one-logins/{oneLoginUser.Subject}/connect-person");
        var trnValue = await page.InputValueAsync("input[name='Trn']");
        Assert.Equal(person.Trn, trnValue);
    }
}

using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.EndToEndTests.SupportUiJourneys.Persons;

public class DisconnectOneLoginTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(DisconnectOneLoginStayVerified.Yes)]
    [InlineData(DisconnectOneLoginStayVerified.No)]
    public async Task DisconnectOneLogin_FromPersonDetailPage_Success(DisconnectOneLoginStayVerified stayVerified)
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickDisconnectOneLoginLinkAsync(oneLogin.EmailAddress!);

        await page.AssertOnDisconnectOneLoginIndexPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickRadioAsync(nameof(DisconnectOneLoginReason.NewInformation));
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectOneLoginVerifiedPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickRadioAsync(stayVerified.ToString());
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectOneLoginCheckYourAnswersPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickGovUkButtonAsync("Confirm and disconnect");

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
        await page.AssertFlashMessageAsync($"GOV.UK One Login disconnected from {person.FirstName} {person.LastName}’s record");

        await WithDbContextAsync(async dbContext =>
        {
            var updated = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLogin.Subject);
            Assert.Null(updated.PersonId);
            Assert.Equal(stayVerified == DisconnectOneLoginStayVerified.Yes, updated.VerifiedOn is not null);
        });
    }

    [Fact]
    public async Task DisconnectOneLogin_Cancel_ReturnsToPersonDetailPage()
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickDisconnectOneLoginLinkAsync(oneLogin.EmailAddress!);

        await page.AssertOnDisconnectOneLoginIndexPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickRadioAsync(nameof(DisconnectOneLoginReason.NewInformation));
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectOneLoginVerifiedPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickGovUkButtonAsync("Cancel");

        await page.AssertOnPersonDetailPageAsync(person.PersonId);

        await WithDbContextAsync(async dbContext =>
        {
            var updated = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLogin.Subject);
            Assert.Equal(person.PersonId, updated.PersonId);
        });
    }

    [Fact]
    public async Task DisconnectOneLogin_NavigateBack()
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonDetailPageAsync(person.PersonId);
        await page.ClickDisconnectOneLoginLinkAsync(oneLogin.EmailAddress!);

        await page.AssertOnDisconnectOneLoginIndexPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickRadioAsync(nameof(DisconnectOneLoginReason.ConnectedIncorrectly));
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectOneLoginVerifiedPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickRadioAsync(nameof(DisconnectOneLoginStayVerified.Yes));
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectOneLoginCheckYourAnswersPageAsync(person.PersonId, oneLogin.Subject);
        await page.ClickBackLinkAsync();

        await page.AssertOnDisconnectOneLoginVerifiedPageAsync(person.PersonId, oneLogin.Subject);
        Assert.True(await page.IsCheckedAsync($"input[value='{nameof(DisconnectOneLoginStayVerified.Yes)}']"));

        await page.ClickBackLinkAsync();

        await page.AssertOnDisconnectOneLoginIndexPageAsync(person.PersonId, oneLogin.Subject);
        Assert.True(await page.IsCheckedAsync($"input[value='{nameof(DisconnectOneLoginReason.ConnectedIncorrectly)}']"));

        await page.ClickBackLinkAsync();

        await page.AssertOnPersonDetailPageAsync(person.PersonId);
    }
}

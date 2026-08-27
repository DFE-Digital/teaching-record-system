using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.EndToEndTests.SupportUiJourneys.OneLogins;

public class DisconnectPersonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(DisconnectPersonReason.NewInformation, null, DisconnectPersonStayVerified.Yes)]
    [InlineData(DisconnectPersonReason.AnotherReason, "Test disconnection reason details", DisconnectPersonStayVerified.No)]
    public async Task DisconnectPerson_FromOneLoginDetailPage_Success(
        DisconnectPersonReason reason,
        string? reasonDetail,
        DisconnectPersonStayVerified stayVerified)
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToOneLoginDetailPageAsync(oneLogin.Subject);
        await page.ClickDisconnectRecordButtonAsync();

        await page.AssertOnDisconnectPersonIndexPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickRadioAsync(reason.ToString());

        if (reasonDetail is not null)
        {
            await page.FillAsync("textarea[name='ReasonDetail']", reasonDetail);
        }

        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectPersonVerifiedPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickRadioAsync(stayVerified.ToString());
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectPersonCheckYourAnswersPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickGovUkButtonAsync("Confirm and disconnect");

        await page.AssertOnOneLoginDetailPageAsync(oneLogin.Subject);
        await page.AssertFlashMessageAsync($"{person.FirstName} {person.LastName}’s record disconnected from GOV.UK One Login");

        await WithDbContextAsync(async dbContext =>
        {
            var updated = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLogin.Subject);
            Assert.Null(updated.PersonId);
            Assert.Equal(stayVerified == DisconnectPersonStayVerified.Yes, updated.VerifiedOn is not null);
        });
    }

    [Fact]
    public async Task DisconnectPerson_Cancel_ReturnsToOneLoginDetailPage()
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToOneLoginDetailPageAsync(oneLogin.Subject);
        await page.ClickDisconnectRecordButtonAsync();

        await page.AssertOnDisconnectPersonIndexPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickRadioAsync(nameof(DisconnectPersonReason.NewInformation));
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectPersonVerifiedPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickGovUkButtonAsync("Cancel");

        await page.AssertOnOneLoginDetailPageAsync(oneLogin.Subject);

        await WithDbContextAsync(async dbContext =>
        {
            var updated = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLogin.Subject);
            Assert.Equal(person.PersonId, updated.PersonId);
        });
    }

    [Fact]
    public async Task DisconnectPerson_NavigateBack()
    {
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToOneLoginDetailPageAsync(oneLogin.Subject);
        await page.ClickDisconnectRecordButtonAsync();

        await page.AssertOnDisconnectPersonIndexPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickRadioAsync(nameof(DisconnectPersonReason.ConnectedIncorrectly));
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectPersonVerifiedPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickRadioAsync(nameof(DisconnectPersonStayVerified.Yes));
        await page.ClickContinueButtonAsync();

        await page.AssertOnDisconnectPersonCheckYourAnswersPageAsync(oneLogin.Subject, person.PersonId);
        await page.ClickBackLinkAsync();

        await page.AssertOnDisconnectPersonVerifiedPageAsync(oneLogin.Subject, person.PersonId);
        Assert.True(await page.IsCheckedAsync($"input[value='{nameof(DisconnectPersonStayVerified.Yes)}']"));

        await page.ClickBackLinkAsync();

        await page.AssertOnDisconnectPersonIndexPageAsync(oneLogin.Subject, person.PersonId);
        Assert.True(await page.IsCheckedAsync($"input[value='{nameof(DisconnectPersonReason.ConnectedIncorrectly)}']"));

        await page.ClickBackLinkAsync();

        await page.AssertOnOneLoginDetailPageAsync(oneLogin.Subject);
    }
}

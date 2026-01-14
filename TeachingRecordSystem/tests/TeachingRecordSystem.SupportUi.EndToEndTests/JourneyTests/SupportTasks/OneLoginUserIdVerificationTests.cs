namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public class OneLoginUserIdVerificationTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task VerifyAndMatch()
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-id-verification");

        await page.ClickAsync($"a{TextIsSelector(supportTask.SupportTaskReference)}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve");

        await page.ClickRadioByLabelAsync("Yes, find a matching record");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches");
        await page.ClickRadioByLabelAsync("Connect it to Record A");
        await page.ClickContinueButtonAsync();

        await page.PauseAsync();
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-connect");
        await page.ClickButtonAsync("Confirm and connect account");

        await page.WaitForUrlPathAsync("/support-tasks/one-login-user-id-verification");
    }

    [Fact]
    public async Task VerifyAndNoMatch()
    {
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-id-verification");

        await page.ClickAsync($"a{TextIsSelector(supportTask.SupportTaskReference)}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve");

        await page.ClickRadioByLabelAsync("Yes, find a matching record");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/no-matches");
        await page.ClickButtonAsync("Send email");

        await page.WaitForUrlPathAsync("/support-tasks/one-login-user-id-verification");
    }

    [Fact]
    public async Task VerifyAndNotConnecting()
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-id-verification");

        await page.ClickAsync($"a{TextIsSelector(supportTask.SupportTaskReference)}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve");

        await page.ClickRadioByLabelAsync("Yes, find a matching record");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches");
        await page.ClickRadioByLabelAsync("Do not connect it to a record");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/not-connecting");
        await page.ClickRadioByLabelAsync("There is no matching record");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting");
        await page.ClickButtonAsync("Confirm and continue");

        await page.WaitForUrlPathAsync("/support-tasks/one-login-user-id-verification");
    }

    [Fact]
    public async Task Reject()
    {
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-id-verification");

        await page.ClickAsync($"a{TextIsSelector(supportTask.SupportTaskReference)}");

        await page.ClickRadioByLabelAsync("No, reject this request");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/reject");
        await page.ClickRadioByLabelAsync("The proof of identity does not match the request details");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-reject");
        await page.ClickButtonAsync("Confirm and reject request");

        await page.WaitForUrlPathAsync("/support-tasks/one-login-user-id-verification");
    }

    [Fact]
    public async Task StartVerifyAndComeBackLater()
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-id-verification");

        await page.ClickAsync($"a{TextIsSelector(supportTask.SupportTaskReference)}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve");

        await page.ClickRadioByLabelAsync("Yes, find a matching record");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches");
        await page.ClickRadioByLabelAsync("Connect it to Record A");
        await page.ClickButtonAsync("Save and come back later");


        // Re-start the journey and check the saved values were persisted

        await page.ClickAsync($"a{TextIsSelector(supportTask.SupportTaskReference)}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve");

        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-connect");
        await page.ClickButtonAsync("Confirm and connect account");

        await page.WaitForUrlPathAsync("/support-tasks/one-login-user-id-verification");
    }
}

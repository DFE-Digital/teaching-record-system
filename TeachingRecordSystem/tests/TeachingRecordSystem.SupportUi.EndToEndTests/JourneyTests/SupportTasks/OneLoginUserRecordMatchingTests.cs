using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public class OneLoginUserRecordMatchingTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Match()
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber)
                .WithStatedTrn(matchedPerson.Trn));
        var taskData = supportTask.GetData<OneLoginUserRecordMatchingData>();
        var firstName = taskData.VerifiedNames![0][0];
        var lastName = taskData.VerifiedNames![0][1];

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($"a{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches");

        await page.ClickRadioByLabelAsync("Connect it to Record A");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect");
        await page.ClickButtonAsync("Confirm and connect account");

        await page.WaitForUrlPathAsync("/support-tasks/one-login-user-matching/record-matching");
    }

    [Fact]
    public async Task NoMatch()
    {
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject);
        var taskData = supportTask.GetData<OneLoginUserRecordMatchingData>();
        var firstName = taskData.VerifiedNames![0][0];
        var lastName = taskData.VerifiedNames![0][1];

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($"a{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches");

        await page.ClickButtonAsync("Send email");

        await page.WaitForUrlPathAsync("/support-tasks/one-login-user-matching/record-matching");
    }

    [Fact]
    public async Task NotConnecting()
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber)
                .WithStatedTrn(matchedPerson.Trn));
        var taskData = supportTask.GetData<OneLoginUserRecordMatchingData>();
        var firstName = taskData.VerifiedNames![0][0];
        var lastName = taskData.VerifiedNames![0][1];

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($"a{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches");

        await page.ClickRadioByLabelAsync("Do not connect it to a record");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting");
        await page.ClickRadioByLabelAsync("There is no matching record");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting");
        await page.ClickButtonAsync("Confirm and continue");

        await page.WaitForUrlPathAsync("/support-tasks/one-login-user-matching/record-matching");
    }

    [Fact]
    public async Task StartMatchAndComeBackLater()
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber)
                .WithStatedTrn(matchedPerson.Trn));
        var taskData = supportTask.GetData<OneLoginUserRecordMatchingData>();
        var firstName = taskData.VerifiedNames![0][0];
        var lastName = taskData.VerifiedNames![0][1];

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($"a{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches");

        await page.ClickRadioByLabelAsync("Connect it to Record A");
        await page.ClickButtonAsync("Save and come back later");


        // Re-start the journey and check the saved values were persisted

        await page.ClickAsync($"a{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches");

        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect");
        await page.ClickButtonAsync("Confirm and connect account");

        await page.WaitForUrlPathAsync("/support-tasks/one-login-user-matching/record-matching");
    }
}

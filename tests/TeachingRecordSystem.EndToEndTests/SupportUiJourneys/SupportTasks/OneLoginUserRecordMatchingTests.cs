using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.EndToEndTests.SupportUiJourneys.SupportTasks;

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
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth!.Value)
                .WithStatedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber)
                .WithStatedTrn(matchedPerson.Trn));
        var taskData = supportTask.GetData<OneLoginUserRecordMatchingData>();
        var firstName = taskData.VerifiedNames![0][0];
        var lastName = taskData.VerifiedNames![0][1];

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($".trs-task-link__name{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches");

        await page.ClickRadioByLabelAsync("Connect it to Record A", exact: false);
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect");
        await page.ClickGovUkButtonAsync("Confirm and connect account");

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

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($".trs-task-link__name{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches");

        await page.ClickGovUkButtonAsync("Confirm");

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
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth!.Value)
                .WithStatedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber)
                .WithStatedTrn(matchedPerson.Trn));
        var taskData = supportTask.GetData<OneLoginUserRecordMatchingData>();
        var firstName = taskData.VerifiedNames![0][0];
        var lastName = taskData.VerifiedNames![0][1];

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($".trs-task-link__name{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches");

        await page.ClickRadioByLabelAsync("Do not connect it to a record");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting");
        await page.ClickRadioByLabelAsync("There is no matching record");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting");
        await page.ClickGovUkButtonAsync("Confirm and continue");

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
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth!.Value)
                .WithStatedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber)
                .WithStatedTrn(matchedPerson.Trn));
        var taskData = supportTask.GetData<OneLoginUserRecordMatchingData>();
        var firstName = taskData.VerifiedNames![0][0];
        var lastName = taskData.VerifiedNames![0][1];

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($".trs-task-link__name{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches");

        await page.ClickRadioByLabelAsync("Connect it to Record A", exact: false);
        await page.ClickGovUkButtonAsync("Save and come back later");

        // With SupportTaskDashboard feature flag enabled, should redirect to the Manage Task page
        await page.WaitForUrlPathAsync($"/support-tasks/{supportTask.SupportTaskReference}");

        // Navigate back to the list and re-start the journey to check the saved values were persisted
        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($".trs-task-link__name{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches");

        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect");
        await page.ClickGovUkButtonAsync("Confirm and connect account");

        await page.WaitForUrlPathAsync("/support-tasks/one-login-user-matching/record-matching");
    }
}

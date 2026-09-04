using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public class OneLoginUserRecordMatchingTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Match_WithQtsDetails()
    {
        var routeTypes = await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync();
        var primaryAndSecondaryPostgraduateFeeFunded = routeTypes.Single(r => r.Name == "Primary and secondary postgraduate fee funded");
        var providerLedPostgrad = routeTypes.Single(r => r.Name == "Provider led Postgrad");
        var trainingProvider = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync()).SingleRandom();
        var subject = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync()).SingleRandom();
        var country = (await TestData.ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var degreeType = (await TestData.ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var currentYear = TestData.TimeProvider.UtcNow.Year;

        var matchedPerson = await TestData.CreatePersonAsync(p => p
            .WithNationalInsuranceNumber()
            .WithEmailAddress()
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(RouteToProfessionalStatusType.QtlsAndSetMembershipId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(new DateOnly(currentYear - 1, 1, 1))
                .WithTrainingSubjectIds([subject.TrainingSubjectId]))
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(primaryAndSecondaryPostgraduateFeeFunded.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(new DateOnly(currentYear, 1, 1))
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithTrainingSubjectIds([subject.TrainingSubjectId])
                .WithTrainingCountryId(country.CountryId)
                .WithDegreeTypeId(degreeType.DegreeTypeId))
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(providerLedPostgrad.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(new DateOnly(currentYear - 2, 1, 1))
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithTrainingSubjectIds([subject.TrainingSubjectId])
                .WithTrainingCountryId(country.CountryId)
                .WithDegreeTypeId(degreeType.DegreeTypeId)));
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth!.Value)
                .WithStatedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber)
                .WithStatedTrn(matchedPerson.Trn)
                .WithYearQtsReceived(currentYear.ToString())
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithSubjectId(subject.TrainingSubjectId));
        var taskData = supportTask.GetData<OneLoginUserRecordMatchingData>();
        var firstName = taskData.VerifiedNames![0][0];
        var lastName = taskData.VerifiedNames![0][1];

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($".trs-task-link__name{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches");

        var matchCardText = await page.Locator("[data-testid='match']").First.TextContentAsync();

        Assert.NotNull(matchCardText);
        Assert.Contains("QTS - QTLS and SET Membership", matchCardText);
        Assert.Contains("QTS - Primary and secondary postgraduate fee funded", matchCardText);
        Assert.Contains("QTS - Provider led Postgrad", matchCardText);
        Assert.Contains((currentYear - 1).ToString(), matchCardText);
        Assert.Contains((currentYear - 2).ToString(), matchCardText);
        Assert.Contains(currentYear.ToString(), matchCardText);
        Assert.Contains(trainingProvider.Name, matchCardText);
        Assert.Contains(subject.Name, matchCardText);

        await page.ClickRadioByLabelAsync("Connect it to Record A", exact: false);
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect");
        await page.ClickButtonAsync("Confirm and connect account");

        await page.WaitForUrlPathAsync("/support-tasks/one-login-user-matching/record-matching");
    }

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

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($".trs-task-link__name{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches");

        await page.ClickRadioByLabelAsync("Connect it to Record A", exact: false);
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

        await page.ClickAsync($".trs-task-link__name{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches");

        await page.ClickButtonAsync("Confirm");

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

        await using var context = await HostFixture.CreateBrowserContext();
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
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth!.Value)
                .WithStatedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber)
                .WithStatedTrn(matchedPerson.Trn));
        var taskData = supportTask.GetData<OneLoginUserRecordMatchingData>();
        var firstName = taskData.VerifiedNames![0][0];
        var lastName = taskData.VerifiedNames![0][1];

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($".trs-task-link__name{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches");

        await page.ClickRadioByLabelAsync("Connect it to Record A", exact: false);
        await page.ClickButtonAsync("Save and come back later");

        // With SupportTaskDashboard feature flag enabled, should redirect to the Manage Task page
        await page.WaitForUrlPathAsync($"/support-tasks/{supportTask.SupportTaskReference}");

        // Navigate back to the list and re-start the journey to check the saved values were persisted
        await page.GotoAsync("/support-tasks/one-login-user-matching/record-matching");

        await page.ClickAsync($".trs-task-link__name{TextIsSelector($"{firstName} {lastName}")}");
        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches");

        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect");
        await page.ClickButtonAsync("Confirm and connect account");

        await page.WaitForUrlPathAsync("/support-tasks/one-login-user-matching/record-matching");
    }
}

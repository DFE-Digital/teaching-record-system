namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public class ApiTrnRequestTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Resolve_CreateNewRecord()
    {
        // Start with a blank slate of tasks
        await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, requestData, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, t => t.WithStatus(SupportTaskStatus.Open));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/api-trn-requests");

        await page.ClickAsync($"a{TextIsSelector($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}")}");

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches");

        await page.CheckAsync($"label{TextIsSelector("Create a new record from it")}");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers");
        await page.ClickButtonAsync("Confirm and create record");

        await page.WaitForUrlPathAsync("/support-tasks/api-trn-requests");

        await page.AssertBannerLinksToPersonRecord();
    }

    [Fact]
    public async Task Resolve_MultiplePotentialMatches_MergeWithExistingRecord()
    {
        // Start with a blank slate of tasks
        await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();
        var emailAddress = TestData.GenerateUniqueEmail();

        var matchedPerson1 = await TestData.CreatePersonAsync(p =>
        {
            p.WithFirstName(firstName);
            p.WithMiddleName(middleName);
            p.WithLastName(lastName);
            p.WithDateOfBirth(dateOfBirth);
            p.WithEmailAddress(emailAddress);
        });

        var matchedPerson2 = await TestData.CreatePersonAsync(p =>
        {
            p.WithFirstName(firstName);
            p.WithMiddleName(TestData.GenerateChangedMiddleName(middleName));
            p.WithLastName(lastName);
            p.WithDateOfBirth(dateOfBirth);
        });

        var applicationUser = await TestData.CreateApplicationUserAsync();

        // Set up two potential matched records to merge with
        var (supportTask, requestData, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithMatchedPersons(matchedPerson1.PersonId, matchedPerson2.PersonId)
                .WithStatus(SupportTaskStatus.Open)
                .WithFirstName(firstName)
                .WithMiddleName(TestData.GenerateChangedMiddleName(middleName))
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithGender(matchedPerson1.Gender)
                .WithEmailAddress(TestData.GenerateUniqueEmail())
            );

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/api-trn-requests");

        await page.ClickAsync($"a{TextIsSelector($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}")}");

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches");

        await page.CheckAsync("label:text-is('Merge it with Record A')");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/merge");
        await page.CheckAsync($"label{HasTextSelector(requestData.MiddleName)}");
        await page.CheckAsync($"label{HasTextSelector(matchedPerson1.EmailAddress)}");
        await page.CheckAsync($"label{HasTextSelector(requestData.NationalInsuranceNumber)}");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers");
        await page.ClickButtonAsync("Confirm and update existing record");

        await page.WaitForUrlPathAsync("/support-tasks/api-trn-requests");

        await page.AssertBannerLinksToPersonRecord(matchedPerson1.PersonId);
    }

    [Fact]
    public async Task Resolve_NoLongerAnyPotentialMatches_CreateNewRecord()
    {
        // Start with a blank slate of tasks
        await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();
        var emailAddress = TestData.GenerateUniqueEmail();

        var matchedPerson1 = await TestData.CreatePersonAsync(p =>
        {
            p.WithFirstName(firstName);
            p.WithMiddleName(middleName);
            p.WithLastName(lastName);
            p.WithDateOfBirth(dateOfBirth);
            p.WithEmailAddress(emailAddress);
        });

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, requestData, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithMatchedPersons(matchedPerson1.PersonId)
                .WithStatus(SupportTaskStatus.Open)
                .WithFirstName(TestData.GenerateChangedFirstName(firstName))
                .WithMiddleName(TestData.GenerateChangedMiddleName(middleName))
                .WithLastName(TestData.GenerateChangedLastName(lastName))
                .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(dateOfBirth))
                .WithEmailAddress(TestData.GenerateUniqueEmail())
            );

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/api-trn-requests");

        await page.ClickAsync($"a{TextIsSelector($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}")}");

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches");
        await page.ClickButtonAsync("Create a record from this request");

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers");
        await page.ClickButtonAsync("Confirm and create record");

        await page.WaitForUrlPathAsync("/support-tasks/api-trn-requests");

        await page.AssertBannerLinksToPersonRecord();
    }

    [Fact]
    public async Task Resolve_DefiniteMatchNowFound_MergeWithExistingRecord()
    {
        // Start with a blank slate of tasks
        await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();
        var emailAddress = TestData.GenerateUniqueEmail();
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();

        var matchedPerson = await TestData.CreatePersonAsync(p =>
        {
            p.WithFirstName(firstName);
            p.WithMiddleName(middleName);
            p.WithLastName(lastName);
            p.WithDateOfBirth(dateOfBirth);
            p.WithEmailAddress(emailAddress);
            p.WithNationalInsuranceNumber(nationalInsuranceNumber);
        });

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, requestData, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithMatchedPersons(matchedPerson.PersonId)
                .WithStatus(SupportTaskStatus.Open)
                .WithFirstName(firstName)
                .WithMiddleName(middleName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithGender(matchedPerson.Gender)
                .WithEmailAddress(emailAddress)
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
            );

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/api-trn-requests");

        await page.ClickAsync($"a{TextIsSelector($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}")}");

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches");
        await page.ClickButtonAsync("Merge this request with Record A");

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/merge");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers");
        await page.ClickButtonAsync("Confirm and update existing record");

        await page.WaitForUrlPathAsync("/support-tasks/api-trn-requests");

        await page.AssertBannerLinksToPersonRecord(matchedPerson.PersonId);
    }
}


using System.Globalization;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public class TeacherPensionsSupportTasks(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task KeepAsSeparateRecord()
    {
        var fileName = "SomeFileName.txt";
        long integrationTransactionId = 1;
        var now = DateTime.UtcNow;

        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
        var duplicatePerson2 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/teacher-pensions?_f=1");
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId, duplicatePerson2.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData(fileName, integrationTransactionId);
                s.WithCreatedOn(now);
            });

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GoToTeacherPensionsSupportTasks();
        await page.AssertOnTeachersPensionsSupportTasksPageAsync();

        await page.ClickAsync($"a{TextIsSelector($"{person.FirstName} {person.MiddleName} {person.LastName}")}");
        await page.AssertOnTeachersPensionsSupportTaskMatchesPageAsync(supportTask.SupportTaskReference);

        await page.CheckAsync("label:text-is('Keep it as a separate record')");
        await page.ClickContinueButtonAsync();
        await page.AssertOnTeachersPensionsSupportTaskKeepSeparatePageAsync(supportTask.SupportTaskReference);

        await page.CheckAsync("label:text-is('The records do not match')");
        await page.ClickContinueButtonAsync();
        await page.AssertOnTeachersPensionsSupportTaskConfirmKeepSeparatePageAsync(supportTask.SupportTaskReference);

        await page.ClickContinueButtonAsync();
        await page.AssertOnTeachersPensionsSupportTasksPageAsync();
        await page.AssertFlashMessageAsync("Teachers’ Pensions duplicate task completed", "The records were not merged.");
    }

    [Fact]
    public async Task CancelConfirmKeepAsSeparateRecord()
    {
        var fileName = "SomeFileName.txt";
        long integrationTransactionId = 1;
        var now = DateTime.UtcNow;

        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
        var duplicatePerson2 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/teacher-pensions?_f=1");
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId, duplicatePerson2.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData(fileName, integrationTransactionId);
                s.WithCreatedOn(now);
            });

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GoToTeacherPensionsSupportTasks();
        await page.AssertOnTeachersPensionsSupportTasksPageAsync();

        await page.ClickAsync($"a{TextIsSelector($"{person.FirstName} {person.MiddleName} {person.LastName}")}");
        await page.AssertOnTeachersPensionsSupportTaskMatchesPageAsync(supportTask.SupportTaskReference);

        await page.CheckAsync("label:text-is('Keep it as a separate record')");
        await page.ClickContinueButtonAsync();
        await page.AssertOnTeachersPensionsSupportTaskKeepSeparatePageAsync(supportTask.SupportTaskReference);

        await page.CheckAsync("label:text-is('The records do not match')");
        await page.ClickContinueButtonAsync();
        await page.AssertOnTeachersPensionsSupportTaskConfirmKeepSeparatePageAsync(supportTask.SupportTaskReference);

        await page.ClickButtonAsync("Cancel");
        await page.AssertOnTeachersPensionsSupportTasksPageAsync();
    }

    [Fact]
    public async Task MergeRecord()
    {
        var fileName = "SomeFileName.txt";
        long integrationTransactionId = 1;
        var now = DateTime.UtcNow;

        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/teacher-pensions?_f=1");
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData(fileName, integrationTransactionId);
                s.WithCreatedOn(now);
            });

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GoToTeacherPensionsSupportTasks();
        await page.AssertOnTeachersPensionsSupportTasksPageAsync();

        await page.ClickAsync($"a{TextIsSelector($"{person.FirstName} {person.MiddleName} {person.LastName}")}");
        await page.AssertOnTeachersPensionsSupportTaskMatchesPageAsync(supportTask.SupportTaskReference);

        await page.CheckAsync("label:text-is('Merge it with Record A')");
        await page.ClickContinueButtonAsync();
        await page.AssertOnTeachersPensionsSupportTaskMergePageAsync(supportTask.SupportTaskReference);

        await page.ClickRadioByLabelAsync($"{duplicatePerson1.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat, CultureInfo.InvariantCulture)}");
        await page.CheckAsync("label:text-is('No')"); //no evidence
        await page.ClickContinueButtonAsync();
        await page.AssertOnTeachersPensionsSupportTaskResolveCheckAnswersPageAsync(supportTask.SupportTaskReference);

        await page.ClickButtonAsync("Confirm and merge records");
        await page.AssertOnTeachersPensionsSupportTasksPageAsync();
        await page.AssertFlashMessageAsync("Teachers’ Pensions duplicate task completed");
    }

    [Fact]
    public async Task CancelCheckAnswersMergeRecord()
    {
        var fileName = "SomeFileName.txt";
        long integrationTransactionId = 1;
        var now = DateTime.UtcNow;

        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/teacher-pensions?_f=1");
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData(fileName, integrationTransactionId);
                s.WithCreatedOn(now);
            });

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GoToTeacherPensionsSupportTasks();
        await page.AssertOnTeachersPensionsSupportTasksPageAsync();

        await page.ClickAsync($"a{TextIsSelector($"{person.FirstName} {person.MiddleName} {person.LastName}")}");
        await page.AssertOnTeachersPensionsSupportTaskMatchesPageAsync(supportTask.SupportTaskReference);

        await page.CheckAsync("label:text-is('Merge it with Record A')");
        await page.ClickContinueButtonAsync();
        await page.AssertOnTeachersPensionsSupportTaskMergePageAsync(supportTask.SupportTaskReference);

        await page.ClickRadioByLabelAsync($"{duplicatePerson1.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat, CultureInfo.InvariantCulture)}");
        await page.CheckAsync("label:text-is('No')"); //no evidence
        await page.ClickContinueButtonAsync();
        await page.AssertOnTeachersPensionsSupportTaskResolveCheckAnswersPageAsync(supportTask.SupportTaskReference);

        await page.ClickButtonAsync("Cancel");
        await page.AssertOnTeachersPensionsSupportTasksPageAsync();
    }
}

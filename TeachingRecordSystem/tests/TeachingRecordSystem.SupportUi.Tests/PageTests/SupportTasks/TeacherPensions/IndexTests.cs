using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TeacherPensions;

[Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture), IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return WithDbContext(dbContext => dbContext.SupportTasks.ExecuteDeleteAsync());
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static IElement[] GetResultRows(IHtmlDocument doc) =>
    doc
        .GetElementsByTagName("tbody")
        .Single()
        .GetElementsByClassName("govuk-table__row")
        .ToArray();

    private static string[] GetResultTaskReferences(IHtmlDocument doc) =>
        GetResultRows(doc)
            .Select(row => row.GetAttribute("data-testid")!["task:".Length..])
            .ToArray();

    [Fact]
    public async Task Get_NoPotentialDuplicateTasks_ReturnsNoResults()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs));
        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/teacher-pensions?_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("no-tasks-message"));
    }

    [Fact]
    public async Task Get_WithSupportTask_RendersResults()
    {
        // Arrange
        var fileName = "SomeFileName.txt";
        long integrationTransactionId = 1;

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
                           s.WithMatchedPersons(new[] { duplicatePerson1.PersonId, duplicatePerson2.PersonId });
                           s.WithLastName(person.LastName);
                           s.WithFirstName(person.FirstName);
                           s.WithMiddleName(person.MiddleName);
                           s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                           s.WithGender(person.Gender);
                           s.WithDateOfBirth(person.DateOfBirth);
                           s.WithSupportTaskData(fileName, integrationTransactionId);
                           s.WithCreatedOn(Clock.UtcNow);
                       });

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var row = doc.GetElementByTestId($"task:{supportTask.SupportTaskReference}");
        Assert.NotNull(row);
        var nameContent = row.GetElementByTestId("name");
        var fileNameContent = row.GetElementByTestId("filename");
        var interfaceIdContent = row.GetElementByTestId("integration-transaction-id");
        var createdOnContent = row.GetElementByTestId("created-on");
        Assert.NotNull(nameContent);
        Assert.NotNull(fileNameContent);
        Assert.NotNull(interfaceIdContent);
        Assert.NotNull(createdOnContent);
        Assert.Contains($"{person!.FirstName!} {person!.LastName!}", nameContent.TextContent);
        Assert.Equal(fileName!, fileNameContent.TextContent);
        Assert.Equal(integrationTransactionId.ToString(), interfaceIdContent.TextContent);
        Assert.Equal($"{Clock.UtcNow.ToString("dd MMM yyyy")}", createdOnContent.TextContent);
    }


    [Fact]
    public async Task Get_NoSortParametersSpecified_ShowsTasksOrderedByCreatedOnAscending()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var person2 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var user = await TestData.CreateUserAsync();
        var person1Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person1.FirstName).WithLastName(person1.LastName).WithNationalInsuranceNumber(person1.NationalInsuranceNumber!));
        var person2Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person2.FirstName).WithLastName(person2.LastName).WithNationalInsuranceNumber(person2.NationalInsuranceNumber!));
        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/teacher-pensions?_f=1");
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person1Duplicate.PersonId, });
                           s.WithLastName(person1.LastName);
                           s.WithFirstName(person1.FirstName);
                           s.WithMiddleName(person1.MiddleName);
                           s.WithNationalInsuranceNumber(person1.NationalInsuranceNumber);
                           s.WithGender(person1.Gender);
                           s.WithDateOfBirth(person1.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow);
                       });

        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person2Duplicate.PersonId, });
                           s.WithLastName(person2.LastName);
                           s.WithFirstName(person2.FirstName);
                           s.WithMiddleName(person2.MiddleName);
                           s.WithNationalInsuranceNumber(person2.NationalInsuranceNumber);
                           s.WithGender(person2.Gender);
                           s.WithDateOfBirth(person2.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow.AddDays(-10));
                       });

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
                result1 => Assert.Equal(supportTask2.SupportTaskReference, result1),
                result2 => Assert.Equal(supportTask1.SupportTaskReference, result2));
    }

    [Fact]
    public async Task Get_WithCreatedOnDescending_ShowsTasksOrderedByCreatedOnDescending()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var person2 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var user = await TestData.CreateUserAsync();
        var person1Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person1.FirstName).WithLastName(person1.LastName).WithNationalInsuranceNumber(person1.NationalInsuranceNumber!));
        var person2Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person2.FirstName).WithLastName(person2.LastName).WithNationalInsuranceNumber(person2.NationalInsuranceNumber!));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions?_f=1&sortBy={TeacherPensionsSortOptions.CreatedOn}&sortDirection={SortDirection.Descending}");
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person1Duplicate.PersonId, });
                           s.WithLastName(person1.LastName);
                           s.WithFirstName(person1.FirstName);
                           s.WithMiddleName(person1.MiddleName);
                           s.WithNationalInsuranceNumber(person1.NationalInsuranceNumber);
                           s.WithGender(person1.Gender);
                           s.WithDateOfBirth(person1.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow.AddDays(-10));
                       });

        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person2Duplicate.PersonId, });
                           s.WithLastName(person2.LastName);
                           s.WithFirstName(person2.FirstName);
                           s.WithMiddleName(person2.MiddleName);
                           s.WithNationalInsuranceNumber(person2.NationalInsuranceNumber);
                           s.WithGender(person2.Gender);
                           s.WithDateOfBirth(person2.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow);
                       });

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
                result1 => Assert.Equal(supportTask2.SupportTaskReference, result1),
                result2 => Assert.Equal(supportTask1.SupportTaskReference, result2));
    }

    [Fact]
    public async Task Get_WithNameSortOrder_ShowsTasksOrderedByNameOnAscending()
    {
        // Arrange
        var person1FirstName = "Alan";
        var person2FirstName = "Terry";
        var person1 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithFirstName(person1FirstName));
        var person2 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithFirstName(person2FirstName));
        var user = await TestData.CreateUserAsync();
        var person1Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person1.FirstName).WithLastName(person1.LastName).WithNationalInsuranceNumber(person1.NationalInsuranceNumber!));
        var person2Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person2.FirstName).WithLastName(person2.LastName).WithNationalInsuranceNumber(person2.NationalInsuranceNumber!));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions?_f=1&sortBy={TeacherPensionsSortOptions.Name}&sortDirection={SortDirection.Ascending}");
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person1Duplicate.PersonId, });
                           s.WithLastName(person1.LastName);
                           s.WithFirstName(person1.FirstName);
                           s.WithMiddleName(person1.MiddleName);
                           s.WithNationalInsuranceNumber(person1.NationalInsuranceNumber);
                           s.WithGender(person1.Gender);
                           s.WithDateOfBirth(person1.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow);
                       });

        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person2Duplicate.PersonId, });
                           s.WithLastName(person2.LastName);
                           s.WithFirstName(person2.FirstName);
                           s.WithMiddleName(person2.MiddleName);
                           s.WithNationalInsuranceNumber(person2.NationalInsuranceNumber);
                           s.WithGender(person2.Gender);
                           s.WithDateOfBirth(person2.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow.AddDays(-10));
                       });

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
                result1 => Assert.Equal(supportTask1.SupportTaskReference, result1),
                result2 => Assert.Equal(supportTask2.SupportTaskReference, result2));
    }

    [Fact]
    public async Task Get_WithFilenameSortOrder_ShowsTasksOrderedByFilenameDescending()
    {
        // Arrange
        var person1FirstName = "Alan";
        var person2FirstName = "Terry";
        var user = await TestData.CreateUserAsync();
        var person1 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithFirstName(person1FirstName));
        var person2 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithFirstName(person2FirstName));
        var person1Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person1.FirstName).WithLastName(person1.LastName).WithNationalInsuranceNumber(person1.NationalInsuranceNumber!));
        var person2Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person2.FirstName).WithLastName(person2.LastName).WithNationalInsuranceNumber(person2.NationalInsuranceNumber!));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions?_f=1&sortBy={TeacherPensionsSortOptions.Filename}&sortDirection={SortDirection.Descending}");
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person1Duplicate.PersonId, });
                           s.WithLastName(person1.LastName);
                           s.WithFirstName(person1.FirstName);
                           s.WithMiddleName(person1.MiddleName);
                           s.WithNationalInsuranceNumber(person1.NationalInsuranceNumber);
                           s.WithGender(person1.Gender);
                           s.WithDateOfBirth(person1.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow);
                           s.WithSupportTaskData("zzzzzz.csv", 0);
                       });

        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person2Duplicate.PersonId, });
                           s.WithLastName(person2.LastName);
                           s.WithFirstName(person2.FirstName);
                           s.WithMiddleName(person2.MiddleName);
                           s.WithNationalInsuranceNumber(person2.NationalInsuranceNumber);
                           s.WithGender(person2.Gender);
                           s.WithDateOfBirth(person2.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow.AddDays(-10));
                           s.WithSupportTaskData("aaaaa.txt", 0);
                       });

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
                result1 => Assert.Equal(supportTask1.SupportTaskReference, result1),
                result2 => Assert.Equal(supportTask2.SupportTaskReference, result2));
    }

    [Fact]
    public async Task Get_WithFilenameSortOrder_ShowsTasksOrderedByFilenamAscending()
    {
        // Arrange
        var person1FirstName = "Alan";
        var person2FirstName = "Terry";
        var user = await TestData.CreateUserAsync();
        var person1 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithFirstName(person1FirstName));
        var person2 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithFirstName(person2FirstName));
        var person1Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person1.FirstName).WithLastName(person1.LastName).WithNationalInsuranceNumber(person1.NationalInsuranceNumber!));
        var person2Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person2.FirstName).WithLastName(person2.LastName).WithNationalInsuranceNumber(person2.NationalInsuranceNumber!));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions?_f=1&sortBy={TeacherPensionsSortOptions.Filename}&sortDirection={SortDirection.Ascending}");
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person1Duplicate.PersonId, });
                           s.WithLastName(person1.LastName);
                           s.WithFirstName(person1.FirstName);
                           s.WithMiddleName(person1.MiddleName);
                           s.WithNationalInsuranceNumber(person1.NationalInsuranceNumber);
                           s.WithGender(person1.Gender);
                           s.WithDateOfBirth(person1.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow);
                           s.WithSupportTaskData("zzzzzz.csv", 0);
                       });

        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person2Duplicate.PersonId, });
                           s.WithLastName(person2.LastName);
                           s.WithFirstName(person2.FirstName);
                           s.WithMiddleName(person2.MiddleName);
                           s.WithNationalInsuranceNumber(person2.NationalInsuranceNumber);
                           s.WithGender(person2.Gender);
                           s.WithDateOfBirth(person2.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow.AddDays(-10));
                           s.WithSupportTaskData("aaaaa.txt", 0);
                       });

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
                result1 => Assert.Equal(supportTask2.SupportTaskReference, result1),
                result2 => Assert.Equal(supportTask1.SupportTaskReference, result2));
    }

    [Fact]
    public async Task Get_WithInterfaceIdSortOrder_ShowsTasksOrderedByInterfaceIdDescending()
    {
        // Arrange
        var person1FirstName = "Alan";
        var person2FirstName = "Terry";
        var user = await TestData.CreateUserAsync();
        var person1 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithFirstName(person1FirstName));
        var person2 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithFirstName(person2FirstName));
        var person1Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person1.FirstName).WithLastName(person1.LastName).WithNationalInsuranceNumber(person1.NationalInsuranceNumber!));
        var person2Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person2.FirstName).WithLastName(person2.LastName).WithNationalInsuranceNumber(person2.NationalInsuranceNumber!));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions?_f=1&sortBy={TeacherPensionsSortOptions.InterfaceId}&sortDirection={SortDirection.Descending}");
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person1Duplicate.PersonId, });
                           s.WithLastName(person1.LastName);
                           s.WithFirstName(person1.FirstName);
                           s.WithMiddleName(person1.MiddleName);
                           s.WithNationalInsuranceNumber(person1.NationalInsuranceNumber);
                           s.WithGender(person1.Gender);
                           s.WithDateOfBirth(person1.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow);
                           s.WithSupportTaskData("zzzzzz.csv", 100);
                       });

        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person2Duplicate.PersonId, });
                           s.WithLastName(person2.LastName);
                           s.WithFirstName(person2.FirstName);
                           s.WithMiddleName(person2.MiddleName);
                           s.WithNationalInsuranceNumber(person2.NationalInsuranceNumber);
                           s.WithGender(person2.Gender);
                           s.WithDateOfBirth(person2.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow.AddDays(-10));
                           s.WithSupportTaskData("aaaaa.txt", 2);
                       });

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
                result1 => Assert.Equal(supportTask1.SupportTaskReference, result1),
                result2 => Assert.Equal(supportTask2.SupportTaskReference, result2));
    }

    [Fact]
    public async Task Get_WithInterfaceIdSortOrder_ShowsTasksOrderedByInterfaceIdAscending()
    {
        // Arrange
        var person1FirstName = "Alan";
        var person2FirstName = "Terry";
        var user = await TestData.CreateUserAsync();
        var person1 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithFirstName(person1FirstName));
        var person2 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithFirstName(person2FirstName));
        var person1Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person1.FirstName).WithLastName(person1.LastName).WithNationalInsuranceNumber(person1.NationalInsuranceNumber!));
        var person2Duplicate = await TestData.CreatePersonAsync(x => x.WithFirstName(person2.FirstName).WithLastName(person2.LastName).WithNationalInsuranceNumber(person2.NationalInsuranceNumber!));
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions?_f=1&sortBy={TeacherPensionsSortOptions.InterfaceId}&sortDirection={SortDirection.Ascending}");
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person1Duplicate.PersonId, });
                           s.WithLastName(person1.LastName);
                           s.WithFirstName(person1.FirstName);
                           s.WithMiddleName(person1.MiddleName);
                           s.WithNationalInsuranceNumber(person1.NationalInsuranceNumber);
                           s.WithGender(person1.Gender);
                           s.WithDateOfBirth(person1.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow);
                           s.WithSupportTaskData("zzzzzz.csv", 100);
                       });

        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person1.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { person2Duplicate.PersonId, });
                           s.WithLastName(person2.LastName);
                           s.WithFirstName(person2.FirstName);
                           s.WithMiddleName(person2.MiddleName);
                           s.WithNationalInsuranceNumber(person2.NationalInsuranceNumber);
                           s.WithGender(person2.Gender);
                           s.WithDateOfBirth(person2.DateOfBirth);
                           s.WithCreatedOn(Clock.UtcNow.AddDays(-10));
                           s.WithSupportTaskData("aaaaa.txt", 2);
                       });

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
                result1 => Assert.Equal(supportTask2.SupportTaskReference, result1),
                result2 => Assert.Equal(supportTask1.SupportTaskReference, result2));
    }
}

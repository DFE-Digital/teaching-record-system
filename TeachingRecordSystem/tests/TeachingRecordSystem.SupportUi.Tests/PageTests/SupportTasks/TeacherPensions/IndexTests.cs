using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TeacherPensions;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoPotentialDuplicateTasks_ReturnsNoResults()
    {
        // Arrange
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
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            fileName: "SomeFileName.txt",
            integrationTransactionId: 1,
            createdOn: Clock.UtcNow,
            configurePerson: p => p.WithFirstName("John").WithLastName("Smith").WithNationalInsuranceNumber());

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/teacher-pensions?_f=1");

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
        Assert.Contains("John Smith", nameContent.TextContent);
        Assert.Equal("SomeFileName.txt", fileNameContent.TextContent);
        Assert.Equal("1", interfaceIdContent.TextContent);
        Assert.Equal($"{Clock.UtcNow.ToString("dd MMM yyyy")}", createdOnContent.TextContent);
    }

    [Fact]
    public async Task Get_NoSortParametersSpecified_ShowsTasksOrderedByCreatedOnAscending()
    {
        // Arrange
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: Clock.UtcNow, configurePerson: p => p.WithNationalInsuranceNumber());
        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: Clock.UtcNow.AddDays(-10), configurePerson: p => p.WithNationalInsuranceNumber());

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/teacher-pensions?_f=1");

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
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: Clock.UtcNow.AddDays(-10), configurePerson: p => p.WithNationalInsuranceNumber());
        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: Clock.UtcNow, configurePerson: p => p.WithNationalInsuranceNumber());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions?_f=1&sortBy={TeachersPensionsPotentialDuplicatesSortByOption.CreatedOn}&sortDirection={SortDirection.Descending}");

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
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: Clock.UtcNow, configurePerson: p => p.WithNationalInsuranceNumber().WithFirstName("Alan"));
        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: Clock.UtcNow.AddDays(-10), configurePerson: p => p.WithNationalInsuranceNumber().WithFirstName("Terry"));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions?_f=1&sortBy={TeachersPensionsPotentialDuplicatesSortByOption.Name}&sortDirection={SortDirection.Ascending}");

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
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            fileName: "zzzzzz.csv",
            createdOn: Clock.UtcNow,
            configurePerson: p => p.WithNationalInsuranceNumber().WithFirstName("Alan"));
        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            fileName: "aaaaa.txt",
            createdOn: Clock.UtcNow.AddDays(-10),
            configurePerson: p => p.WithNationalInsuranceNumber().WithFirstName("Terry"));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions?_f=1&sortBy={TeachersPensionsPotentialDuplicatesSortByOption.Filename}&sortDirection={SortDirection.Descending}");

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
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            fileName: "zzzzzz.csv",
            createdOn: Clock.UtcNow,
            configurePerson: p => p.WithNationalInsuranceNumber().WithFirstName("Alan"));
        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            fileName: "aaaaa.txt",
            createdOn: Clock.UtcNow.AddDays(-10),
            configurePerson: p => p.WithNationalInsuranceNumber().WithFirstName("Terry"));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions?_f=1&sortBy={TeachersPensionsPotentialDuplicatesSortByOption.Filename}&sortDirection={SortDirection.Ascending}");

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
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            fileName: "zzzzzz.csv",
            integrationTransactionId: 100,
            createdOn: Clock.UtcNow,
            configurePerson: p => p.WithNationalInsuranceNumber().WithFirstName("Alan"));
        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            fileName: "aaaaa.txt",
            integrationTransactionId: 2,
            createdOn: Clock.UtcNow.AddDays(-10),
            configurePerson: p => p.WithNationalInsuranceNumber().WithFirstName("Terry"));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions?_f=1&sortBy={TeachersPensionsPotentialDuplicatesSortByOption.InterfaceId}&sortDirection={SortDirection.Descending}");

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
        var supportTask1 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            fileName: "zzzzzz.csv",
            integrationTransactionId: 100,
            createdOn: Clock.UtcNow,
            configurePerson: p => p.WithNationalInsuranceNumber().WithFirstName("Alan"));
        var supportTask2 = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            fileName: "aaaaa.txt",
            integrationTransactionId: 2,
            createdOn: Clock.UtcNow.AddDays(-10),
            configurePerson: p => p.WithNationalInsuranceNumber().WithFirstName("Terry"));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions?_f=1&sortBy={TeachersPensionsPotentialDuplicatesSortByOption.InterfaceId}&sortDirection={SortDirection.Ascending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Collection(
            GetResultTaskReferences(doc),
                result1 => Assert.Equal(supportTask2.SupportTaskReference, result1),
                result2 => Assert.Equal(supportTask1.SupportTaskReference, result2));
    }

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
}

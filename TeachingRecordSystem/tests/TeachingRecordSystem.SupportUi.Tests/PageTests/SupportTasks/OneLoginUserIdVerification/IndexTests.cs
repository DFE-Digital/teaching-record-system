using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private OneLoginUser[]? OneLoginUsers { get; set; }
    private SupportTask[]? SupportTasks { get; set; }

    private async Task CreateSupportTasksWithOneLoginUsersAsync()
    {
        OneLoginUsers = [
            await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null),
            await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null)
        ];

        var supportTask1 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUsers[0].Subject);
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUsers[1].Subject);

        SupportTasks = [
            supportTask1,
            supportTask2
        ];
    }

    [Fact]
    public async Task Get_ShowsListOfOpenTasksInTaskIdOrder()
    {
        // Arrange
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask1 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser1.Subject);
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser2.Subject);
        var expectedResultsOrderedByReference = (new[] { supportTask1, supportTask2 })
            .Join([oneLoginUser1, oneLoginUser2],
                task => ((OneLoginUserIdVerificationData)task.Data).OneLoginUserSubject,
                user => user.Subject,
                (task, user) => new
                {
                    task.SupportTaskReference,
                    ((OneLoginUserIdVerificationData)task.Data)!.StatedFirstName,
                    ((OneLoginUserIdVerificationData)task.Data)!.StatedLastName,
                    task.CreatedOn,
                    user.EmailAddress
                })
            .OrderBy(r => r.SupportTaskReference)
            .ToArray();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var resultRows = doc.GetElementByTestId("results")
            ?.GetElementsByTagName("tbody")
            .FirstOrDefault()
            ?.GetElementsByTagName("tr");

        Assert.NotNull(resultRows);
        var topRow = resultRows[0];
        var expectedFirstResult = expectedResultsOrderedByReference[0];
        AssertRowHasContent(topRow, "taskId", expectedFirstResult.SupportTaskReference);
        AssertRowHasContent(topRow, "name", $"{expectedFirstResult.StatedFirstName} {expectedFirstResult.StatedLastName}");
        AssertRowHasContent(topRow, "email", expectedFirstResult.EmailAddress!);
        AssertRowHasContent(topRow, "requested-on", expectedFirstResult.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));

        var nextRow = resultRows[1];
        var expectedNextResult = expectedResultsOrderedByReference[1];
        AssertRowHasContent(nextRow, "taskId", expectedNextResult.SupportTaskReference);
        AssertRowHasContent(nextRow, "name", $"{expectedNextResult.StatedFirstName} {expectedNextResult.StatedLastName}");
        AssertRowHasContent(nextRow, "email", expectedNextResult.EmailAddress!);
        AssertRowHasContent(nextRow, "requested-on", expectedNextResult.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task Get_OrderListByReferenceId_OrdersList(SortDirection sortDirection)
    {
        // Arrange
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask1 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser1.Subject);
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser2.Subject);
        var supportTasks = new[] { supportTask1, supportTask2 };
        var expectedResultsOrderedByReference = supportTasks.OrderBy(s => s.SupportTaskReference).ToArray();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification?sortBy={OneLoginIdVerificationRequestsSortByOption.ReferenceId}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var resultRows = doc.GetElementByTestId("results")
            ?.GetElementsByTagName("tbody")
            .FirstOrDefault()
            ?.GetElementsByTagName("tr");

        Assert.NotNull(resultRows);
        var topRow = resultRows[0];
        var expectedFirstResult = expectedResultsOrderedByReference[sortDirection == SortDirection.Ascending ? 0 : 1];
        var expectedNextResult = expectedResultsOrderedByReference[sortDirection == SortDirection.Ascending ? 1 : 0];
        AssertRowHasContent(resultRows[0], "taskId", expectedFirstResult.SupportTaskReference);
        AssertRowHasContent(resultRows[1], "taskId", expectedNextResult.SupportTaskReference);
    }

    [Theory]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.Name, SortDirection.Ascending)]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.Name, SortDirection.Descending)]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.RequestedOn, SortDirection.Ascending)]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.RequestedOn, SortDirection.Descending)]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.Email, SortDirection.Ascending)]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.Email, SortDirection.Descending)]
    public async Task Get_OrderListByOption_OrdersList(OneLoginIdVerificationRequestsSortByOption sortBy, SortDirection sortDirection)
    {
        // Arrange
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("Aaron@example.com"), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("Sam@example.com"), verifiedInfo: null);
        var supportTask1 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser1.Subject, options => options
            .WithStatedFirstName("Aaron")
            .WithStatedLastName("Aerosmith"));
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser2.Subject, options => options
            .WithStatedFirstName("Sam")
            .WithStatedLastName("Smith"));

        var expectedResults = (new[] { supportTask1, supportTask2 })
            .Join([oneLoginUser1, oneLoginUser2],
                task => ((OneLoginUserIdVerificationData)task.Data).OneLoginUserSubject,
                user => user.Subject,
                (task, user) => new
                {
                    task.SupportTaskReference,
                    ((OneLoginUserIdVerificationData)task.Data)!.StatedFirstName,
                    ((OneLoginUserIdVerificationData)task.Data)!.StatedLastName,
                    task.CreatedOn,
                    user.EmailAddress
                });

        var expectedResultsOrdered = sortBy switch
        {
            OneLoginIdVerificationRequestsSortByOption.ReferenceId => (sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.SupportTaskReference)
                : expectedResults.OrderByDescending(s => s.SupportTaskReference)).ToArray(),
            OneLoginIdVerificationRequestsSortByOption.Name => (sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.StatedFirstName).ThenBy(s => s.StatedLastName)
                : expectedResults.OrderByDescending(s => s.StatedFirstName).ThenByDescending(s => s.StatedLastName)).ToArray(),
            OneLoginIdVerificationRequestsSortByOption.Email => (sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.EmailAddress)
                : expectedResults.OrderByDescending(s => s.EmailAddress)).ToArray(),
            OneLoginIdVerificationRequestsSortByOption.RequestedOn => (sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.CreatedOn)
                : expectedResults.OrderByDescending(s => s.CreatedOn)).ToArray(),
            _ => expectedResults.ToArray()
        };

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification?sortBy={sortBy}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var resultRows = doc.GetElementByTestId("results")
            ?.GetElementsByTagName("tbody")
            .FirstOrDefault()
            ?.GetElementsByTagName("tr");

        Assert.NotNull(resultRows);
        AssertRowHasContent(resultRows[0], "taskId", expectedResultsOrdered[0].SupportTaskReference);
        AssertRowHasContent(resultRows[1], "taskId", expectedResultsOrdered[1].SupportTaskReference);
        AssertRowHasContent(resultRows[0], "requested-on", expectedResultsOrdered[0].CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent(resultRows[1], "requested-on", expectedResultsOrdered[1].CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent(resultRows[0], "name", $"{expectedResultsOrdered[0].StatedFirstName} {expectedResultsOrdered[0].StatedLastName}");
        AssertRowHasContent(resultRows[1], "name", $"{expectedResultsOrdered[1].StatedFirstName} {expectedResultsOrdered[1].StatedLastName}");
    }

    [Fact]
    public async Task Get_TaskListItemLinksToSupportTask()
    {
        // Arrange
        await CreateSupportTasksWithOneLoginUsersAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var resultRow = doc.GetElementByTestId("results")
            ?.GetElementsByTagName("tbody")
            .FirstOrDefault()
            ?.GetElementsByTagName("tr")
            .FirstOrDefault();

        Assert.NotNull(resultRow);
        var nameLink = resultRow.GetElementByTestId("name")!.GetElementsByTagName("a").FirstOrDefault() as IHtmlAnchorElement;
        Assert.Contains($"/support-tasks/one-login-user-id-verification/{SupportTasks![0].SupportTaskReference}/resolve", nameLink!.Href);
    }

    [Fact]
    public async Task Get_NoTasks_ShowsNoTasksMessage()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var resultSection = doc.GetElementByTestId("results");
        Assert.NotNull(resultSection);
        Assert.NotNull(resultSection.GetElementByTestId("no-tasks-message"));
    }

    private static void AssertRowHasContent(IElement row, string testId, string expectedText)
    {
        var column = row.GetElementByTestId(testId);
        Assert.NotNull(column);
        Assert.Equal(expectedText, column.TrimmedText());
    }
}

using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private OneLoginUser[]? OneLoginUsers { get; set; }
    private SupportTask[]? SupportTasks { get; set; }

    private async Task CreateSupportTasksWithOneLoginUsersAsync()
    {
        OneLoginUsers = new OneLoginUser[]
        {
            await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null),
            await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null)
        };

        var supportTask1 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUsers[0].Subject);
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(OneLoginUsers[1].Subject);

        SupportTasks = new SupportTask[]
        {
            supportTask1,
            supportTask2
        };
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
            .Join(new[] { oneLoginUser1, oneLoginUser2 },
                task => ((OneLoginUserIdVerificationData)task.Data).OneLoginUserSubject,
                user => user.Subject,
                (task, user) => new
                {
                    SupportTaskReference = task.SupportTaskReference,
                    StatedFirstName = ((OneLoginUserIdVerificationData)task.Data)!.StatedFirstName,
                    StatedLastName = ((OneLoginUserIdVerificationData)task.Data)!.StatedLastName,
                    CreatedOn = task.CreatedOn,
                    EmailAddress = user.EmailAddress
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

        void AssertRowHasContent(IElement row, string testId, string expectedText)
        {
            var column = row.GetElementByTestId(testId);
            Assert.NotNull(column);
            Assert.Equal(expectedText, column.TrimmedText());
        }
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification?sortBy={SortByOption.ReferenceId}&sortDirection={sortDirection}");

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
    [InlineData(SortByOption.Name, SortDirection.Ascending)]
    [InlineData(SortByOption.Name, SortDirection.Descending)]
    [InlineData(SortByOption.DateCreated, SortDirection.Ascending)]
    [InlineData(SortByOption.DateCreated, SortDirection.Descending)]
    [InlineData(SortByOption.Email, SortDirection.Ascending)]
    [InlineData(SortByOption.Email, SortDirection.Descending)]
    public async Task Get_OrderListByOption_OrdersList(SortByOption sortBy, SortDirection sortDirection)
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
            .Join(new[] { oneLoginUser1, oneLoginUser2 },
                task => ((OneLoginUserIdVerificationData)task.Data).OneLoginUserSubject,
                user => user.Subject,
                (task, user) => new
                {
                    SupportTaskReference = task.SupportTaskReference,
                    StatedFirstName = ((OneLoginUserIdVerificationData)task.Data)!.StatedFirstName,
                    StatedLastName = ((OneLoginUserIdVerificationData)task.Data)!.StatedLastName,
                    CreatedOn = task.CreatedOn,
                    EmailAddress = user.EmailAddress
                });

        var expectedResultsOrderedByReference = sortBy switch
        {
            SortByOption.ReferenceId => expectedResults.OrderBy(s => s.SupportTaskReference).ToArray(),
            SortByOption.Name => expectedResults.OrderBy(s => s.StatedFirstName).ThenBy(s => s.StatedLastName).ToArray(),
            SortByOption.Email => expectedResults.OrderBy(s => s.EmailAddress).ToArray(),
            SortByOption.DateCreated => expectedResults.OrderBy(s => s.CreatedOn).ToArray(),
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
        var expectedFirstResult = expectedResultsOrderedByReference[sortDirection == SortDirection.Ascending ? 0 : 1];
        var expectedNextResult = expectedResultsOrderedByReference[sortDirection == SortDirection.Ascending ? 1 : 0];
        AssertRowHasContent(resultRows[0], "taskId", expectedFirstResult.SupportTaskReference);
        AssertRowHasContent(resultRows[1], "taskId", expectedNextResult.SupportTaskReference);
        AssertRowHasContent(resultRows[0], "requested-on", expectedFirstResult.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent(resultRows[1], "requested-on", expectedNextResult.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent(resultRows[0], "name", $"{expectedFirstResult.StatedFirstName} {expectedFirstResult.StatedLastName}");
        AssertRowHasContent(resultRows[1], "name", $"{expectedNextResult.StatedFirstName} {expectedNextResult.StatedLastName}");
    }

    [Fact]
    public async Task Get_OrderListByEmailAscending_OrdersList()
    {
        // Arrange
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("Sam@example.com"), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("Aaron@example.com"), verifiedInfo: null);
        var supportTask1 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser1.Subject);
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser2.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-id-verification?sortBy={SortByOption.Email}&sortDirection={SortDirection.Ascending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var resultRows = doc.GetElementByTestId("results")
            ?.GetElementsByTagName("tbody")
            .FirstOrDefault()
            ?.GetElementsByTagName("tr");

        Assert.NotNull(resultRows);
        var topRow = resultRows[1];
        AssertRowHasContent(topRow, "email", oneLoginUser1.EmailAddress!);
        var nextRow = resultRows[0];
        AssertRowHasContent(nextRow, "email", oneLoginUser2.EmailAddress!);
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

    private void AssertRowHasContent(IElement row, string testId, string expectedText)
    {
        var column = row.GetElementByTestId(testId);
        Assert.NotNull(column);
        Assert.Equal(expectedText, column.TrimmedText());
    }
}

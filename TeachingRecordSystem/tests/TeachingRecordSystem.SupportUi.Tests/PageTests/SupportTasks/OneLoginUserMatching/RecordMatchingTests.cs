using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserMatching;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class RecordMatchingTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_ShowsListOfOpenTasksInDateAscendingOrder()
    {
        // Arrange
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser1.Subject, configure =>
                configure.WithVerifiedNames(["Alphie", "Smith"]).WithCreatedOn(new DateTime(2025,1,22, 1, 1, 1))),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser1.Subject, configure =>
                configure.WithVerifiedNames(["Bert", "Johnson"]).WithCreatedOn(new DateTime(2025,1,22, 1, 0, 0))),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser2.Subject, configure =>
                configure.WithVerifiedNames(["Colin", "Smith"]).WithCreatedOn(new DateTime(2025,1,20, 1, 1, 1)))
        };
        var expectedResults = supportTasksList
            .Join([oneLoginUser1, oneLoginUser2],
                task => ((OneLoginUserRecordMatchingData)task.Data).OneLoginUserSubject,
                user => user.Subject,
                (task, user) => new
                {
                    task.SupportTaskReference,
                    FirstName = ((OneLoginUserRecordMatchingData)task.Data).VerifiedNames!.First().First(),
                    LastName = ((OneLoginUserRecordMatchingData)task.Data).VerifiedNames!.First().Last(),
                    task.CreatedOn,
                    user.EmailAddress
                })
            .ToArray();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-matching/record-matching");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var resultRows = doc.GetElementByTestId("results")?
            .QuerySelectorAll("tbody > tr");

        Assert.NotNull(resultRows);
        var topRow = resultRows[0];
        var expectedFirstResult = expectedResults[2];
        AssertRowHasContent(topRow, "task-name-and-id", expectedFirstResult.SupportTaskReference);
        AssertRowHasContent(topRow, "email", expectedFirstResult.EmailAddress!);
        AssertRowHasContent(topRow, "requested-on", expectedFirstResult.CreatedOn.ToString(WebConstants.DateOnlyDisplayFormat));

        var nextRow = resultRows[1];
        var expectedNextResult = expectedResults[1];
        AssertRowHasContent(nextRow, "task-name-and-id", expectedNextResult.SupportTaskReference);
        AssertRowHasContent(nextRow, "email", expectedNextResult.EmailAddress!);
        AssertRowHasContent(nextRow, "requested-on", expectedNextResult.CreatedOn.ToString(WebConstants.DateOnlyDisplayFormat));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task Get_OrderListByReferenceId_OrdersList(SortDirection sortDirection)
    {
        // Arrange
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask1 = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser1.Subject);
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser2.Subject);
        var supportTasks = new[] { supportTask1, supportTask2 };
        var expectedResultsOrderedByReference = supportTasks.OrderBy(s => s.SupportTaskReference).ToArray();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-matching/record-matching?sortBy={OneLoginUserRecordMatchingSupportTasksSortByOption.SupportTaskReference}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var resultRows = doc.GetElementByTestId("results")?
            .QuerySelectorAll("tbody > tr");

        Assert.NotNull(resultRows);
        var expectedFirstResult = expectedResultsOrderedByReference[sortDirection == SortDirection.Ascending ? 0 : 1];
        var expectedNextResult = expectedResultsOrderedByReference[sortDirection == SortDirection.Ascending ? 1 : 0];
        AssertRowHasContent(resultRows[0], "task-name-and-id", expectedFirstResult.SupportTaskReference);
        AssertRowHasContent(resultRows[1], "task-name-and-id", expectedNextResult.SupportTaskReference);
    }

    [Theory]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.Name, SortDirection.Ascending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.Name, SortDirection.Descending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.RequestedOn, SortDirection.Ascending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.RequestedOn, SortDirection.Descending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.Email, SortDirection.Ascending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.Email, SortDirection.Descending)]
    public async Task Get_OrderListByOption_OrdersList(OneLoginUserRecordMatchingSupportTasksSortByOption sortBy, SortDirection sortDirection)
    {
        // Arrange
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("Aaron@example.com"), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("Sam@example.com"), verifiedInfo: null);
        var supportTask1 = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser1.Subject, options => options
            .WithVerifiedNames(["Aaron", "Aerosmith"]));
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser2.Subject, options => options
            .WithVerifiedNames(["Sam", "Smith"]));

        var expectedResults = (new[] { supportTask1, supportTask2 })
            .Join([oneLoginUser1, oneLoginUser2],
                task => ((OneLoginUserRecordMatchingData)task.Data).OneLoginUserSubject,
                user => user.Subject,
                (task, user) => new
                {
                    task.SupportTaskReference,
                    FirstName = ((OneLoginUserRecordMatchingData)task.Data).VerifiedNames!.First().First(),
                    LastName = ((OneLoginUserRecordMatchingData)task.Data).VerifiedNames!.First().Last(),
                    task.Status,
                    task.CreatedOn,
                    user.EmailAddress
                });

        var expectedResultsOrdered = (sortBy switch
        {
            OneLoginUserRecordMatchingSupportTasksSortByOption.SupportTaskReference => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.SupportTaskReference)
                : expectedResults.OrderByDescending(s => s.SupportTaskReference),
            OneLoginUserRecordMatchingSupportTasksSortByOption.Name => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.FirstName).ThenBy(s => s.LastName)
                : expectedResults.OrderByDescending(s => s.FirstName).ThenByDescending(s => s.LastName),
            OneLoginUserRecordMatchingSupportTasksSortByOption.Email => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.EmailAddress)
                : expectedResults.OrderByDescending(s => s.EmailAddress),
            OneLoginUserRecordMatchingSupportTasksSortByOption.RequestedOn => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.CreatedOn)
                : expectedResults.OrderByDescending(s => s.CreatedOn),
            _ => expectedResults
        }).ToArray();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-matching/record-matching?sortBy={sortBy}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var resultRows = doc.GetElementByTestId("results")?
            .QuerySelectorAll("tbody > tr");

        Assert.NotNull(resultRows);
        AssertRowHasContent(resultRows[0], "task-name-and-id", $"{expectedResultsOrdered[0].FirstName} {expectedResultsOrdered[0].LastName} {expectedResultsOrdered[0].SupportTaskReference}");
        AssertRowHasContent(resultRows[1], "task-name-and-id", $"{expectedResultsOrdered[1].FirstName} {expectedResultsOrdered[1].LastName} {expectedResultsOrdered[1].SupportTaskReference}");
        AssertRowHasContent(resultRows[0], "status", expectedResultsOrdered[0].Status.GetDisplayName()!);
        AssertRowHasContent(resultRows[1], "status", expectedResultsOrdered[1].Status.GetDisplayName()!);
        AssertRowHasContent(resultRows[0], "requested-on", expectedResultsOrdered[0].CreatedOn.ToString(WebConstants.DateOnlyDisplayFormat));
        AssertRowHasContent(resultRows[1], "requested-on", expectedResultsOrdered[1].CreatedOn.ToString(WebConstants.DateOnlyDisplayFormat));
    }

    [Theory]
    [InlineData(1, 20)]
    [InlineData(2, 20)]
    [InlineData(3, 1)]
    public async Task Get_ShowsPageOfResults(int page, int expectedNumberOfResults)
    {
        // Arrange
        var pageSize = 20;
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("Aaron@example.com"), verifiedInfo: null);

        // Create multiple pages
        await Enumerable.Range(1, pageSize * 2 + 1)
            .ToAsyncEnumerable()
            .Select(async (int _, CancellationToken _) => await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject))
            .ToArrayAsync();

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/record-matching?pageNumber={page}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var resultRows = doc.GetElementByTestId("results")?
            .QuerySelectorAll("tbody > tr");

        Assert.Equal(expectedNumberOfResults, resultRows?.Length);
    }

    [Fact]
    public async Task Get_HasPagination_ShowsPaginationControls()
    {
        // Arrange
        var pageSize = 20;
        var page = 1;
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("Aaron@example.com"), verifiedInfo: null);

        // Create multiple pages
        await Enumerable.Range(1, (pageSize * page) + 1)
            .ToAsyncEnumerable()
            .Select(async (int _, CancellationToken _) => await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject))
            .ToArrayAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-matching/record-matching?pageNumber={page}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.QuerySelector(".govuk-pagination"));
    }

    [Fact]
    public async Task Get_TaskListItemLinksToSupportTask()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-matching/record-matching");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var resultRows = doc.GetElementByTestId("results")?
            .QuerySelectorAll("tbody > tr");

        Assert.NotNull(resultRows);
        var nameLink = resultRows[0].GetElementByTestId("task-name-and-id")!.GetElementsByTagName("a").FirstOrDefault() as IHtmlAnchorElement;
        Assert.Contains($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve", nameLink!.Href);
    }

    [Theory]
    [InlineData("Smith", new[] { "Alphie Smith", "Colin Smith" })]
    [InlineData("bert", new[] { "Bert Johnson" })]
    [InlineData("Colin smith", new[] { "Colin Smith" })]
    [InlineData("Aaron@example.com", new[] { "Alphie Smith", "Bert Johnson", "Colin Smith" })]
    [InlineData("21 Jan 2025", new[] { "Colin Smith" })]
    [InlineData("20/1/2025", new[] { "Alphie Smith", "Bert Johnson" })]
    [InlineData("Jonsun", new[] { "Bert Johnson" })]
    [InlineData("rob", new[] { "Bert Johnson" })]
    public async Task Get_Search_ShowsMatchingResult(string search, string[] expected)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("Aaron@example.com"), verifiedInfo: null);
        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Alphie", "Smith"]).WithCreatedOn(new DateTime(2025,1,20))),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Bert", "Johnson"], ["Rob","Jonsun"]).WithCreatedOn(new DateTime(2025,1,20))),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Colin", "Smith"]).WithCreatedOn(new DateTime(2025,1,21)))
        };

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-matching/record-matching?Search={search}&sortBy={OneLoginUserRecordMatchingSupportTasksSortByOption.Name}&sortDirection={SortDirection.Ascending}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expected, doc.GetElementByTestId("results")!
            .QuerySelectorAll("tbody > tr")
            .Select(row => row.GetElementByTestId("task-name-and-id")?.GetElementsByTagName("a").FirstOrDefault()?.TrimmedText())
            .ToArray());
    }

    [Fact]
    public async Task Get_SearchByReference_ShowsMatchingResult()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("Aaron@example.com"), verifiedInfo: null);
        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Alphie", "Smith"]).WithCreatedOn(new DateTime(2025,1,20))),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Bert", "Johnson"]).WithCreatedOn(new DateTime(2025,1,20))),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Colin", "Smith"]).WithCreatedOn(new DateTime(2025,1,21)))
        };

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-matching/record-matching?Search={supportTasksList[0].SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal([supportTasksList[0].SupportTaskReference], doc.GetElementByTestId("results")!
            .QuerySelectorAll("tbody > tr")
            .Select(row => row.GetElementByTestId("task-id")!.TrimmedText())
            .ToArray());
    }

    [Fact]
    public async Task Get_Search_NoMatchingResult_ShowsMessage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("Aaron@example.com"), verifiedInfo: null);
        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Alphie", "Smith"]).WithCreatedOn(new DateTime(2025,1,20))),
        };

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-matching/record-matching?Search=bert");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("results"));
        Assert.Null(doc.QuerySelector(".govuk-pagination"));
        Assert.NotNull(doc.GetElementByTestId("search"));
        Assert.NotNull(doc.GetElementByTestId("no-results-message"));
    }

    [Fact]
    public async Task Get_NoTasks_ShowsNoTasksMessage()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/one-login-user-matching/record-matching");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByTestId("search"));
        Assert.Null(doc.GetElementByTestId("results"));
        Assert.Null(doc.QuerySelector(".govuk-pagination"));
        Assert.NotNull(doc.GetElementByTestId("no-tasks-message"));
    }

    private static void AssertRowHasContent(IElement row, string testId, string expectedText)
    {
        var column = row.GetElementByTestId(testId);
        Assert.NotNull(column);
        Assert.Contains(expectedText, column.GetTextContentWithNormalizedWhitespace());
    }
}

file static class Extensions
{
    public static string GetTextContentWithNormalizedWhitespace(this IElement element)
    {
        var textContent = element.TextContent;
        return Regex.Replace(textContent, @"\s+", " ").Trim();
    }
}

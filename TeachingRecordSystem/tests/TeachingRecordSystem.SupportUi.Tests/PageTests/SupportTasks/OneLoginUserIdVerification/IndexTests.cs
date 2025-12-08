using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

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
    public async Task Get_ShowsListOfOpenTasksWithOldestFirst()
    {
        // Arrange
        await CreateSupportTasksWithOneLoginUsersAsync();

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
        AssertRowHasContent(topRow, "name", $"{((OneLoginUserIdVerificationData)SupportTasks![0].Data)!.StatedFirstName} {((OneLoginUserIdVerificationData)SupportTasks[0].Data)!.StatedLastName}");
        AssertRowHasContent(topRow, "email", OneLoginUsers![0].EmailAddress!);
        AssertRowHasContent(topRow, "requested-on", SupportTasks[0].CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent(topRow, "requested-on", SupportTasks[0].CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));

        var nextRow = resultRows[1];
        AssertRowHasContent(nextRow, "name", $"{((OneLoginUserIdVerificationData)SupportTasks[1].Data)!.StatedFirstName} {((OneLoginUserIdVerificationData)SupportTasks[1].Data)!.StatedLastName}");
        AssertRowHasContent(nextRow, "email", OneLoginUsers[1].EmailAddress!);
        AssertRowHasContent(nextRow, "requested-on", SupportTasks[1].CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));

        void AssertRowHasContent(IElement row, string testId, string expectedText)
        {
            var column = row.GetElementByTestId(testId);
            Assert.NotNull(column);
            Assert.Equal(expectedText, column.TrimmedText());
        }
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
}

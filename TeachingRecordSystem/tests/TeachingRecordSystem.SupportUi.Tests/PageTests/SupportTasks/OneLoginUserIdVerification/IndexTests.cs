using AngleSharp.Html.Dom;
using Optional;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_ShowsListOfOpenTasks()
    {
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress());
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some(person.EmailAddress), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationDataSupportTaskAsync(oneLoginUser.Subject);

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
        AssertRowHasContent("name", $"{((OneLoginUserIdVerificationData)supportTask.Data)!.StatedFirstName} {((OneLoginUserIdVerificationData)supportTask.Data)!.StatedLastName}");
        AssertRowHasContent("email", person.EmailAddress!);
        AssertRowHasContent("requested-on", supportTask.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));

        void AssertRowHasContent(string testId, string expectedText)
        {
            var column = resultRow.GetElementByTestId(testId);
            Assert.NotNull(column);
            Assert.Equal(expectedText, column.TrimmedText());
        }
    }

    [Fact]
    public async Task Get_TaskListItemLinksToSupportTask()
    {
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress());
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some(person.EmailAddress), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationDataSupportTaskAsync(oneLoginUser.Subject);

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
        Assert.Contains($"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve", nameLink!.Href);
    }

    [Fact]
    public async Task Get_NoTasks_ShowsNoTasksMessage()
    {
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

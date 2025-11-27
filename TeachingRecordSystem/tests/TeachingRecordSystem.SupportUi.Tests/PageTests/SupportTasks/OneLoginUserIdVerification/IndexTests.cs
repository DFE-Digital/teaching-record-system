using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Optional;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_ShowsListOfOpenTasksWithOldestFirst()
    {
        var person1 = await TestData.CreatePersonAsync(p => p.WithEmailAddress());
        var person2 = await TestData.CreatePersonAsync(p => p.WithEmailAddress());
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some(person1.EmailAddress), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some(person2.EmailAddress), verifiedInfo: null);
        var supportTask1 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser1.Subject);
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser2.Subject);

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
        AssertRowHasContent(topRow, "name", $"{((OneLoginUserIdVerificationData)supportTask1.Data)!.StatedFirstName} {((OneLoginUserIdVerificationData)supportTask1.Data)!.StatedLastName}");
        AssertRowHasContent(topRow, "email", person1.EmailAddress!);
        AssertRowHasContent(topRow, "requested-on", supportTask1.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent(topRow, "requested-on", supportTask1.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));

        var nextRow = resultRows[1];
        AssertRowHasContent(nextRow, "name", $"{((OneLoginUserIdVerificationData)supportTask2.Data)!.StatedFirstName} {((OneLoginUserIdVerificationData)supportTask2.Data)!.StatedLastName}");
        AssertRowHasContent(nextRow, "email", person2.EmailAddress!);
        AssertRowHasContent(nextRow, "requested-on", supportTask2.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));

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
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress());
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some(person.EmailAddress), verifiedInfo: null);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

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

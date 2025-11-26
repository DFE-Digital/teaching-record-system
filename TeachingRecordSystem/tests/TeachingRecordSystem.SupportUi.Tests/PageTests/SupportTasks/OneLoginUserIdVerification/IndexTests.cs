using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task GetShowsListOfOpenTasks()
    {
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress());
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));
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
}

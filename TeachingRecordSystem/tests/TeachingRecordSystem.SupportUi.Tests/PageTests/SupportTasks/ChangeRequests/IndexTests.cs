using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ChangeRequests;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoOpenTasks_ShowsNoTasksMessage()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)).WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Empty(doc.GetElementsByTagName("table"));
        Assert.NotNull(doc.GetElementByTestId("no-tasks-message"));
    }

    [Theory]
    [InlineData(SupportTaskType.ChangeNameRequest)]
    [InlineData(SupportTaskType.ChangeDateOfBirthRequest)]
    public async Task Get_WithTask_ShowsExpectedDataInResultsTable(SupportTaskType supportTaskType)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        SupportTask supportTask;

        if (supportTaskType == SupportTaskType.ChangeNameRequest)
        {
            supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));
        }
        else
        {
            supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth)));
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests");

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
        AssertRowHasContent("name", $"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}");
        AssertRowHasContent("requested-on", supportTask.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent("change-type", supportTaskType == SupportTaskType.ChangeNameRequest ? "Name" : "DOB");

        void AssertRowHasContent(string testId, string expectedText)
        {
            var column = resultRow.GetElementByTestId(testId);
            Assert.NotNull(column);
            Assert.Equal(expectedText, column.TrimmedText());
        }
    }
}

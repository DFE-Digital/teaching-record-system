using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task? Get_UserWithoutRole_DoesNotRenderSupportTaskPanel()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(name: "Reviewer One");
        SetCurrentUser(user);
        var request = new HttpRequestMessage(HttpMethod.Get, "/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var supportTasks = doc.GetElementByTestId($"support-tasks-panel");
        Assert.Null(supportTasks);
    }

    [Fact]
    public async Task Get_NoTasks_RendersCorrectCounts()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(name: "Reviewer One", role: UserRoles.RecordManager);
        SetCurrentUser(user);
        var request = new HttpRequestMessage(HttpMethod.Get, "/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var myTasks = doc.GetElementByTestId($"my-tasks-count");
        var unassignedTasks = doc.GetElementByTestId($"unassigned-task-count");
        var inProgressTasks = doc.GetElementByTestId($"in-progress-tasks-count");
        Assert.NotNull(myTasks);
        Assert.NotNull(unassignedTasks);
        Assert.NotNull(inProgressTasks);
        Assert.Equal("0", myTasks.TextContent);
        Assert.Equal("0", unassignedTasks.TextContent);
        Assert.Equal("0", inProgressTasks.TextContent);
    }

    [Fact]
    public async Task Get_TasksAssignedToMe_RendersCorrectCount()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(name: "Reviewer One", role: UserRoles.RecordManager);
        SetCurrentUser(user);
        var supportTask1 = await TestData.CreateChangeNameRequestSupportTaskAsync(
            configure: r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.InProgress),
            configurePerson: p => p
                .WithFirstName("Alice")
                .WithMiddleName("The")
                .WithLastName("Apple"));
        var supportTask2 = await TestData.CreateChangeNameRequestSupportTaskAsync(
            configure: r => r
                .WithCreatedOn(new DateTime(2023, 1, 20))
                .WithStatus(SupportTaskStatus.Open),
            configurePerson: p => p
                .WithFirstName("bob")
                .WithLastName("smith"));
        await AssignToUserAsync(supportTask1, user.UserId);
        await AssignToUserAsync(supportTask2, user.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var myTasks = doc.GetElementByTestId($"my-tasks-count");
        var inProgressTasks = doc.GetElementByTestId($"in-progress-tasks-count");
        Assert.NotNull(myTasks);
        Assert.Equal("2", myTasks.TextContent);
        Assert.NotNull(inProgressTasks);
        Assert.Equal("1", inProgressTasks.TextContent);
    }

    [Fact]
    public async Task Get_Tasks_RendersCorrectCount()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(name: "Reviewer One", role: UserRoles.RecordManager);
        SetCurrentUser(user);
        var supportTask1 = await TestData.CreateChangeNameRequestSupportTaskAsync(
            configure: r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.Open),
            configurePerson: p => p
                .WithFirstName("Alice")
                .WithMiddleName("The")
                .WithLastName("Apple"));
        var supportTask2 = await TestData.CreateChangeNameRequestSupportTaskAsync(
            configure: r => r
                .WithCreatedOn(new DateTime(2023, 1, 20))
                .WithStatus(SupportTaskStatus.Open),
            configurePerson: p => p
                .WithFirstName("bob")
                .WithLastName("smith"));

        var request = new HttpRequestMessage(HttpMethod.Get, "/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var unassignedTasks = doc.GetElementByTestId($"unassigned-task-count");
        Assert.NotNull(unassignedTasks);
        Assert.Equal("2", unassignedTasks.TextContent);
    }

    private Task AssignToUserAsync(SupportTask task, Guid userId) =>
        WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.FindAsync(task.SupportTaskReference);
            dbTask!.AssignedToUserId = userId;
            await dbContext.SaveChangesAsync();
        });
}

using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using PaginationOptions = TeachingRecordSystem.SupportUi.Services.PaginationOptions;

namespace TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

public partial class SupportTaskSearchServiceTests
{
    [Fact]
    public async Task SearchSupportTasksAsync_NoFilters_ReturnsAllTasksRegardlessOfStatus()
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.Open)),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 21))
                .WithStatus(SupportTaskStatus.Closed)),
        };

        // Act
        var result = await SearchSupportTasksAsync();

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST1", "ST2"], tasks.GetKeysFor(result.SearchResults));
    }

    [Fact]
    public async Task SearchSupportTasksAsync_ResultFields()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(name: "Reviewer One");
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(
                configure: r => r
                    .WithCreatedOn(new DateTime(2025, 1, 20))
                    .WithStatus(SupportTaskStatus.InProgress),
                configurePerson: p => p
                    .WithFirstName("Alice")
                    .WithMiddleName("The")
                    .WithLastName("Apple")),
        };
        await AssignToUserAsync(tasks["ST1"], user.UserId);

        // Act
        var result = await SearchSupportTasksAsync();

        // Assert
        var item = Assert.Single(result.SearchResults);
        Assert.Equal("ST1", tasks.GetKeyFor(item.SupportTaskReference));
        Assert.Equal("Alice The Apple", item.Subject);
        Assert.Equal(SupportTaskType.ChangeNameRequest, item.SupportTaskType);
        Assert.Equal(SupportTaskStatus.InProgress, item.Status);
        Assert.Equal(user.UserId, item.AssignedToUserId);
        Assert.Equal("Reviewer One", item.AssignedToName);
        Assert.Equal(new DateTime(2025, 1, 20), item.CreatedOn);
    }

    [Fact]
    public async Task SearchSupportTasksAsync_UnassignedTask_HasNullAssignedToFields()
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(),
        };

        // Act
        var result = await SearchSupportTasksAsync();

        // Assert
        var item = Assert.Single(result.SearchResults);
        Assert.Null(item.AssignedToUserId);
        Assert.Null(item.AssignedToName);
    }

    [Theory]
    [InlineData(SupportTaskType.ChangeNameRequest, new[] { "ST1" })]
    [InlineData(SupportTaskType.ChangeDateOfBirthRequest, new[] { "ST2" })]
    public async Task SearchSupportTasksAsync_FilterBySupportTaskType_ReturnsOnlyTasksOfGivenType(SupportTaskType supportTaskType, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21))),
        };

        // Act
        var result = await SearchSupportTasksAsync(supportTaskType: supportTaskType);

        // Assert
        Assert.Equal(expectedTaskKeys.Length, result.TotalTaskCount);
        Assert.Equal(expectedTaskKeys.Length, result.SearchResults.TotalItemCount);
        Assert.Equal(expectedTaskKeys, tasks.GetKeysFor(result.SearchResults));
    }

    [Fact]
    public async Task SearchSupportTasksAsync_FilterByAssignedToUserId_ReturnsOnlyTasksAssignedToGivenUser()
    {
        // Arrange
        var userA = await TestData.CreateUserAsync(name: "User A");
        var userB = await TestData.CreateUserAsync(name: "User B");
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21))),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 22))),
        };
        await AssignToUserAsync(tasks["ST1"], userA.UserId);
        await AssignToUserAsync(tasks["ST2"], userB.UserId);
        await AssignToUserAsync(tasks["ST3"], userA.UserId);

        // Act
        var result = await SearchSupportTasksAsync(assignedToUserId: userA.UserId);

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST1", "ST3"], tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(new[] { SupportTaskStatus.Open }, new[] { "ST1" })]
    [InlineData(new[] { SupportTaskStatus.InProgress }, new[] { "ST2" })]
    [InlineData(new[] { SupportTaskStatus.Closed }, new[] { "ST3" })]
    [InlineData(new[] { SupportTaskStatus.Open, SupportTaskStatus.InProgress }, new[] { "ST1", "ST2" })]
    public async Task SearchSupportTasksAsync_FilterByStatuses_ReturnsOnlyTasksWithGivenStatuses(SupportTaskStatus[] statuses, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20)).WithStatus(SupportTaskStatus.Open)),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21)).WithStatus(SupportTaskStatus.InProgress)),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 22)).WithStatus(SupportTaskStatus.Closed)),
        };

        // Act
        var result = await SearchSupportTasksAsync(statuses: statuses);

        // Assert
        Assert.Equal(expectedTaskKeys.Length, result.TotalTaskCount);
        Assert.Equal(expectedTaskKeys.Length, result.SearchResults.TotalItemCount);
        Assert.Equal(expectedTaskKeys, tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchSupportTasksAsync_SortByTaskType_SortsByTitleRatherThanEnumValue(SortDirection sortDirection)
    {
        // Arrange
        // "Change date of birth request" sorts before "Change name request" by title, but ChangeNameRequest
        // has the lower enum value - so this asserts the ordering is by title, not the numeric enum value.
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21))),
        };

        // Act
        var result = await SearchSupportTasksAsync(sortBy: SupportTasksSortByOption.TaskType, sortDirection: sortDirection);

        // Assert
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST2", "ST1"] : ["ST1", "ST2"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchSupportTasksAsync_SortByRequestedOn_SortsByCreatedOn(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20, 12, 30, 0))),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20, 8, 10, 0))),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21, 8, 10, 0))),
        };

        // Act
        var result = await SearchSupportTasksAsync(sortBy: SupportTasksSortByOption.RequestedOn, sortDirection: sortDirection);

        // Assert
        Assert.Equal(3, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST2", "ST1", "ST3"] : ["ST3", "ST1", "ST2"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchSupportTasksAsync_SortBySubject_SortsBySubject(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(
                configure: r => r.WithCreatedOn(new DateTime(2025, 1, 20)),
                configurePerson: p => p.WithFirstName("Zeta").WithMiddleName("The").WithLastName("Zebra")),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(
                configure: r => r.WithCreatedOn(new DateTime(2025, 1, 21)),
                configurePerson: p => p.WithFirstName("Alpha").WithMiddleName("The").WithLastName("Ant")),
        };

        // Act
        var result = await SearchSupportTasksAsync(sortBy: SupportTasksSortByOption.Subject, sortDirection: sortDirection);

        // Assert
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST2", "ST1"] : ["ST1", "ST2"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchSupportTasksAsync_SortByStatus_SortsByStatus(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20)).WithStatus(SupportTaskStatus.Open)),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21)).WithStatus(SupportTaskStatus.Closed)),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 22)).WithStatus(SupportTaskStatus.InProgress)),
        };

        // Act
        var result = await SearchSupportTasksAsync(sortBy: SupportTasksSortByOption.Status, sortDirection: sortDirection);

        // Assert
        Assert.Equal(3, result.SearchResults.TotalItemCount);
        // Enum values: Open = 0, Closed = 1, InProgress = 2.
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST1", "ST2", "ST3"] : ["ST3", "ST2", "ST1"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchSupportTasksAsync_SortByAssignedTo_SortsByAssignedToName(SortDirection sortDirection)
    {
        // Arrange
        var aaron = await TestData.CreateUserAsync(name: "Aaron Reviewer");
        var zoe = await TestData.CreateUserAsync(name: "Zoe Reviewer");
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21))),
        };
        await AssignToUserAsync(tasks["ST1"], zoe.UserId);
        await AssignToUserAsync(tasks["ST2"], aaron.UserId);

        // Act
        var result = await SearchSupportTasksAsync(sortBy: SupportTasksSortByOption.AssignedTo, sortDirection: sortDirection);

        // Assert
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST2", "ST1"] : ["ST1", "ST2"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(null, new[] { "ST1", "ST2" })]
    [InlineData(0, new[] { "ST1", "ST2" })]
    [InlineData(1, new[] { "ST1", "ST2" })]
    [InlineData(2, new[] { "ST3", "ST4" })]
    [InlineData(3, new[] { "ST5" })]
    [InlineData(4, new[] { "ST5" })]
    public async Task SearchSupportTasksAsync_Pagination(int? pageNumber, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21))),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 22))),
            ["ST4"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 23))),
            ["ST5"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 24))),
        };

        // Act
        var result = await SearchSupportTasksAsync(sortBy: SupportTasksSortByOption.RequestedOn, pageNumber: pageNumber, pageSize: 2);

        // Assert
        Assert.Equal(5, result.TotalTaskCount);
        Assert.Equal(5, result.SearchResults.TotalItemCount);
        Assert.Equal(expectedTaskKeys, tasks.GetKeysFor(result.SearchResults));
    }

    private Task AssignToUserAsync(SupportTask task, Guid userId) =>
        WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.FindAsync(task.SupportTaskReference);
            dbTask!.AssignedToUserId = userId;
            await dbContext.SaveChangesAsync();
        });

    private Task<SupportTasksSearchResult> SearchSupportTasksAsync(
        SupportTaskType? supportTaskType = null,
        Guid? assignedToUserId = null,
        IReadOnlyCollection<SupportTaskStatus>? statuses = null,
        SupportTasksSortByOption? sortBy = null,
        SortDirection? sortDirection = null,
        int? pageNumber = null,
        int pageSize = 10) =>
        WithServiceAsync<SupportTaskSearchService, SupportTasksSearchResult>(service =>
            service.SearchSupportTasksAsync(
                new SupportTasksSearchOptions(supportTaskType, assignedToUserId, statuses, sortBy, sortDirection),
                new PaginationOptions(pageNumber, pageSize)));
}

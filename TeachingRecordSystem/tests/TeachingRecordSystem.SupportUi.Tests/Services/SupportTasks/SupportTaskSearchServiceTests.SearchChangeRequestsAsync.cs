using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using PaginationOptions = TeachingRecordSystem.SupportUi.Services.SupportTasks.PaginationOptions;

namespace TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

public partial class SupportTaskSearchServiceTests
{
    [Fact]
    public async Task SearchChangeRequestsAsync_ReturnsOpenChangeRequestSupportTasks()
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(),
        };

        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.FindAsync(tasks["ST1"].SupportTaskReference);
            dbTask!.Status = SupportTaskStatus.Closed;
            await dbContext.SaveChangesAsync();
        });

        // Act
        var result = await SearchChangeRequestsAsync(new(), new());

        // Assert
        Assert.Equal(1, result.TotalRequestCount);
        Assert.Equal(1, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST2"], tasks.GetKeysFor(result.SearchResults));
    }

    [Fact]
    public async Task SearchChangeRequestsAsync_ResultFields()
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                configure: r => r
                    .WithCreatedOn(new DateTime(2025, 1, 20))
                    .WithEmailAddress("alice@example.com")
                    .WithDateOfBirth(new DateOnly(1990, 1, 1)),
                configurePerson: p => p
                    .WithFirstName("Alice")
                    .WithMiddleName("The")
                    .WithLastName("Apple")),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(
                configure: r => r
                    .WithCreatedOn(new DateTime(2025, 1, 21))
                    .WithEmailAddress("bob@example.com")
                    .WithFirstName("Bob")
                    .WithMiddleName("The")
                    .WithLastName("Banana"),
                configurePerson: p => p
                    .WithFirstName("Bob")
                    .WithMiddleName("A")
                    .WithLastName("Banana")),
        };

        // Act
        var result = await SearchChangeRequestsAsync(new(), new());

        // Assert
        Assert.Equal(2, result.TotalRequestCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Collection(result.SearchResults, r =>
        {
            Assert.Equal("ST1", tasks.GetKeyFor(r.SupportTaskReference));
            Assert.Equal(new DateTime(2025, 1, 20), r.CreatedOn);
            Assert.Equal(SupportTaskType.ChangeDateOfBirthRequest, r.SupportTaskType);
            Assert.Equal("Alice", r.FirstName);
            Assert.Equal("The", r.MiddleName);
            Assert.Equal("Apple", r.LastName);
        }, r =>
        {
            Assert.Equal("ST2", tasks.GetKeyFor(r.SupportTaskReference));
            Assert.Equal(new DateTime(2025, 1, 21), r.CreatedOn);
            Assert.Equal(SupportTaskType.ChangeNameRequest, r.SupportTaskType);
            Assert.Equal("Bob", r.FirstName);
            Assert.Equal("A", r.MiddleName);
            Assert.Equal("Banana", r.LastName);
        });
    }

    [Fact]
    public async Task SearchChangeRequestsAsync_SearchTextIsEmpty_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(),
        };

        // Act
        var result = await SearchChangeRequestsAsync(new(Search: ""), new());

        // Assert
        Assert.Equal(2, result.TotalRequestCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST1", "ST2"], tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData("Alice", new[] { "ST1", "ST2", "ST3" })]
    [InlineData("A", new[] { "ST2", "ST3", "ST4" })]
    [InlineData("Banana", new[] { "ST4", "ST6" })]
    [InlineData("Alice Apple", new[] { "ST1", "ST3" })]
    [InlineData("Bob The Banana", new[] { "ST6" })]
    [InlineData("Billy The Kid", new string[0])]
    public async Task SearchChangeRequestsAsync_SearchTextIsNeitherADateNorAnEmailAddress_ReturnsTasksWithTrnRequestNamePartsMatchingAnyPartOfSearchString(string searchText, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20)),
                configurePerson: p => p.WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21)),
                configurePerson: p => p.WithFirstName("Alice").WithMiddleName("A").WithLastName("Zephyr")),
            ["ST3"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 22)),
                configurePerson: p => p.WithFirstName("Alice").WithMiddleName("A").WithLastName("Apple")),
            ["ST4"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 23)),
                configurePerson: p => p.WithFirstName("Bob").WithMiddleName("A").WithLastName("Banana")),
            ["ST5"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 24)),
                configurePerson: p => p.WithFirstName("Bob").WithMiddleName("The").WithLastName("Yellow")),
            ["ST6"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 25)),
                configurePerson: p => p.WithFirstName("Bob").WithMiddleName("The").WithLastName("Banana"))
        };

        // Act
        var result = await SearchChangeRequestsAsync(new(Search: searchText), new());

        // Assert
        Assert.Equal(6, result.TotalRequestCount);
        Assert.Equal(expectedTaskKeys.Length, result.SearchResults.TotalItemCount);
        Assert.Equal(expectedTaskKeys, tasks.GetKeysFor(result.SearchResults));
    }

    [Fact]
    public async Task SearchChangeRequestsAsync_SearchBySupportTaskType()
    {
        Assert.Fail("TODO");
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchChangeRequestsAsync_SortByName_SortsByTrnRequestMetadataFirstNameThenMiddleNameThenLastName(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20)),
                configurePerson: p => p.WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21)),
                configurePerson: p => p.WithFirstName("Alice").WithMiddleName("A").WithLastName("Zephyr")),
            ["ST3"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 22)),
                configurePerson: p => p.WithFirstName("Alice").WithMiddleName("A").WithLastName("Apple")),
            ["ST4"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 23)),
                configurePerson: p => p.WithFirstName("Bob").WithMiddleName("A").WithLastName("Banana")),
            ["ST5"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 24)),
                configurePerson: p => p.WithFirstName("Bob").WithMiddleName("The").WithLastName("Yellow")),
            ["ST6"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 25)),
                configurePerson: p => p.WithFirstName("Bob").WithMiddleName("The").WithLastName("Banana"))
        };

        // Act
        var result = await SearchChangeRequestsAsync(new(SortBy: ChangeRequestsSortByOption.Name, SortDirection: sortDirection), new());

        // Assert
        Assert.Equal(6, result.TotalRequestCount);
        Assert.Equal(6, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending
            ? ["ST3", "ST2", "ST1", "ST4", "ST6", "ST5"]
            : ["ST5", "ST6", "ST4", "ST1", "ST2", "ST3"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchChangeRequestsAsync_SortByRequestedOn_SortsByCreatedOn(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20, 12, 30, 0))),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20, 8, 10, 0))),
            ["ST3"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21, 8, 10, 0))),
            ["ST4"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21, 12, 30, 0))),
        };

        // Act
        var result = await SearchChangeRequestsAsync(new(SortBy: ChangeRequestsSortByOption.RequestedOn, SortDirection: sortDirection), new());

        // Assert
        Assert.Equal(4, result.TotalRequestCount);
        Assert.Equal(4, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST2", "ST1", "ST3", "ST4"] : ["ST4", "ST3", "ST1", "ST2"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchChangeRequestsAsync_SortByChangeType_SortsBySupportTaskType(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(),
        };

        // Act
        var result = await SearchChangeRequestsAsync(new(SortBy: ChangeRequestsSortByOption.ChangeType, SortDirection: sortDirection), new());

        // Assert
        Assert.Equal(2, result.TotalRequestCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST1", "ST2"] : ["ST2", "ST1"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(null, new[] { "ST1", "ST2" })]
    [InlineData(-1, new[] { "ST1", "ST2" })]
    [InlineData(0, new[] { "ST1", "ST2" })]
    [InlineData(1, new[] { "ST1", "ST2" })]
    [InlineData(2, new[] { "ST3", "ST4" })]
    [InlineData(3, new[] { "ST5" })]
    [InlineData(4, new[] { "ST5" })]
    public async Task SearchChangeRequestsAsync_Pagination(int? pageNumber, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 21))),
            ["ST3"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 22))),
            ["ST4"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 23))),
            ["ST5"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 24))),
        };

        // Act
        var result = await SearchChangeRequestsAsync(new(), new(PageNumber: pageNumber, ItemsPerPage: 2));

        // Assert
        Assert.Equal(5, result.TotalRequestCount);
        Assert.Equal(5, result.SearchResults.TotalItemCount);
        Assert.Equal(expectedTaskKeys, tasks.GetKeysFor(result.SearchResults));
    }

    [Fact]
    public async Task SearchChangeRequestsAsync_SearchTextSortByAndPagination()
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(configurePerson: p => p.WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(configurePerson: p => p.WithFirstName("Alice").WithMiddleName("A").WithLastName("Zephyr")),
            ["ST3"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(configurePerson: p => p.WithFirstName("Alice").WithMiddleName("A").WithLastName("Apple")),
            ["ST4"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(configurePerson: p => p.WithFirstName("Bob").WithMiddleName("A").WithLastName("Banana")),
            ["ST5"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(configurePerson: p => p.WithFirstName("Bob").WithMiddleName("The").WithLastName("Yellow")),
            ["ST6"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(configurePerson: p => p.WithFirstName("Bob").WithMiddleName("The").WithLastName("Banana")),
        };

        // Act
        var result = await SearchChangeRequestsAsync(
            new(Search: "A", SortBy: ChangeRequestsSortByOption.Name, SortDirection: SortDirection.Descending),
            new(PageNumber: 2, ItemsPerPage: 2));

        // Assert
        Assert.Equal(6, result.TotalRequestCount);
        Assert.Equal(3, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST3"], tasks.GetKeysFor(result.SearchResults));
    }

    private Task<ChangeRequestsSearchResult> SearchChangeRequestsAsync(ChangeRequestsSearchOptions searchOptions, PaginationOptions paginationOptions) =>
        WithServiceAsync<SupportTaskSearchService, ChangeRequestsSearchResult>(service => service.SearchChangeRequestsAsync(searchOptions, paginationOptions));
}

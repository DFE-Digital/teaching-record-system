using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using PaginationOptions = TeachingRecordSystem.SupportUi.Services.SupportTasks.PaginationOptions;

namespace TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

public partial class SupportTaskSearchServiceTests
{
    [Fact]
    public async Task SearchTrnRequestManualChecksAsync_ReturnsOpenTrnRequestManualChecksNeededSupportTasks()
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(),
            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(),
        };

        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.FindAsync(tasks["ST1"].SupportTaskReference);
            dbTask!.Status = SupportTaskStatus.Closed;
            await dbContext.SaveChangesAsync();
        });

        // Act
        var result = await SearchTrnRequestManualChecksAsync(new(), new());

        // Assert
        Assert.Equal(1, result.TotalTaskCount);
        Assert.Equal(1, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST2"], tasks.GetKeysFor(result.SearchResults));
    }

    [Fact]
    public async Task SearchTrnRequestManualChecksAsync_ResultFields()
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "A application");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "B application");

        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(applicationUser1.UserId,
                createdOn: new DateTime(2025, 1, 20),
                configureApiTrnRequest: r => r
                    .WithDateOfBirth(new DateOnly(1990, 1, 10))
                    .WithFirstName("Alice")
                    .WithMiddleName("The")
                    .WithLastName("Apple")),
            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(applicationUser2.UserId,
                createdOn: new DateTime(2025, 1, 21),
                configureApiTrnRequest: r => r
                    .WithDateOfBirth(new DateOnly(1990, 1, 11))
                    .WithFirstName("Bob")
                    .WithMiddleName("A")
                    .WithLastName("Banana")),
        };

        // Act
        var result = await SearchTrnRequestManualChecksAsync(new(), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Collection(result.SearchResults, r =>
        {
            Assert.Equal("ST1", tasks.GetKeyFor(r.SupportTaskReference));
            Assert.Equal(new DateTime(2025, 1, 20), r.CreatedOn);
            Assert.Equal(new DateOnly(1990, 1, 10), r.DateOfBirth);
            Assert.Equal("Alice", r.FirstName);
            Assert.Equal("The", r.MiddleName);
            Assert.Equal("Apple", r.LastName);
            Assert.Equal("A application", r.SourceApplicationName);
        }, r =>
        {
            Assert.Equal("ST2", tasks.GetKeyFor(r.SupportTaskReference));
            Assert.Equal(new DateTime(2025, 1, 21), r.CreatedOn);
            Assert.Equal(new DateOnly(1990, 1, 11), r.DateOfBirth);
            Assert.Equal("Bob", r.FirstName);
            Assert.Equal("A", r.MiddleName);
            Assert.Equal("Banana", r.LastName);
            Assert.Equal("B application", r.SourceApplicationName);
        });
    }

    [Fact]
    public async Task SearchTrnRequestManualChecksAsync_SearchTextIsEmpty_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 20)),
            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 21)),
        };

        // Act
        var result = await SearchTrnRequestManualChecksAsync(new(Search: ""), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
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
    public async Task SearchTrnRequestManualChecksAsync_SearchTextIsNeitherADateNorAnEmailAddress_ReturnsTasksWithTrnRequestNamePartsMatchingAnyPartOfSearchString(string searchText, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 20),
                configureApiTrnRequest: r => r.WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")),
            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 21),
                configureApiTrnRequest: r => r.WithFirstName("Alice").WithMiddleName("A").WithLastName("Zephyr")),
            ["ST3"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 22),
                configureApiTrnRequest: r => r.WithFirstName("Alice").WithMiddleName("A").WithLastName("Apple")),
            ["ST4"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 23),
                configureApiTrnRequest: r => r.WithFirstName("Bob").WithMiddleName("A").WithLastName("Banana")),
            ["ST5"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 24),
                configureApiTrnRequest: r => r.WithFirstName("Bob").WithMiddleName("The").WithLastName("Yellow")),
            ["ST6"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 25),
                configureApiTrnRequest: r => r.WithFirstName("Bob").WithMiddleName("The").WithLastName("Banana"))
        };

        // Act
        var result = await SearchTrnRequestManualChecksAsync(new(Search: searchText), new());

        // Assert
        Assert.Equal(6, result.TotalTaskCount);
        Assert.Equal(expectedTaskKeys.Length, result.SearchResults.TotalItemCount);
        Assert.Equal(expectedTaskKeys, tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(new[] { "A application" }, new[] { "ST1", "ST3" })]
    [InlineData(new[] { "B application" }, new[] { "ST2", "ST4" })]
    [InlineData(new[] { "A application", "B application" }, new[] { "ST1", "ST2", "ST3", "ST4" })]
    public async Task SearchTrnRequestManualChecksAsync_SearchBySource_ReturnsOnlyTasksWithGivenSource(string[] sourceNames, string[] expectedTaskKeys)
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "A application");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "B application");
        var sources = new[] { applicationUser1, applicationUser2 };
        var allSourceIds = sources.Select(s => s.UserId).ToArray();

        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(applicationUser1.UserId, createdOn: new DateTime(2025, 1, 20)),
            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(applicationUser2.UserId, createdOn: new DateTime(2025, 1, 21)),
            ["ST3"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(applicationUser1.UserId, createdOn: new DateTime(2025, 1, 22)),
            ["ST4"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(applicationUser2.UserId, createdOn: new DateTime(2025, 1, 23)),
        };

        // Act
        var sourceIds = sourceNames.Select(n => sources.First(s => s.Name == n).UserId);
        var result = await SearchTrnRequestManualChecksAsync(new(Sources: sourceIds), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(allSourceIds, result.Sources);
        Assert.Equal(expectedTaskKeys, tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchTrnRequestManualChecksAsync_SortByName_SortsByTrnRequestMetadataFirstNameThenMiddleNameThenLastName(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: r => r.WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")),
            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: r => r.WithFirstName("Alice").WithMiddleName("A").WithLastName("Zephyr")),
            ["ST3"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: r => r.WithFirstName("Alice").WithMiddleName("A").WithLastName("Apple")),
            ["ST4"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: r => r.WithFirstName("Bob").WithMiddleName("A").WithLastName("Banana")),
            ["ST5"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: r => r.WithFirstName("Bob").WithMiddleName("The").WithLastName("Yellow")),
            ["ST6"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: r => r.WithFirstName("Bob").WithMiddleName("The").WithLastName("Banana"))
        };

        // Act
        var result = await SearchTrnRequestManualChecksAsync(new(SortBy: TrnRequestManualChecksSortByOption.Name, SortDirection: sortDirection), new());

        // Assert
        Assert.Equal(6, result.TotalTaskCount);
        Assert.Equal(6, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending
            ? ["ST3", "ST2", "ST1", "ST4", "ST6", "ST5"]
            : ["ST5", "ST6", "ST4", "ST1", "ST2", "ST3"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchTrnRequestManualChecksAsync_SortByEmail_SortsByTrnRequestMetadataEmailAddress(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(configureApiTrnRequest: r => r.WithDateOfBirth(new DateOnly(1990, 1, 10))),
            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(configureApiTrnRequest: r => r.WithDateOfBirth(new DateOnly(1990, 1, 11))),
        };

        // Act
        var result = await SearchTrnRequestManualChecksAsync(new(SortBy: TrnRequestManualChecksSortByOption.DateOfBirth, SortDirection: sortDirection), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST1", "ST2"] : ["ST2", "ST1"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchTrnRequestManualChecksAsync_SortByDateCreated_SortsByCreatedOn(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 20, 12, 30, 0)),
            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 20, 8, 10, 0)),
            ["ST3"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 21, 8, 10, 0)),
            ["ST4"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 21, 12, 30, 0)),
        };

        // Act
        var result = await SearchTrnRequestManualChecksAsync(new(SortBy: TrnRequestManualChecksSortByOption.DateCreated, SortDirection: sortDirection), new());

        // Assert
        Assert.Equal(4, result.TotalTaskCount);
        Assert.Equal(4, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST2", "ST1", "ST3", "ST4"] : ["ST4", "ST3", "ST1", "ST2"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchTrnRequestManualChecksAsync_SortBySource_SortsByApplicationUserName(SortDirection sortDirection)
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "A application");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "B application");

        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(applicationUser1.UserId),
            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(applicationUser2.UserId),
        };

        // Act
        var result = await SearchTrnRequestManualChecksAsync(new(SortBy: TrnRequestManualChecksSortByOption.Source, SortDirection: sortDirection), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
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
    public async Task SearchTrnRequestManualChecksAsync_Pagination(int? pageNumber, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 20)),
            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 21)),
            ["ST3"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 22)),
            ["ST4"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 23)),
            ["ST5"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(createdOn: new DateTime(2025, 1, 24)),
        };

        // Act
        var result = await SearchTrnRequestManualChecksAsync(new(), new(PageNumber: pageNumber, ItemsPerPage: 2));

        // Assert
        Assert.Equal(5, result.TotalTaskCount);
        Assert.Equal(5, result.SearchResults.TotalItemCount);
        Assert.Equal(expectedTaskKeys, tasks.GetKeysFor(result.SearchResults));
    }

    [Fact]
    public async Task SearchTrnRequestManualChecksAsync_SearchTextSortByAndPagination()
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: t => t.WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")),
            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: t => t.WithFirstName("Alice").WithMiddleName("A").WithLastName("Zephyr")),
            ["ST3"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: t => t.WithFirstName("Alice").WithMiddleName("A").WithLastName("Apple")),
            ["ST4"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: t => t.WithFirstName("Bob").WithMiddleName("A").WithLastName("Banana")),
            ["ST5"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: t => t.WithFirstName("Bob").WithMiddleName("The").WithLastName("Yellow")),
            ["ST6"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: t => t.WithFirstName("Bob").WithMiddleName("The").WithLastName("Banana")),
        };

        // Act
        var result = await SearchTrnRequestManualChecksAsync(
            new(Search: "A", SortBy: TrnRequestManualChecksSortByOption.Name, SortDirection: SortDirection.Descending),
            new(PageNumber: 2, ItemsPerPage: 2));

        // Assert
        Assert.Equal(6, result.TotalTaskCount);
        Assert.Equal(3, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST3"], tasks.GetKeysFor(result.SearchResults));
    }

    private Task<TrnRequestManualChecksSearchResult> SearchTrnRequestManualChecksAsync(TrnRequestManualChecksSearchOptions searchOptions, PaginationOptions paginationOptions) =>
        WithServiceAsync<SupportTaskSearchService, TrnRequestManualChecksSearchResult>(service => service.SearchTrnRequestManualChecksAsync(searchOptions, paginationOptions));
}

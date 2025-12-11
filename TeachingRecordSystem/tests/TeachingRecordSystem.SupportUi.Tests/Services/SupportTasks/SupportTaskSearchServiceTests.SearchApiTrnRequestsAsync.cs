using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using PaginationOptions = TeachingRecordSystem.SupportUi.Services.SupportTasks.PaginationOptions;

namespace TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

public partial class SupportTaskSearchServiceTests
{
    [Fact]
    public async Task SearchApiTrnRequestsAsync_ReturnsOpenApiTrnRequestSupportTasks()
    {
        // Arrange
        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(),
            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(),
        });

        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.FindAsync(tasks["ST1"].SupportTaskReference);
            dbTask!.Status = SupportTaskStatus.Closed;
            await dbContext.SaveChangesAsync();
        });

        // Act
        var result = await SearchApiTrnRequestsAsync(new(), new());

        // Assert
        Assert.Equal(1, result.TotalTaskCount);
        Assert.Equal(1, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST2"], tasks.GetKeysFor(result.SearchResults));
    }

    [Fact]
    public async Task SearchApiTrnRequestsAsync_ResultFields()
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "A application");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "B application");

        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser1.UserId, r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithEmailAddress("alice@example.com")
                .WithFirstName("Alice")
                .WithMiddleName("The")
                .WithLastName("Apple")),
            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser2.UserId, r => r
                .WithCreatedOn(new DateTime(2025, 1, 21))
                .WithEmailAddress("bob@example.com")
                .WithFirstName("Bob")
                .WithMiddleName("A")
                .WithLastName("Banana")),
        });

        // Act
        var result = await SearchApiTrnRequestsAsync(new(), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Collection(result.SearchResults, r =>
        {
            Assert.Equal("ST1", tasks.GetKeyFor(r.SupportTaskReference));
            Assert.Equal(new DateTime(2025, 1, 20), r.CreatedOn);
            Assert.Equal("alice@example.com", r.EmailAddress);
            Assert.Equal("Alice", r.FirstName);
            Assert.Equal("The", r.MiddleName);
            Assert.Equal("Apple", r.LastName);
            Assert.Equal("A application", r.SourceApplicationName);
        }, r =>
        {
            Assert.Equal("ST2", tasks.GetKeyFor(r.SupportTaskReference));
            Assert.Equal(new DateTime(2025, 1, 21), r.CreatedOn);
            Assert.Equal("bob@example.com", r.EmailAddress);
            Assert.Equal("Bob", r.FirstName);
            Assert.Equal("A", r.MiddleName);
            Assert.Equal("Banana", r.LastName);
            Assert.Equal("B application", r.SourceApplicationName);
        });
    }

    [Fact]
    public async Task SearchApiTrnRequestsAsync_SearchTextIsEmpty_ReturnsAllTasks()
    {
        // Arrange
        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(),
            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync()
        });

        // Act
        var result = await SearchApiTrnRequestsAsync(new(Search: ""), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST1", "ST2"], tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData("20/01/2025")]
    [InlineData("20/1/2025")]
    [InlineData("20 Jan 2025")]
    [InlineData("20 January 2025")]
    [InlineData("20 jan 2025")]
    [InlineData("20 january 2025")]
    public async Task SearchApiTrnRequestsAsync_SearchTextIsDate_ReturnsTasksCreatedOnDate(string searchText)
    {
        // Arrange
        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST3"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 21)))
        });

        // Act
        var result = await SearchApiTrnRequestsAsync(new(Search: searchText), new());

        // Assert
        Assert.Equal(3, result.TotalTaskCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST1", "ST2"], tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData("1/20/2025")]
    [InlineData("20th Jan 2025")]
    [InlineData("20th January 2025")]
    public async Task SearchApiTrnRequestsAsync_SearchTextIsDateButInvalidFormat_DoesNotMatch(string searchText)
    {
        // Arrange
        await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 20)));

        // Act
        var result = await SearchApiTrnRequestsAsync(new(Search: searchText), new());

        // Assert
        Assert.Equal(1, result.TotalTaskCount);
        Assert.Equal(0, result.SearchResults.TotalItemCount);
        Assert.Empty(result.SearchResults);
    }

    [Fact]
    public async Task SearchApiTrnRequestsAsync_SearchTextIsEmailAddress_ReturnsTaskWithMatchingTrnRequestEmailAddress()
    {
        // Arrange
        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithEmailAddress("alice@example.com")),
            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithEmailAddress("bob@example.com")),
        });

        // Act
        var result = await SearchApiTrnRequestsAsync(new(Search: "bob@example.com"), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(1, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST2"], tasks.GetKeysFor(result.SearchResults));
    }

    [Fact]
    public async Task SearchApiTrnRequestsAsync_PartialEmailAddresss_DoesNotMatch()
    {
        // Arrange
        await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithEmailAddress("alice@example.com"));
        await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithEmailAddress("bob@example.com"));

        // Act
        var result = await SearchApiTrnRequestsAsync(new(Search: "bob@example"), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(0, result.SearchResults.TotalItemCount);
        Assert.Empty(result.SearchResults);
    }

    [Theory]
    [InlineData("Alice", new[] { "ST1", "ST2", "ST3" })]
    [InlineData("A", new[] { "ST2", "ST3", "ST4" })]
    [InlineData("Banana", new[] { "ST4", "ST6" })]
    [InlineData("Alice Apple", new[] { "ST1", "ST3" })]
    [InlineData("Bob The Banana", new[] { "ST6" })]
    [InlineData("Billy The Kid", new string[0])]
    public async Task SearchApiTrnRequestsAsync_SearchTextIsNeitherADateNorAnEmailAddress_ReturnsTasksWithTrnRequestNamePartsMatchingAnyPartOfSearchString(string searchText, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 20)).WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")),
            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 21)).WithFirstName("Alice").WithMiddleName("A").WithLastName("Zephyr")),
            ["ST3"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 22)).WithFirstName("Alice").WithMiddleName("A").WithLastName("Apple")),
            ["ST4"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 23)).WithFirstName("Bob").WithMiddleName("A").WithLastName("Banana")),
            ["ST5"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 24)).WithFirstName("Bob").WithMiddleName("The").WithLastName("Yellow")),
            ["ST6"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 25)).WithFirstName("Bob").WithMiddleName("The").WithLastName("Banana"))
        });

        // Act
        var result = await SearchApiTrnRequestsAsync(new(Search: searchText), new());

        // Assert
        Assert.Equal(6, result.TotalTaskCount);
        Assert.Equal(expectedTaskKeys.Length, result.SearchResults.TotalItemCount);
        Assert.Equal(expectedTaskKeys, tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchApiTrnRequestsAsync_SortByName_SortsByTrnRequestMetadataFirstNameThenMiddleNameThenLastName(SortDirection sortDirection)
    {
        // Arrange
        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")),
            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithFirstName("Alice").WithMiddleName("A").WithLastName("Zephyr")),
            ["ST3"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithFirstName("Alice").WithMiddleName("A").WithLastName("Apple")),
            ["ST4"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithFirstName("Bob").WithMiddleName("A").WithLastName("Banana")),
            ["ST5"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithFirstName("Bob").WithMiddleName("The").WithLastName("Yellow")),
            ["ST6"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithFirstName("Bob").WithMiddleName("The").WithLastName("Banana"))
        });

        // Act
        var result = await SearchApiTrnRequestsAsync(new(SortBy: ApiTrnRequestsSortByOption.Name, SortDirection: sortDirection), new());

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
    public async Task SearchApiTrnRequestsAsync_SortByEmail_SortsByTrnRequestMetadataEmailAddress(SortDirection sortDirection)
    {
        // Arrange
        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithEmailAddress("alice@example.com")),
            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithEmailAddress("bob@example.com")),
        });

        // Act
        var result = await SearchApiTrnRequestsAsync(new(SortBy: ApiTrnRequestsSortByOption.Email, SortDirection: sortDirection), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST1", "ST2"] : ["ST2", "ST1"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchApiTrnRequestsAsync_SortByRequestedOn_SortsByCreatedOn(SortDirection sortDirection)
    {
        // Arrange
        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 20, 12, 30, 0))),
            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 20, 8, 10, 0))),
            ["ST3"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 21, 8, 10, 0))),
            ["ST4"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 21, 12, 30, 0))),
        });

        // Act
        var result = await SearchApiTrnRequestsAsync(new(SortBy: ApiTrnRequestsSortByOption.RequestedOn, SortDirection: sortDirection), new());

        // Assert
        Assert.Equal(4, result.TotalTaskCount);
        Assert.Equal(4, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST2", "ST1", "ST3", "ST4"] : ["ST4", "ST3", "ST1", "ST2"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchApiTrnRequestsAsync_SortBySource_SortsByApplicationUserName(SortDirection sortDirection)
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "A application");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "B application");

        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser1.UserId),
            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser2.UserId),
        });

        // Act
        var result = await SearchApiTrnRequestsAsync(new(SortBy: ApiTrnRequestsSortByOption.Source, SortDirection: sortDirection), new());

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
    public async Task SearchApiTrnRequestsAsync_Pagination(int? pageNumber, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 21))),
            ["ST3"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 22))),
            ["ST4"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 23))),
            ["ST5"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: r => r.WithCreatedOn(new DateTime(2025, 1, 24))),
        });

        // Act
        var result = await SearchApiTrnRequestsAsync(new(), new(PageNumber: pageNumber, ItemsPerPage: 2));

        // Assert
        Assert.Equal(5, result.TotalTaskCount);
        Assert.Equal(5, result.SearchResults.TotalItemCount);
        Assert.Equal(expectedTaskKeys, tasks.GetKeysFor(result.SearchResults));
    }

    [Fact]
    public async Task SearchApiTrnRequestsAsync_SearchTextSortByAndPagination()
    {
        // Arrange
        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: t => t.WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")),
            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: t => t.WithFirstName("Alice").WithMiddleName("A").WithLastName("Zephyr")),
            ["ST3"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: t => t.WithFirstName("Alice").WithMiddleName("A").WithLastName("Apple")),
            ["ST4"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: t => t.WithFirstName("Bob").WithMiddleName("A").WithLastName("Banana")),
            ["ST5"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: t => t.WithFirstName("Bob").WithMiddleName("The").WithLastName("Yellow")),
            ["ST6"] = await TestData.CreateApiTrnRequestSupportTaskAsync(configure: t => t.WithFirstName("Bob").WithMiddleName("The").WithLastName("Banana")),
        });

        // Act
        var result = await SearchApiTrnRequestsAsync(
            new(Search: "A", SortBy: ApiTrnRequestsSortByOption.Name, SortDirection: SortDirection.Descending),
            new(PageNumber: 2, ItemsPerPage: 2));

        // Assert
        Assert.Equal(6, result.TotalTaskCount);
        Assert.Equal(3, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST3"], tasks.GetKeysFor(result.SearchResults));
    }

    private Task<ApiTrnRequestsSearchResult> SearchApiTrnRequestsAsync(ApiTrnRequestsSearchOptions searchOptions, PaginationOptions paginationOptions) =>
        WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));
}

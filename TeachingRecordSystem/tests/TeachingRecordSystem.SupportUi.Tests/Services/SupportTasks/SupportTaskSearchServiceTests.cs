using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public partial class SupportTaskSearchServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task SearchApiTrnRequests_TextIsDate_ReturnsTasksCreatedOnDate()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (task, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var search = task.CreatedOn.ToString("d/M/yyyy");
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.RequestedOn, SortDirection.Ascending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Collection(result.SearchResults, r => Assert.Equal(task.SupportTaskReference, r.SupportTaskReference));
    }

    [Fact]
    public async Task SearchApiTrnRequests_TextIsEmailAddress_ReturnsTaskWithMatchingTrnRequestEmailAddress()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (task, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var search = task.TrnRequestMetadata!.EmailAddress;
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.RequestedOn, SortDirection.Ascending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Collection(result.SearchResults, r => Assert.Equal(task.SupportTaskReference, r.SupportTaskReference));
    }

    [Fact]
    public async Task SearchApiTrnRequests_TextIsNeitherADateNorAnEmailAddress_ReturnsTasksWithMatchingTrnRequestName()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (task, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var search = $"{task.TrnRequestMetadata!.FirstName} {task.TrnRequestMetadata.LastName}";
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.RequestedOn, SortDirection.Ascending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Collection(result.SearchResults, r => Assert.Equal(task.SupportTaskReference, r.SupportTaskReference));
    }

    [Fact]
    public async Task SearchWithNoSearchText_ReturnsOpenApiTrnRequestSupportTasks()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (task, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var search = string.Empty;
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.RequestedOn, SortDirection.Ascending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Collection(result.SearchResults, r => Assert.Equal(task.SupportTaskReference, r.SupportTaskReference));
    }

    [Fact]
    public async Task SearchApiTrnRequests_DoesNotReturnClosedTasks()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (task, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.FindAsync(task.SupportTaskReference);
            dbTask!.Status = SupportTaskStatus.Closed;
            await dbContext.SaveChangesAsync();
        });

        var search = string.Empty;
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.RequestedOn, SortDirection.Ascending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Empty(result.SearchResults);
    }

    [Fact]
    public async Task SearchApiTrnRequests_WithSortByNameAscending_SortsByNameAscending()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (task1, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, r => r.WithFirstName("Alice").WithLastName("Zephyr"));
        var (task2, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, r => r.WithFirstName("Bob").WithLastName("Yellow"));

        var search = string.Empty;
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.Name, SortDirection.Ascending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Collection(result.SearchResults,
            r => Assert.Equal(task1.SupportTaskReference, r.SupportTaskReference),
            r => Assert.Equal(task2.SupportTaskReference, r.SupportTaskReference));
    }

    [Fact]
    public async Task SearchApiTrnRequests_WithSortByNameDescending_SortsByNameDescending()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (task1, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, r => r.WithFirstName("Alice").WithLastName("Zephyr"));
        var (task2, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, r => r.WithFirstName("Bob").WithLastName("Yellow"));

        var search = string.Empty;
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.Name, SortDirection.Descending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Collection(result.SearchResults,
            r => Assert.Equal(task2.SupportTaskReference, r.SupportTaskReference),
            r => Assert.Equal(task1.SupportTaskReference, r.SupportTaskReference));
    }

    [Fact]
    public async Task SearchApiTrnRequests_WithSortByEmailAscending_SortsByEmailAddressAscending()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (task1, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, r => r.WithEmailAddress("alice@example.com"));
        var (task2, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, r => r.WithEmailAddress("bob@example.com"));

        var search = string.Empty;
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.Email, SortDirection.Ascending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Collection(result.SearchResults,
            r => Assert.Equal(task1.SupportTaskReference, r.SupportTaskReference),
            r => Assert.Equal(task2.SupportTaskReference, r.SupportTaskReference));
    }

    [Fact]
    public async Task SearchApiTrnRequests_WithSortByEmailDescending_SortsByEmailAddressDescending()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (task1, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, r => r.WithEmailAddress("alice@example.com"));
        var (task2, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, r => r.WithEmailAddress("bob@example.com"));

        var search = string.Empty;
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.Email, SortDirection.Descending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Collection(result.SearchResults,
            r => Assert.Equal(task2.SupportTaskReference, r.SupportTaskReference),
            r => Assert.Equal(task1.SupportTaskReference, r.SupportTaskReference));
    }

    [Fact]
    public async Task SearchApiTrnRequests_WithSortByRequestedOnAscending_SortsByRequestedOnAscending()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (task1, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);
        Clock.Advance();
        var (task2, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var search = string.Empty;
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.RequestedOn, SortDirection.Ascending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Collection(result.SearchResults,
            r => Assert.Equal(task1.SupportTaskReference, r.SupportTaskReference),
            r => Assert.Equal(task2.SupportTaskReference, r.SupportTaskReference));
    }

    [Fact]
    public async Task SearchApiTrnRequests_WithSortByRequestedOnDescending_SortsByRequestedOnDescending()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (task1, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);
        Clock.Advance();
        var (task2, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var search = string.Empty;
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.RequestedOn, SortDirection.Descending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Collection(result.SearchResults,
            r => Assert.Equal(task2.SupportTaskReference, r.SupportTaskReference),
            r => Assert.Equal(task1.SupportTaskReference, r.SupportTaskReference));
    }

    [Fact]
    public async Task SearchApiTrnRequests_WithSortBySourceAscending_SortsBySourceApplicationUserNameAscending()
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "A application");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "B application");
        var (taskForUser1, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser1.UserId);
        var (taskForUser2, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser2.UserId);

        var search = string.Empty;
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.Source, SortDirection.Ascending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Collection(result.SearchResults,
            r => Assert.Equal(taskForUser1.SupportTaskReference, r.SupportTaskReference),
            r => Assert.Equal(taskForUser2.SupportTaskReference, r.SupportTaskReference));
    }

    [Fact]
    public async Task SearchApiTrnRequests_WithSortBySourceDescending_SortsBySourceApplicationUserNameDescending()
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "A application");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "B application");
        var (taskForUser1, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser1.UserId);
        var (taskForUser2, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser2.UserId);

        var search = string.Empty;
        var searchOptions = new ApiTrnRequestsSearchOptions(search, ApiTrnRequestsSortByOption.Source, SortDirection.Descending);
        var paginationOptions = new PaginationOptions(1, 10);

        // Act
        var result = await WithServiceAsync<SupportTaskSearchService, ApiTrnRequestsSearchResult>(service => service.SearchApiTrnRequestsAsync(searchOptions, paginationOptions));

        // Assert
        Assert.Collection(result.SearchResults,
            r => Assert.Equal(taskForUser2.SupportTaskReference, r.SupportTaskReference),
            r => Assert.Equal(taskForUser1.SupportTaskReference, r.SupportTaskReference));
    }
}

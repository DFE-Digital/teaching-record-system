using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using PaginationOptions = TeachingRecordSystem.SupportUi.Services.SupportTasks.PaginationOptions;

namespace TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

public partial class SupportTaskSearchServiceTests
{
    [Fact]
    public async Task SearchTeachersPensionsPotentialDuplicatesAsync_ReturnsOpenTeachersPensionsPotentialDuplicatesSupportTasks()
    {
        // Arrange
        var tasks = new TaskLookup
        {
            ["ST1"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(),
            ["ST2"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(),
        };

        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.FindAsync(tasks["ST1"].SupportTaskReference);
            dbTask!.Status = SupportTaskStatus.Closed;
            await dbContext.SaveChangesAsync();
        });

        // Act
        var result = await SearchTeachersPensionsPotentialDuplicatesAsync(new(), new());

        // Assert
        Assert.Equal(1, result.TotalTaskCount);
        Assert.Equal(1, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST2"], tasks.GetKeysFor(result.SearchResults));
    }

    [Fact]
    public async Task SearchTeachersPensionsPotentialDuplicatesAsync_ResultFields()
    {
        // Arrange
        var tasks = new TaskLookup
        {
            ["ST1"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                fileName: "zzzzzz.csv",
                integrationTransactionId: 100,
                createdOn: new DateTime(2025, 1, 20),
                configurePerson: p => p.WithNationalInsuranceNumber().WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")),
            ["ST2"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                fileName: "aaaaa.txt",
                integrationTransactionId: 2,
                createdOn: new DateTime(2025, 1, 21),
                configurePerson: p => p.WithNationalInsuranceNumber().WithFirstName("Bob").WithMiddleName("A").WithLastName("Banana"))
        };

        // Act
        var result = await SearchTeachersPensionsPotentialDuplicatesAsync(new(), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Collection(result.SearchResults, r =>
        {
            Assert.Equal("ST1", tasks.GetKeyFor(r.SupportTaskReference));
            Assert.Equal(new DateTime(2025, 1, 20), r.CreatedOn);
            Assert.Equal("zzzzzz.csv", r.Filename);
            Assert.Equal(100, r.IntegrationTransactionId);
            Assert.Equal("Alice The Apple", r.Name);
        }, r =>
        {
            Assert.Equal("ST2", tasks.GetKeyFor(r.SupportTaskReference));
            Assert.Equal(new DateTime(2025, 1, 21), r.CreatedOn);
            Assert.Equal("aaaaa.txt", r.Filename);
            Assert.Equal(2, r.IntegrationTransactionId);
            Assert.Equal("Bob A Banana", r.Name);
        });
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchTeachersPensionsPotentialDuplicatesAsync_SortByName_SortsByPersonFirstNameThenMiddleNameThenLastName(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new TaskLookup
        {
            ["ST1"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => p.WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")),
            ["ST2"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => p.WithFirstName("Alice").WithMiddleName("A").WithLastName("Zephyr")),
            ["ST3"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => p.WithFirstName("Alice").WithMiddleName("A").WithLastName("Apple")),
            ["ST4"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => p.WithFirstName("Bob").WithMiddleName("A").WithLastName("Banana")),
            ["ST5"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => p.WithFirstName("Bob").WithMiddleName("The").WithLastName("Yellow")),
            ["ST6"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => p.WithFirstName("Bob").WithMiddleName("The").WithLastName("Banana"))
        };

        // Act
        var result = await SearchTeachersPensionsPotentialDuplicatesAsync(new(SortBy: TeachersPensionsPotentialDuplicatesSortByOption.Name, SortDirection: sortDirection), new());

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
    public async Task SearchTeachersPensionsPotentialDuplicatesAsync_SortByFilename_SortsByIntegrationTransactionFilename(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new TaskLookup
        {
            ["ST1"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(fileName: "aaaaaa.csv"),
            ["ST2"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(fileName: "zzzzzz.csv"),
        };

        // Act
        var result = await SearchTeachersPensionsPotentialDuplicatesAsync(new(SortBy: TeachersPensionsPotentialDuplicatesSortByOption.Filename, SortDirection: sortDirection), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST1", "ST2"] : ["ST2", "ST1"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchTeachersPensionsPotentialDuplicatesAsync_SortByInterfaceId_SortsByIntegrationTransactionId(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new TaskLookup
        {
            ["ST1"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(integrationTransactionId: 1),
            ["ST2"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(integrationTransactionId: 2),
        };

        // Act
        var result = await SearchTeachersPensionsPotentialDuplicatesAsync(new(SortBy: TeachersPensionsPotentialDuplicatesSortByOption.InterfaceId, SortDirection: sortDirection), new());

        // Assert
        Assert.Equal(2, result.TotalTaskCount);
        Assert.Equal(2, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST1", "ST2"] : ["ST2", "ST1"],
            tasks.GetKeysFor(result.SearchResults));
    }

    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public async Task SearchTeachersPensionsPotentialDuplicatesAsync_SortByCreatedOn_SortsByCreatedOn(SortDirection sortDirection)
    {
        // Arrange
        var tasks = new TaskLookup
        {
            ["ST1"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: new DateTime(2025, 1, 20, 12, 30, 0)),
            ["ST2"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: new DateTime(2025, 1, 20, 8, 10, 0)),
            ["ST3"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: new DateTime(2025, 1, 21, 8, 10, 0)),
            ["ST4"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: new DateTime(2025, 1, 21, 12, 30, 0)),
        };

        // Act
        var result = await SearchTeachersPensionsPotentialDuplicatesAsync(new(SortBy: TeachersPensionsPotentialDuplicatesSortByOption.CreatedOn, SortDirection: sortDirection), new());

        // Assert
        Assert.Equal(4, result.TotalTaskCount);
        Assert.Equal(4, result.SearchResults.TotalItemCount);
        Assert.Equal(sortDirection == SortDirection.Ascending ? ["ST2", "ST1", "ST3", "ST4"] : ["ST4", "ST3", "ST1", "ST2"],
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
    public async Task SearchTeachersPensionsPotentialDuplicatesAsync_Pagination(int? pageNumber, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = new TaskLookup
        {
            ["ST1"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: new DateTime(2025, 1, 20)),
            ["ST2"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: new DateTime(2025, 1, 21)),
            ["ST3"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: new DateTime(2025, 1, 22)),
            ["ST4"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: new DateTime(2025, 1, 23)),
            ["ST5"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(createdOn: new DateTime(2025, 1, 24)),
        };

        // Act
        var result = await SearchTeachersPensionsPotentialDuplicatesAsync(new(), new(PageNumber: pageNumber, ItemsPerPage: 2));

        // Assert
        Assert.Equal(5, result.TotalTaskCount);
        Assert.Equal(5, result.SearchResults.TotalItemCount);
        Assert.Equal(expectedTaskKeys, tasks.GetKeysFor(result.SearchResults));
    }

    [Fact]
    public async Task SearchTeachersPensionsPotentialDuplicatesAsync_SortByAndPagination()
    {
        // Arrange
        var tasks = new TaskLookup
        {
            ["ST1"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => p.WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")), // 3
            ["ST2"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => p.WithFirstName("Alice").WithMiddleName("A").WithLastName("Zephyr")),  // 2
            ["ST3"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => p.WithFirstName("Alice").WithMiddleName("A").WithLastName("Apple")),   // 1
            ["ST4"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => p.WithFirstName("Bob").WithMiddleName("A").WithLastName("Banana")),    // 4
            ["ST5"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => p.WithFirstName("Bob").WithMiddleName("The").WithLastName("Yellow")),  // 6
            ["ST6"] = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(configurePerson: p => p.WithFirstName("Bob").WithMiddleName("The").WithLastName("Banana"))   // 5
        };

        // Act
        var result = await SearchTeachersPensionsPotentialDuplicatesAsync(
            new(SortBy: TeachersPensionsPotentialDuplicatesSortByOption.Name, SortDirection: SortDirection.Descending),
            new(PageNumber: 2, ItemsPerPage: 2));

        // Assert
        Assert.Equal(6, result.TotalTaskCount);
        Assert.Equal(6, result.SearchResults.TotalItemCount);
        Assert.Equal(["ST4", "ST1"], tasks.GetKeysFor(result.SearchResults));
    }

    private Task<TeachersPensionsPotentialDuplicatesSearchResult> SearchTeachersPensionsPotentialDuplicatesAsync(TeachersPensionsPotentialDuplicatesSearchOptions searchOptions, PaginationOptions paginationOptions) =>
        WithServiceAsync<SupportTaskSearchService, TeachersPensionsPotentialDuplicatesSearchResult>(service => service.SearchTeachersPensionsPotentialDuplicatesAsync(searchOptions, paginationOptions));
}

using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

public partial class SupportTaskSearchServiceTests
{
    [Theory]
    [InlineData(OneLoginIdVerificationSupportTasksSortByOption.SupportTaskReference, SortDirection.Ascending)]
    [InlineData(OneLoginIdVerificationSupportTasksSortByOption.Email, SortDirection.Ascending)]
    [InlineData(OneLoginIdVerificationSupportTasksSortByOption.Name, SortDirection.Ascending)]
    [InlineData(OneLoginIdVerificationSupportTasksSortByOption.RequestedOn, SortDirection.Ascending)]
    [InlineData(OneLoginIdVerificationSupportTasksSortByOption.SupportTaskReference, SortDirection.Descending)]
    [InlineData(OneLoginIdVerificationSupportTasksSortByOption.Email, SortDirection.Descending)]
    [InlineData(OneLoginIdVerificationSupportTasksSortByOption.Name, SortDirection.Descending)]
    [InlineData(OneLoginIdVerificationSupportTasksSortByOption.RequestedOn, SortDirection.Descending)]
    public async Task SearchOneLoginIdVerificationSupportTasks_ReturnsOrderedResults(OneLoginIdVerificationSupportTasksSortByOption sortBy, SortDirection sortDirection)
    {
        // Arrange
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var oneLoginUsers = new OneLoginUser[] { oneLoginUser1, oneLoginUser2 };

        var supportTasks = new SupportTask[] {
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser1.Subject, configure =>
                configure.WithCreatedOn(new DateTime(2000,10,10,1,1,1, DateTimeKind.Utc))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser2.Subject, configure =>
                configure.WithCreatedOn(new DateTime(2000,10,11,1,1,1, DateTimeKind.Utc)))
        };

        var options = new OneLoginUserIdVerificationSupportTasksOptions(Search: null, sortBy, sortDirection);

        var expectedResults = supportTasks
            .Join(oneLoginUsers,
                task => ((OneLoginUserIdVerificationData)task.Data).OneLoginUserSubject,
                user => user.Subject,
                (task, user) => new
                {
                    task.SupportTaskReference,
                    ((OneLoginUserIdVerificationData)task.Data)!.StatedFirstName,
                    ((OneLoginUserIdVerificationData)task.Data)!.StatedLastName,
                    task.CreatedOn,
                    user.EmailAddress
                });
        var paginationOptions = new PaginationOptions(PageNumber: 1);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginIdVerificationSupportTasksSearchResult>(service =>
            service.SearchOneLoginIdVerificationSupportTasksAsync(options, paginationOptions));

        // Assert
        var expectedResultsOrdered = (sortBy switch
        {
            OneLoginIdVerificationSupportTasksSortByOption.SupportTaskReference => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.SupportTaskReference)
                : expectedResults.OrderByDescending(s => s.SupportTaskReference),
            OneLoginIdVerificationSupportTasksSortByOption.Name => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.StatedFirstName).ThenBy(s => s.StatedLastName)
                : expectedResults.OrderByDescending(s => s.StatedFirstName).ThenByDescending(s => s.StatedLastName),
            OneLoginIdVerificationSupportTasksSortByOption.Email => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.EmailAddress)
                : expectedResults.OrderByDescending(s => s.EmailAddress),
            OneLoginIdVerificationSupportTasksSortByOption.RequestedOn => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.CreatedOn)
                : expectedResults.OrderByDescending(s => s.CreatedOn),
            _ => expectedResults
        }).ToArray();

        Assert.Equal(expectedResultsOrdered.Length, results.SearchResults.Count);
        Assert.Equal(expectedResultsOrdered.Select(r => r.SupportTaskReference), results.SearchResults.Select(r => r.SupportTaskReference));
        Assert.Equal(expectedResultsOrdered.Select(r => r.EmailAddress), results.SearchResults.Select(r => r.EmailAddress));
        Assert.Equal(expectedResultsOrdered.Select(r => r.CreatedOn), results.SearchResults.Select(r => r.CreatedOn));
        Assert.Equal(expectedResultsOrdered.Select(r => r.StatedFirstName), results.SearchResults.Select(r => r.FirstName));
        Assert.Equal(expectedResultsOrdered.Select(r => r.StatedLastName), results.SearchResults.Select(r => r.LastName));
    }

    [Theory]
    [InlineData(1, 2, new[] { "Alphie", "Bert" })]
    [InlineData(2, 2, new[] { "Colin", "David" })]
    [InlineData(3, 1, new[] { "Edward" })]
    public async Task SearchOneLoginIdVerificationSupportTasks_ReturnsPagedResults(int pageNumber, int expectedResultCount, string[] expectedRecords)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Alphie").WithCreatedOn(new DateTime(2000,10,1,1,1,1))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Bert").WithCreatedOn(new DateTime(2000,9,1,1,2,1))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Colin").WithCreatedOn(new DateTime(2000,8,1,1,3,1))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("David").WithCreatedOn(new DateTime(2000,11,1,1,4,1))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Edward").WithCreatedOn(new DateTime(2000,10,1,1,5,1)))
        };

        var options = new OneLoginUserIdVerificationSupportTasksOptions(Search: null, OneLoginIdVerificationSupportTasksSortByOption.Name, SortDirection.Ascending);

        var paginationOptions = new PaginationOptions(PageNumber: pageNumber, ItemsPerPage: 2);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginIdVerificationSupportTasksSearchResult>(service =>
            service.SearchOneLoginIdVerificationSupportTasksAsync(options, paginationOptions));

        // Assert
        Assert.Equal(expectedResultCount, results.SearchResults.Count);
        Assert.Equal(expectedRecords, results.SearchResults.Select(r => r.FirstName));
    }

    [Theory]
    [InlineData("Smith", 1, 2, new[] { "Alphie", "Colin" })]
    [InlineData("Smith", 2, 1, new[] { "Edward" })]
    public async Task SearchOneLoginIdVerificationSupportTasks_SearchTerm_ReturnsPagedResults(string search, int pageNumber, int expectedResultCount, string[] expectedRecords)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Alphie").WithStatedLastName("Smith").WithCreatedOn(new DateTime(2000,10,1,1,1,1))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Bert").WithCreatedOn(new DateTime(2000,9,1,1,2,1))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Colin").WithStatedLastName("Smith").WithCreatedOn(new DateTime(2000,8,1,1,3,1))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("David").WithCreatedOn(new DateTime(2000,11,1,1,4,1))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Edward").WithStatedLastName("Smith").WithCreatedOn(new DateTime(2000,10,1,1,5,1)))
        };

        var options = new OneLoginUserIdVerificationSupportTasksOptions(Search: search, OneLoginIdVerificationSupportTasksSortByOption.Name, SortDirection.Ascending);

        var paginationOptions = new PaginationOptions(PageNumber: pageNumber, ItemsPerPage: 2);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginIdVerificationSupportTasksSearchResult>(service =>
            service.SearchOneLoginIdVerificationSupportTasksAsync(options, paginationOptions));

        // Assert
        Assert.Equal(expectedResultCount, results.SearchResults.Count);
        Assert.Equal(2, results.SearchResults.LastPage);
        Assert.Equal(3, results.SearchResults.TotalItemCount);
        Assert.Equal(expectedRecords, results.SearchResults.Select(r => r.FirstName));
    }

    [Theory]
    [InlineData("20/01/2025")]
    [InlineData("20/1/2025")]
    [InlineData("20 Jan 2025")]
    [InlineData("20 January 2025")]
    [InlineData("20 jan 2025")]
    [InlineData("20 january 2025")]
    public async Task SearchOneLoginIdVerificationSupportTasks_SearchTextIsDate_ReturnsMatchingTasks(string searchText)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Alphie").WithCreatedOn(new DateTime(2025,1,20))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Bert").WithCreatedOn(new DateTime(2025,1,20))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Colin").WithCreatedOn(new DateTime(2025,1,21)))
        };

        var options = new OneLoginUserIdVerificationSupportTasksOptions(Search: searchText, OneLoginIdVerificationSupportTasksSortByOption.Name, SortDirection.Ascending);

        var paginationOptions = new PaginationOptions(PageNumber: 1);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginIdVerificationSupportTasksSearchResult>(service =>
            service.SearchOneLoginIdVerificationSupportTasksAsync(options, paginationOptions));

        // Assert
        Assert.Equal(2, results.SearchResults.Count);
        Assert.Equal((new string[] { "Alphie", "Bert" }), results.SearchResults.Select(r => r.FirstName));
    }

    [Theory]
    [InlineData("alphie@example.com")]
    [InlineData("Alphie@example.com")]
    public async Task SearchOneLoginIdVerificationSupportTasks_SearchTextIsEmailAddress_ReturnsMatchingTasks(string searchText)
    {
        // Arrange
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("alphie@example.com"), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser1.Subject, configure =>
                configure.WithStatedFirstName("Alphie")),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser2.Subject, configure =>
                configure.WithStatedFirstName("Bert")),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser2.Subject, configure =>
                configure.WithStatedFirstName("Colin"))
        };

        var options = new OneLoginUserIdVerificationSupportTasksOptions(Search: searchText, OneLoginIdVerificationSupportTasksSortByOption.Name, SortDirection.Ascending);

        var paginationOptions = new PaginationOptions(PageNumber: 1);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginIdVerificationSupportTasksSearchResult>(service =>
            service.SearchOneLoginIdVerificationSupportTasksAsync(options, paginationOptions));

        // Assert
        Assert.Single(results.SearchResults);
        Assert.Equal((new string[] { "Alphie" }), results.SearchResults.Select(r => r.FirstName));
    }

    [Theory]
    [InlineData("alphie", new string[] { "Alphie Jones", "Alphie Smith" })]
    [InlineData("Alphie", new string[] { "Alphie Jones", "Alphie Smith" })]
    [InlineData("Alphie Jones", new string[] { "Alphie Jones" })]
    [InlineData("Smith", new string[] { "Alphie Smith" })]
    [InlineData("Jones", new string[] { "Alphie Jones", "Colin Jones" })]
    public async Task SearchOneLoginIdVerificationSupportTasks_SearchTextIsName_ReturnsMatchingTasks(string searchText, string[] expected)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Alphie").WithStatedLastName("Smith")),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Alphie").WithStatedLastName("Jones")),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Colin").WithStatedLastName("Jones"))
        };

        var options = new OneLoginUserIdVerificationSupportTasksOptions(Search: searchText, OneLoginIdVerificationSupportTasksSortByOption.Name, SortDirection.Ascending);

        var paginationOptions = new PaginationOptions(PageNumber: 1);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginIdVerificationSupportTasksSearchResult>(service =>
            service.SearchOneLoginIdVerificationSupportTasksAsync(options, paginationOptions));

        // Assert
        Assert.Equal(expected, results.SearchResults.Select(r => $"{r.FirstName} {r.LastName}"));
    }

    [Fact]
    public async Task SearchOneLoginIdVerificationSupportTasks_SearchTextIsReferenceId_ReturnsMatchingTasks()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Alphie")),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Bert")),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Colin"))
        };

        foreach (var task in supportTasksList)
        {
            var search = task.SupportTaskReference;
            var options = new OneLoginUserIdVerificationSupportTasksOptions(Search: search);

            var paginationOptions = new PaginationOptions(PageNumber: 1);

            // Act
            var results = await WithServiceAsync<SupportTaskSearchService, OneLoginIdVerificationSupportTasksSearchResult>(service =>
                service.SearchOneLoginIdVerificationSupportTasksAsync(options, paginationOptions));

            // Assert
            Assert.Single(results.SearchResults);
            Assert.Equal([task.SupportTaskReference], results.SearchResults.Select(r => r.SupportTaskReference));
        }
    }

    [Fact]
    public async Task SearchOneLoginIdVerificationSupportTasks_SearchTextIsUnusedReferenceId_ReturnsNoTasks()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Alphie")),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Bert")),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Colin"))
        };

        var search = SupportTask.GenerateSupportTaskReference();
        var options = new OneLoginUserIdVerificationSupportTasksOptions(Search: search);

        var paginationOptions = new PaginationOptions(PageNumber: 1);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginIdVerificationSupportTasksSearchResult>(service =>
            service.SearchOneLoginIdVerificationSupportTasksAsync(options, paginationOptions));

        // Assert
        Assert.Empty(results.SearchResults);
    }
}

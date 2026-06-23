using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Services;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

public partial class SupportTaskSearchServiceTests
{
    [Theory]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.SupportTaskReference, SortDirection.Ascending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.Email, SortDirection.Ascending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.Name, SortDirection.Ascending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.RequestedOn, SortDirection.Ascending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.Source, SortDirection.Ascending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.SupportTaskReference, SortDirection.Descending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.Email, SortDirection.Descending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.Name, SortDirection.Descending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.RequestedOn, SortDirection.Descending)]
    [InlineData(OneLoginUserRecordMatchingSupportTasksSortByOption.Source, SortDirection.Descending)]
    public async Task SearchOneLoginUserRecordMatchingSupportTasks_ReturnsOrderedResults(OneLoginUserRecordMatchingSupportTasksSortByOption sortBy, SortDirection sortDirection)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId1 = Guid.NewGuid().ToString();
        var trnRequestId2 = Guid.NewGuid().ToString();
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var oneLoginUsers = new OneLoginUser[] { oneLoginUser1, oneLoginUser2 };

        var supportTasks = new SupportTask[] {
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser1.Subject, configure =>
                configure.WithCreatedOn(new DateTime(2000,10,10,1,1,1, DateTimeKind.Utc))
                    .WithTrnRequestId(trnRequestId1).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser2.Subject, configure =>
                configure.WithCreatedOn(new DateTime(2000,10,11,1,1,1, DateTimeKind.Utc))
                    .WithTrnRequestId(trnRequestId2).WithClientApplicationUserId(applicationUser.UserId))
        };

        var options = new OneLoginUserRecordMatchingSupportTasksOptions(Search: null, sortBy, sortDirection);

        var expectedResults = supportTasks
            .Join(oneLoginUsers,
                task => ((OneLoginUserRecordMatchingData)task.Data).OneLoginUserSubject,
                user => user.Subject,
                (task, user) => new
                {
                    task.SupportTaskReference,
                    FirstName = ((OneLoginUserRecordMatchingData)task.Data)!.VerifiedNames!.First().First(),
                    LastName = ((OneLoginUserRecordMatchingData)task.Data)!.VerifiedNames!.First().Last(),
                    task.CreatedOn,
                    user.EmailAddress,
                    ShortName = task.TrnRequestMetadata!.ApplicationUser!.ShortName ?? task.TrnRequestMetadata!.ApplicationUser!.Name
                });
        var paginationOptions = new PaginationOptions(PageNumber: 1);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginUserRecordMatchingSupportTasksSearchResult>(service =>
            service.SearchOneLoginUserRecordMatchingSupportTasksAsync(options, paginationOptions));

        // Assert
        var expectedResultsOrdered = (sortBy switch
        {
            OneLoginUserRecordMatchingSupportTasksSortByOption.SupportTaskReference => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.SupportTaskReference)
                : expectedResults.OrderByDescending(s => s.SupportTaskReference),
            OneLoginUserRecordMatchingSupportTasksSortByOption.Name => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.FirstName).ThenBy(s => s.LastName)
                : expectedResults.OrderByDescending(s => s.FirstName).ThenByDescending(s => s.LastName),
            OneLoginUserRecordMatchingSupportTasksSortByOption.Email => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.EmailAddress)
                : expectedResults.OrderByDescending(s => s.EmailAddress),
            OneLoginUserRecordMatchingSupportTasksSortByOption.RequestedOn => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.CreatedOn)
                : expectedResults.OrderByDescending(s => s.CreatedOn),
            OneLoginUserRecordMatchingSupportTasksSortByOption.Source => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.ShortName)
                : expectedResults.OrderByDescending(s => s.ShortName),
            _ => expectedResults
        }).ToArray();

        Assert.Equal(expectedResultsOrdered.Length, results.SearchResults.Count);
        Assert.Equal(expectedResultsOrdered.Select(r => r.SupportTaskReference), results.SearchResults.Select(r => r.SupportTaskReference));
        Assert.Equal(expectedResultsOrdered.Select(r => r.EmailAddress), results.SearchResults.Select(r => r.EmailAddress));
        Assert.Equal(expectedResultsOrdered.Select(r => r.CreatedOn), results.SearchResults.Select(r => r.CreatedOn));
        Assert.Equal(expectedResultsOrdered.Select(r => r.FirstName), results.SearchResults.Select(r => r.FirstName));
        Assert.Equal(expectedResultsOrdered.Select(r => r.LastName), results.SearchResults.Select(r => r.LastName));
        Assert.Equal(expectedResultsOrdered.Select(r => r.ShortName), results.SearchResults.Select(r => r.SourceApplicationName));
    }

    [Theory]
    [InlineData(1, 2, new[] { "Alphie", "Bert" })]
    [InlineData(2, 2, new[] { "Colin", "David" })]
    [InlineData(3, 1, new[] { "Edward" })]
    public async Task SearchOneLoginUserRecordMatchingSupportTasks_ReturnsPagedResults(int pageNumber, int expectedResultCount, string[] expectedRecords)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId1 = Guid.NewGuid().ToString();
        var trnRequestId2 = Guid.NewGuid().ToString();
        var trnRequestId3 = Guid.NewGuid().ToString();
        var trnRequestId4 = Guid.NewGuid().ToString();
        var trnRequestId5 = Guid.NewGuid().ToString();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Alphie", TestData.GenerateLastName()]).WithCreatedOn(new DateTime(2000,10,1,1,1,1))
                    .WithTrnRequestId(trnRequestId1).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Bert", TestData.GenerateLastName()]).WithCreatedOn(new DateTime(2000,9,1,1,2,1))
                    .WithTrnRequestId(trnRequestId2).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Colin", TestData.GenerateLastName()]).WithCreatedOn(new DateTime(2000,8,1,1,3,1))
                    .WithTrnRequestId(trnRequestId3).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["David", TestData.GenerateLastName()]).WithCreatedOn(new DateTime(2000,11,1,1,4,1))
                    .WithTrnRequestId(trnRequestId4).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Edward", TestData.GenerateLastName()]).WithCreatedOn(new DateTime(2000,10,1,1,5,1))
                    .WithTrnRequestId(trnRequestId5).WithClientApplicationUserId(applicationUser.UserId))
        };

        var options = new OneLoginUserRecordMatchingSupportTasksOptions(Search: null, OneLoginUserRecordMatchingSupportTasksSortByOption.Name, SortDirection.Ascending);

        var paginationOptions = new PaginationOptions(PageNumber: pageNumber, ItemsPerPage: 2);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginUserRecordMatchingSupportTasksSearchResult>(service =>
            service.SearchOneLoginUserRecordMatchingSupportTasksAsync(options, paginationOptions));

        // Assert
        Assert.Equal(expectedResultCount, results.SearchResults.Count);
        Assert.Equal(expectedRecords, results.SearchResults.Select(r => r.FirstName));
    }

    [Theory]
    [InlineData("Smith", 1, 2, new[] { "Alphie", "Colin" })]
    [InlineData("Smith", 2, 1, new[] { "Edward" })]
    public async Task SearchOneLoginUserRecordMatchingSupportTasks_SearchTerm_ReturnsPagedResults(string search, int pageNumber, int expectedResultCount, string[] expectedRecords)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId1 = Guid.NewGuid().ToString();
        var trnRequestId2 = Guid.NewGuid().ToString();
        var trnRequestId3 = Guid.NewGuid().ToString();
        var trnRequestId4 = Guid.NewGuid().ToString();
        var trnRequestId5 = Guid.NewGuid().ToString();

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Alphie", "Smith"]).WithCreatedOn(new DateTime(2000,10,1,1,1,1))
                    .WithTrnRequestId(trnRequestId1).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Bert", TestData.GenerateLastName()]).WithCreatedOn(new DateTime(2000,9,1,1,2,1))
                    .WithTrnRequestId(trnRequestId2).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Colin", "Smith"]).WithCreatedOn(new DateTime(2000,8,1,1,3,1))
                    .WithTrnRequestId(trnRequestId3).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["David", TestData.GenerateLastName()]).WithCreatedOn(new DateTime(2000,11,1,1,4,1))
                    .WithTrnRequestId(trnRequestId4).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Edward", "Smith"]).WithCreatedOn(new DateTime(2000,10,1,1,5,1))
                    .WithTrnRequestId(trnRequestId5).WithClientApplicationUserId(applicationUser.UserId))
        };

        var options = new OneLoginUserRecordMatchingSupportTasksOptions(Search: search, OneLoginUserRecordMatchingSupportTasksSortByOption.Name, SortDirection.Ascending);

        var paginationOptions = new PaginationOptions(PageNumber: pageNumber, ItemsPerPage: 2);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginUserRecordMatchingSupportTasksSearchResult>(service =>
            service.SearchOneLoginUserRecordMatchingSupportTasksAsync(options, paginationOptions));

        // Assert
        Assert.Equal(expectedResultCount, results.SearchResults.Count);
        Assert.Equal(5, results.TotalTaskCount);
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
    public async Task SearchOneLoginUserRecordMatchingSupportTasks_SearchTextIsDate_ReturnsMatchingTasks(string searchText)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId1 = Guid.NewGuid().ToString();
        var trnRequestId2 = Guid.NewGuid().ToString();
        var trnRequestId3 = Guid.NewGuid().ToString();

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Alphie", TestData.GenerateLastName()]).WithCreatedOn(new DateTime(2025,1,20))
                    .WithTrnRequestId(trnRequestId1).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Bert", TestData.GenerateLastName()]).WithCreatedOn(new DateTime(2025,1,20))
                    .WithTrnRequestId(trnRequestId2).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Colin", TestData.GenerateLastName()]).WithCreatedOn(new DateTime(2025,1,21))
                    .WithTrnRequestId(trnRequestId3).WithClientApplicationUserId(applicationUser.UserId))
        };

        var options = new OneLoginUserRecordMatchingSupportTasksOptions(Search: searchText, OneLoginUserRecordMatchingSupportTasksSortByOption.Name, SortDirection.Ascending);

        var paginationOptions = new PaginationOptions(PageNumber: 1);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginUserRecordMatchingSupportTasksSearchResult>(service =>
            service.SearchOneLoginUserRecordMatchingSupportTasksAsync(options, paginationOptions));

        // Assert
        Assert.Equal(2, results.SearchResults.Count);
        Assert.Equal((new string[] { "Alphie", "Bert" }), results.SearchResults.Select(r => r.FirstName));
    }

    [Theory]
    [InlineData("alphie@example.com")]
    [InlineData("Alphie@example.com")]
    public async Task SearchOneLoginUserRecordMatchingSupportTasks_SearchTextIsEmailAddress_ReturnsMatchingTasks(string searchText)
    {
        // Arrange
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("alphie@example.com"), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId1 = Guid.NewGuid().ToString();
        var trnRequestId2 = Guid.NewGuid().ToString();
        var trnRequestId3 = Guid.NewGuid().ToString();

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser1.Subject, configure =>
                configure.WithVerifiedNames(["Alphie", TestData.GenerateLastName()])
                    .WithTrnRequestId(trnRequestId1).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser2.Subject, configure =>
                configure.WithVerifiedNames(["Bert", TestData.GenerateLastName()])
                    .WithTrnRequestId(trnRequestId2).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser2.Subject, configure =>
                configure.WithVerifiedNames(["Colin", TestData.GenerateLastName()])
                    .WithTrnRequestId(trnRequestId3).WithClientApplicationUserId(applicationUser.UserId))
        };

        var options = new OneLoginUserRecordMatchingSupportTasksOptions(Search: searchText, OneLoginUserRecordMatchingSupportTasksSortByOption.Name, SortDirection.Ascending);

        var paginationOptions = new PaginationOptions(PageNumber: 1);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginUserRecordMatchingSupportTasksSearchResult>(service =>
            service.SearchOneLoginUserRecordMatchingSupportTasksAsync(options, paginationOptions));

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
    [InlineData("Jonas", new[] { "Alphie Jones" })]
    [InlineData("alfy", new[] { "Alphie Jones" })]
    public async Task SearchOneLoginUserRecordMatchingSupportTasks_SearchTextIsName_ReturnsMatchingTasks(string searchText, string[] expected)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId1 = Guid.NewGuid().ToString();
        var trnRequestId2 = Guid.NewGuid().ToString();
        var trnRequestId3 = Guid.NewGuid().ToString();

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Alphie", "Smith"])
                    .WithTrnRequestId(trnRequestId1).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Alphie", "Jones"], ["Alfy", "Jonas"])
                    .WithTrnRequestId(trnRequestId2).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Colin", "Jones"])
                    .WithTrnRequestId(trnRequestId3).WithClientApplicationUserId(applicationUser.UserId))
        };

        var options = new OneLoginUserRecordMatchingSupportTasksOptions(Search: searchText, OneLoginUserRecordMatchingSupportTasksSortByOption.Name, SortDirection.Ascending);

        var paginationOptions = new PaginationOptions(PageNumber: 1);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginUserRecordMatchingSupportTasksSearchResult>(service =>
            service.SearchOneLoginUserRecordMatchingSupportTasksAsync(options, paginationOptions));

        // Assert
        Assert.Equal(expected, results.SearchResults.Select(r => $"{r.FirstName} {r.LastName}"));
    }

    [Fact]
    public async Task SearchOneLoginUserRecordMatchingSupportTasks_SearchTextIsReferenceId_ReturnsMatchingTasks()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId1 = Guid.NewGuid().ToString();
        var trnRequestId2 = Guid.NewGuid().ToString();
        var trnRequestId3 = Guid.NewGuid().ToString();

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Alphie", TestData.GenerateLastName()])
                    .WithTrnRequestId(trnRequestId1).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Bert", TestData.GenerateLastName()])
                    .WithTrnRequestId(trnRequestId2).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Colin", TestData.GenerateLastName()])
                    .WithTrnRequestId(trnRequestId3).WithClientApplicationUserId(applicationUser.UserId))
        };

        foreach (var task in supportTasksList)
        {
            var search = task.SupportTaskReference;
            var options = new OneLoginUserRecordMatchingSupportTasksOptions(Search: search);

            var paginationOptions = new PaginationOptions(PageNumber: 1);

            // Act
            var results = await WithServiceAsync<SupportTaskSearchService, OneLoginUserRecordMatchingSupportTasksSearchResult>(service =>
                service.SearchOneLoginUserRecordMatchingSupportTasksAsync(options, paginationOptions));

            // Assert
            Assert.Single(results.SearchResults);
            Assert.Equal([task.SupportTaskReference], results.SearchResults.Select(r => r.SupportTaskReference));
        }
    }

    [Fact]
    public async Task SearchOneLoginUserRecordMatchingSupportTasks_SearchTextIsUnusedReferenceId_ReturnsNoTasks()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId1 = Guid.NewGuid().ToString();
        var trnRequestId2 = Guid.NewGuid().ToString();
        var trnRequestId3 = Guid.NewGuid().ToString();

        var supportTasksList = new List<SupportTask>
        {
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Alphie", TestData.GenerateLastName()])
                    .WithTrnRequestId(trnRequestId1).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Bert", TestData.GenerateLastName()])
                    .WithTrnRequestId(trnRequestId2).WithClientApplicationUserId(applicationUser.UserId)),
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithVerifiedNames(["Colin", TestData.GenerateLastName()])
                    .WithTrnRequestId(trnRequestId3).WithClientApplicationUserId(applicationUser.UserId))
        };

        var search = "TASK-123";
        var options = new OneLoginUserRecordMatchingSupportTasksOptions(Search: search);

        var paginationOptions = new PaginationOptions(PageNumber: 1);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginUserRecordMatchingSupportTasksSearchResult>(service =>
            service.SearchOneLoginUserRecordMatchingSupportTasksAsync(options, paginationOptions));

        // Assert
        Assert.Empty(results.SearchResults);
    }
}

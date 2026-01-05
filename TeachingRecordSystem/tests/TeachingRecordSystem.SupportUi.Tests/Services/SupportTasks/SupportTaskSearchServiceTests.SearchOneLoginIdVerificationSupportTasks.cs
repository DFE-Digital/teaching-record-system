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

        var options = new SearchOneLoginUserIdVerificationSupportTasksOptions(sortBy, sortDirection);

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
                configure.WithStatedFirstName("Alphie").WithCreatedOn(new DateTime(2000,10,1,1,1,1, DateTimeKind.Utc))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Bert").WithCreatedOn(new DateTime(2000,9,1,1,2,1, DateTimeKind.Utc))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Colin").WithCreatedOn(new DateTime(2000,8,1,1,3,1, DateTimeKind.Utc))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("David").WithCreatedOn(new DateTime(2000,11,1,1,4,1, DateTimeKind.Utc))),
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure =>
                configure.WithStatedFirstName("Edward").WithCreatedOn(new DateTime(2000,10,1,1,5,1, DateTimeKind.Utc)))
        };

        var options = new SearchOneLoginUserIdVerificationSupportTasksOptions(OneLoginIdVerificationSupportTasksSortByOption.Name, SortDirection.Ascending);

        var paginationOptions = new PaginationOptions(PageNumber: pageNumber, ItemsPerPage: 2);

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, OneLoginIdVerificationSupportTasksSearchResult>(service =>
            service.SearchOneLoginIdVerificationSupportTasksAsync(options, paginationOptions));

        // Assert
        Assert.Equal(expectedResultCount, results.SearchResults.Count);
        Assert.Equal(expectedRecords, results.SearchResults.Select(r => r.FirstName));
    }
}

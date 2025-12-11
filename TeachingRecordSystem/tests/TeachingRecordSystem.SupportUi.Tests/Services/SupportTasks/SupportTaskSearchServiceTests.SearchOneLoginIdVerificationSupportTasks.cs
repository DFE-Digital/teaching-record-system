using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

public partial class SupportTaskSearchServiceTests
{
    private IList<SupportTask> SupportTasks { get; set; }
    private IList<OneLoginUser> OneLoginUsers { get; set; }

    private async Task CreateSupportTasks()
    {
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var oneLoginUser2 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        OneLoginUsers = [oneLoginUser1, oneLoginUser2];
        var supportTask1 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser1.Subject);
        Clock.Advance(TimeSpan.FromDays(1));
        var supportTask2 = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser2.Subject);
        SupportTasks = [supportTask1, supportTask2];
    }

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
        await CreateSupportTasks();

        var options = new SearchOneLoginUserIdVerificationSupportTasksOptions(sortBy, sortDirection);

        var expectedResults = SupportTasks
            .Join(OneLoginUsers,
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

        // Act
        var results = await WithServiceAsync<SupportTaskSearchService, SupportTask[]>(service => service.SearchOneLoginIdVerificationSupportTasks(options).ToArrayAsync());

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

        Assert.Equal(expectedResultsOrdered.Length, results.Length);
        Assert.Equal(expectedResultsOrdered.Select(r => r.SupportTaskReference), results.Select(r => r.SupportTaskReference));
        Assert.Equal(expectedResultsOrdered.Select(r => r.EmailAddress), results.Select(r => r.OneLoginUser!.EmailAddress));
        Assert.Equal(expectedResultsOrdered.Select(r => r.CreatedOn), results.Select(r => r.CreatedOn));
        Assert.Equal(expectedResultsOrdered.Select(r => r.StatedFirstName), results.Select(r => ((OneLoginUserIdVerificationData)r.Data).StatedFirstName));
        Assert.Equal(expectedResultsOrdered.Select(r => r.StatedLastName), results.Select(r => ((OneLoginUserIdVerificationData)r.Data).StatedLastName));
    }
}

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
    [InlineData(OneLoginIdVerificationRequestsSortByOption.ReferenceId, SortDirection.Ascending)]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.Email, SortDirection.Ascending)]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.Name, SortDirection.Ascending)]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.RequestedOn, SortDirection.Ascending)]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.ReferenceId, SortDirection.Descending)]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.Email, SortDirection.Descending)]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.Name, SortDirection.Descending)]
    [InlineData(OneLoginIdVerificationRequestsSortByOption.RequestedOn, SortDirection.Descending)]
    public async Task SearchOneLoginIdVerificationSupportTasks_ReturnsOrderedResults(OneLoginIdVerificationRequestsSortByOption sortBy, SortDirection sortDirection)
    {
        // Arrange
        await CreateSupportTasks();

        var options = new SearchOneLoginUserIdVerificationRequestsOptions(sortBy, sortDirection);

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
        var expectedResultsOrdered = sortBy switch
        {
            OneLoginIdVerificationRequestsSortByOption.ReferenceId => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.SupportTaskReference).ToArray()
                : expectedResults.OrderByDescending(s => s.SupportTaskReference).ToArray(),
            OneLoginIdVerificationRequestsSortByOption.Name => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.StatedFirstName).ThenBy(s => s.StatedLastName).ToArray()
                : expectedResults.OrderByDescending(s => s.StatedFirstName).ThenByDescending(s => s.StatedLastName).ToArray(),
            OneLoginIdVerificationRequestsSortByOption.Email => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.EmailAddress).ToArray()
                : expectedResults.OrderByDescending(s => s.EmailAddress).ToArray(),
            OneLoginIdVerificationRequestsSortByOption.RequestedOn => sortDirection == SortDirection.Ascending
                ? expectedResults.OrderBy(s => s.CreatedOn).ToArray()
                : expectedResults.OrderByDescending(s => s.CreatedOn).ToArray(),
            _ => expectedResults.ToArray()
        };

        Assert.Equal(expectedResultsOrdered.Length, results.Length);
        Assert.Equal(expectedResultsOrdered.Select(r => r.SupportTaskReference), results.Select(r => r.SupportTaskReference));
        Assert.Equal(expectedResultsOrdered.Select(r => r.EmailAddress), results.Select(r => r.OneLoginUser!.EmailAddress));
        Assert.Equal(expectedResultsOrdered.Select(r => r.CreatedOn), results.Select(r => r.CreatedOn));
        Assert.Equal(expectedResultsOrdered.Select(r => r.StatedFirstName), results.Select(r => ((OneLoginUserIdVerificationData)r.Data).StatedFirstName));
        Assert.Equal(expectedResultsOrdered.Select(r => r.StatedLastName), results.Select(r => ((OneLoginUserIdVerificationData)r.Data).StatedLastName));
    }
}

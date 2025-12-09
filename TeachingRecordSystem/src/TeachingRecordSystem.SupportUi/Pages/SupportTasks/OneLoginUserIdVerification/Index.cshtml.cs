using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification;

public class Index(TrsDbContext dbContext) : PageModel
{
    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortByOption? SortBy { get; set; }

    public IReadOnlyCollection<Result>? Results { get; set; } = [];

    public async Task OnGetAsync()
    {
        var sortDirection = SortDirection ??= SupportUi.SortDirection.Ascending;

        var tasks = dbContext.SupportTasks
            .Where(t => t.SupportTaskType == SupportTaskType.OneLoginUserIdVerification && t.Status == SupportTaskStatus.Open)
            .OrderBy(t => t.SupportTaskReference);

        var results = (await tasks
            .Select(t => new
            {
                t,
                Data = t.Data as OneLoginUserIdVerificationData
            })
            .Where(item => item.Data != null)
            .Join(
                dbContext.OneLoginUsers,
                y => y.Data!.OneLoginUserSubject,
                user => user.Subject,
                (item, user) => new Result(
                    item.t.SupportTaskReference,
                    item.Data!.StatedFirstName,
                    item.Data!.StatedLastName,
                    user.EmailAddress,
                    item.t.CreatedOn))
            .ToListAsync()).AsQueryable(); // CML TODO

        results = SortBy switch
        {
            SortByOption.ReferenceId => results.OrderBy(sortDirection, r => r.SupportTaskReference),
            SortByOption.Name => results.OrderBy(sortDirection, r => r.FirstName).ThenBy(sortDirection, r => r.LastName),
            SortByOption.Email => results.OrderBy(sortDirection, r => r.EmailAddress),
            SortByOption.DateCreated => results.OrderBy(sortDirection, r => r.CreatedOn),
            _ => results
        };

        Results = results?.ToList();
    }

    public record Result(
        string SupportTaskReference,
        string FirstName,
        string LastName,
        string? EmailAddress,
        DateTime CreatedOn)
    {
        public string Name => $"{FirstName} {LastName}";
    }
}

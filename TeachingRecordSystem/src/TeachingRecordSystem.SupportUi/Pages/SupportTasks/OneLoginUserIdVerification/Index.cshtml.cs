using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification;

public class Index(SupportTaskSearchService searchService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public OneLoginIdVerificationRequestsSortByOption? SortBy { get; set; }

    public IReadOnlyCollection<Result>? Results { get; set; }

    public async Task OnGetAsync()
    {
        var sortDirection = SortDirection ??= SupportUi.SortDirection.Ascending;

        //var tasks = dbContext.SupportTasks
        //    .Where(t => t.SupportTaskType == SupportTaskType.OneLoginUserIdVerification && t.Status == SupportTaskStatus.Open)
        //    .OrderBy(t => t.SupportTaskReference);

        //var query = tasks
        //    .Select(t => new
        //    {
        //        t.SupportTaskReference,
        //        t.CreatedOn,
        //        Data = t.Data as OneLoginUserIdVerificationData
        //    })
        //    .Where(item => item.Data != null)
        //    .Join(
        //        dbContext.OneLoginUsers,
        //        y => y.Data!.OneLoginUserSubject,
        //        user => user.Subject,
        //        (item, user) => new
        //        {
        //            item.SupportTaskReference,
        //            FirstName = item.Data!.StatedFirstName,
        //            LastName = item.Data!.StatedLastName,
        //            user.EmailAddress,
        //            item.CreatedOn
        //        });
        //query = SortBy switch
        //{
        //    OneLoginIdVerificationRequestsOptions.ReferenceId => query.OrderBy(sortDirection, r => r.SupportTaskReference),
        //    OneLoginIdVerificationRequestsOptions.Name => query.OrderBy(sortDirection, r => r.FirstName).ThenBy(sortDirection, r => r.LastName),
        //    OneLoginIdVerificationRequestsOptions.Email => query.OrderBy(sortDirection, r => r.EmailAddress),
        //    OneLoginIdVerificationRequestsOptions.DateCreated => query.OrderBy(sortDirection, r => r.CreatedOn),
        //    _ => query
        //};

        var query = searchService.SearchOneLoginIdVerificationSupportTasks(new SearchOneLoginUserIdVerificationRequestsOptions(
            SortBy ??= OneLoginIdVerificationRequestsSortByOption.ReferenceId,
            sortDirection));

        Results = (await query.ToListAsync())
            .Select(r => new Result(
                r.SupportTaskReference,
                (r.Data as OneLoginUserIdVerificationData)!.StatedFirstName,
                (r.Data as OneLoginUserIdVerificationData)!.StatedLastName,
                r.OneLoginUser!.EmailAddress,
                r.CreatedOn))
            .ToList();
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

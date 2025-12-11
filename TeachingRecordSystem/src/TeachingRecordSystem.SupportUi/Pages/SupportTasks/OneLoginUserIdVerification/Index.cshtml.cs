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
    public OneLoginIdVerificationSupportTasksSortByOption? SortBy { get; set; }

    public IReadOnlyCollection<Result>? Results { get; set; }

    public async Task OnGetAsync()
    {
        var sortDirection = SortDirection ??= SupportUi.SortDirection.Ascending;

        var query = searchService.SearchOneLoginIdVerificationSupportTasks(new SearchOneLoginUserIdVerificationSupportTasksOptions(
            SortBy ??= OneLoginIdVerificationSupportTasksSortByOption.SupportTaskReference,
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

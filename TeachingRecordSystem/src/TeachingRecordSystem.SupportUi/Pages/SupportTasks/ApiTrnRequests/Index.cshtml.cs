using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Common;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;

public class Index(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : PageModel
{
    private const int TasksPerPage = 20;

    [Display(Name = "Search", Description = "By name, email or request date, for example 4/3/1975")]
    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public ApiTrnRequestsSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "page")]
    public int? PageNumber { get; set; }

    public ResultPage<Result>? Results { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var sortDirection = SortDirection ??= SupportUi.SortDirection.Ascending;
        var sortBy = SortBy ??= ApiTrnRequestsSortByOption.Name;

        var tasks = dbContext.SupportTasks
            .Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest && t.Status == SupportTaskStatus.Open);

        Search = Search?.Trim() ?? string.Empty;

        if (SearchTextIsDate(out var date))
        {
            var minDate = date.ToDateTime(new TimeOnly(0, 0, 0), DateTimeKind.Utc);
            var maxDate = minDate.AddDays(1);
            tasks = tasks.Where(t => t.TrnRequestMetadata!.CreatedOn >= minDate && t.TrnRequestMetadata.CreatedOn < maxDate);
        }
        else if (SearchTextIsEmail())
        {
            tasks = tasks
                .Where(t => t.TrnRequestMetadata!.EmailAddress != null && t.TrnRequestMetadata.EmailAddress.ToLower() == Search.ToLower());
        }
        else
        {
            var nameParts = Search
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(n => n.ToLower())
                .ToArray();

            if (nameParts.Length > 0)
            {
                tasks = tasks.Where(t => nameParts.All(n => t.TrnRequestMetadata!.Name.Select(m => m.ToLower()).Contains(n)));
            }
        }

        if (sortBy == ApiTrnRequestsSortByOption.Name)
        {
            tasks = tasks
                .OrderBy(sortDirection, t => t.TrnRequestMetadata!.FirstName)
                .ThenBy(sortDirection, t => t.TrnRequestMetadata!.MiddleName)
                .ThenBy(sortDirection, t => t.TrnRequestMetadata!.LastName);
        }
        else if (sortBy == ApiTrnRequestsSortByOption.Email)
        {
            tasks = tasks.OrderBy(sortDirection, t => t.TrnRequestMetadata!.EmailAddress);
        }
        else if (sortBy == ApiTrnRequestsSortByOption.RequestedOn)
        {
            tasks = tasks.OrderBy(sortDirection, t => t.CreatedOn);
        }
        else if (sortBy == ApiTrnRequestsSortByOption.Source)
        {
            tasks = tasks.OrderBy(sortDirection, t => t.TrnRequestMetadata!.ApplicationUser!.Name);
        }

        Results = await tasks
            .Include(t => t.TrnRequestMetadata)
            .ThenInclude(m => m!.ApplicationUser)
            .Select(t => new Result(
                t.SupportTaskReference,
                t.TrnRequestMetadata!.FirstName!,
                t.TrnRequestMetadata!.MiddleName ?? "",
                t.TrnRequestMetadata!.LastName!,
                t.TrnRequestMetadata!.EmailAddress,
                t.CreatedOn,
                t.TrnRequestMetadata.ApplicationUser!.Name))
            .GetPageAsync(PageNumber, TasksPerPage);

        Pagination = PaginationViewModel.Create(
            Results,
            page => linkGenerator.ApiTrnRequests(Search, SortBy, SortDirection, page));

        return Page();

        bool SearchTextIsDate(out DateOnly date) =>
            DateOnly.TryParseExact(Search, UiDefaults.DateOnlyDisplayFormat, out date) ||
            DateOnly.TryParseExact(Search, "d/M/yyyy", out date);

        bool SearchTextIsEmail() => Search.Contains('@');
    }

    public record Result(
        string SupportTaskReference,
        string FirstName,
        string MiddleName,
        string LastName,
        string? EmailAddress,
        DateTime CreatedOn,
        string SourceApplicationName);
}

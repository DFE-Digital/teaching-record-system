using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests;

public class Index(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int TasksPerPage = 20;

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public ResultPage<Result>? Results { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var sortDirection = SortDirection ??= SupportUi.SortDirection.Ascending;
        var sortBy = SortBy ??= SortByOption.RequestedOn;

        var tasks = dbContext.SupportTasks
            .Include(t => t.TrnRequestMetadata)
            .ThenInclude(m => m!.ApplicationUser)
            .Where(t => t.SupportTaskType == SupportTaskType.NpqTrnRequest && t.Status == SupportTaskStatus.Open);

        Search = Search?.Trim() ?? string.Empty;

        if (SearchTextIsDate(out var date))
        {
            var minDate = date.ToDateTime(new TimeOnly(0, 0, 0), DateTimeKind.Utc);
            var maxDate = minDate.AddDays(1);
            tasks = tasks.Where(t => t.CreatedOn >= minDate && t.CreatedOn < maxDate);
        }
        else if (SearchTextIsEmail())
        {
            tasks = tasks.Where(t =>
                t.TrnRequestMetadata!.EmailAddress != null && EF.Functions.Collate(t.TrnRequestMetadata.EmailAddress, Collations.CaseInsensitive) == Search);
        }
        else
        {
            var nameParts = Search
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(n => n.ToLower(CultureInfo.InvariantCulture))
                .ToArray();

            if (nameParts.Length > 0)
            {
                tasks = tasks.Where(t =>
                    nameParts.All(n => t.TrnRequestMetadata!.Name.Select(m => EF.Functions.Collate(m, Collations.CaseInsensitive)).Contains(n)));
            }
        }

        if (sortBy == SortByOption.Name)
        {
            tasks = tasks
                .OrderBy(sortDirection, t => t.TrnRequestMetadata!.FirstName)
                .ThenBy(sortDirection, t => t.TrnRequestMetadata!.MiddleName)
                .ThenBy(sortDirection, t => t.TrnRequestMetadata!.LastName);
        }
        else if (sortBy == SortByOption.Email)
        {
            tasks = tasks.OrderBy(sortDirection, t => t.TrnRequestMetadata!.EmailAddress);
        }
        else if (sortBy == SortByOption.RequestedOn)
        {
            tasks = tasks.OrderBy(sortDirection, t => t.CreatedOn);
        }
        else if (sortBy == SortByOption.PotentialDuplicate)
        {
            tasks = tasks.OrderBy(sortDirection, t => t.TrnRequestMetadata!.PotentialDuplicate);
        }

        Results = await tasks
            .Select(t => new Result(
                t.SupportTaskReference,
                t.TrnRequestMetadata!.FirstName!,
                t.TrnRequestMetadata!.MiddleName ?? "",
                t.TrnRequestMetadata!.LastName!,
                t.TrnRequestMetadata!.EmailAddress,
                t.CreatedOn,
                t.TrnRequestMetadata.ApplicationUser!.Name,
                t.TrnRequestMetadata.PotentialDuplicate))
            .GetPageAsync(PageNumber, TasksPerPage);

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.NpqTrnRequests.Index(Search, SortBy, SortDirection, pageNumber));

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
        string SourceApplicationName,
        bool? PotentialDuplicate);
}

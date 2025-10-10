using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded;

public class Index(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int TasksPerPage = 20;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public TrnRequestManualChecksNeededSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid[]? Sources { get; set; }

    public ResultPage<Result>? Results { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public IReadOnlyDictionary<string, IReadOnlyDictionary<object, int>>? Facets { get; set; }

    [BindProperty(SupportsGet = true, Name = "_f")]
    public bool FormSubmitted { get; set; }

    public async Task OnGetAsync()
    {
        var sortDirection = SortDirection ??= SupportUi.SortDirection.Ascending;
        var sortBy = SortBy ??= TrnRequestManualChecksNeededSortByOption.DateCreated;

        var baseQuery = dbContext.SupportTasks
            .Include(t => t.TrnRequestMetadata)
            .ThenInclude(m => m!.ApplicationUser)
            .Where(t => t.SupportTaskType == SupportTaskType.TrnRequestManualChecksNeeded && t.Status == SupportTaskStatus.Open);

        var groupedBySource = await baseQuery
            .Join(dbContext.ApplicationUsers, t => t.TrnRequestMetadata!.ApplicationUserId, u => u.UserId, (t, u) => new { Task = t, User = u })
            .GroupBy(m => new { UserName = m.User.Name, m.User.UserId })
            .Select(g => new { g.Key.UserId, Count = g.Count(), g.Key.UserName })
            .ToArrayAsync();

        Facets = new Dictionary<string, IReadOnlyDictionary<object, int>>
        {
            [nameof(Sources)] = groupedBySource.ToDictionary(object (g) => new Source(g.UserId, g.UserName), g => g.Count)
        };

        var tasks = baseQuery
            .Join(dbContext.Persons, t => t.TrnRequestMetadata!.ResolvedPersonId, p => p.PersonId, (t, p) => new { Task = t, Person = p });

        if (FormSubmitted || Sources?.Length > 0)
        {
            Sources ??= [];
            tasks = tasks.Where(t => Sources.Contains(t.Task.TrnRequestMetadata!.ApplicationUserId));
        }
        else
        {
            Sources = groupedBySource.Select(g => g.UserId).ToArray();
        }

        Search = Search?.Trim() ?? string.Empty;

        {
            var nameParts = Search
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(n => n.ToLower(CultureInfo.InvariantCulture))
                .ToArray();

            if (nameParts.Length > 0)
            {
                tasks = tasks.Where(t =>
                    nameParts.All(n => t.Task.TrnRequestMetadata!.Name.Select(m => EF.Functions.Collate(m, Collations.CaseInsensitive)).Contains(n)));
            }
        }

        var totalTaskCount = await tasks.CountAsync();

        if (sortBy == TrnRequestManualChecksNeededSortByOption.Name)
        {
            tasks = tasks
                .OrderBy(sortDirection, t => t.Task.TrnRequestMetadata!.FirstName)
                .ThenBy(sortDirection, t => t.Task.TrnRequestMetadata!.MiddleName)
                .ThenBy(sortDirection, t => t.Task.TrnRequestMetadata!.LastName);
        }
        else if (sortBy == TrnRequestManualChecksNeededSortByOption.DateCreated)
        {
            tasks = tasks.OrderBy(sortDirection, t => t.Task.CreatedOn);
        }
        else if (sortBy == TrnRequestManualChecksNeededSortByOption.DateOfBirth)
        {
            tasks = tasks.OrderBy(sortDirection, t => t.Task.TrnRequestMetadata!.DateOfBirth);
        }
        else if (sortBy == TrnRequestManualChecksNeededSortByOption.Source)
        {
            tasks = tasks.OrderBy(sortDirection, t => t.Task.TrnRequestMetadata!.ApplicationUser!.Name);
        }

        Results = await tasks
            .Select(t => t.Task)
            .Select(t => new Result(
                t.SupportTaskReference,
                t.TrnRequestMetadata!.FirstName!,
                t.TrnRequestMetadata!.MiddleName ?? "",
                t.TrnRequestMetadata!.LastName!,
                t.TrnRequestMetadata!.DateOfBirth,
                t.CreatedOn,
                t.TrnRequestMetadata.ApplicationUser!.Name,
                t.TrnRequestMetadata.ApplicationUser.ShortName))
            .GetPageAsync(PageNumber, TasksPerPage, totalTaskCount);

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.TrnRequestManualChecksNeeded.Index(Search, SortBy, SortDirection, pageNumber));
    }

    public record Result(
        string SupportTaskReference,
        string FirstName,
        string MiddleName,
        string LastName,
        DateOnly DateOfBirth,
        DateTime CreatedOn,
        string SourceApplicationName,
        string? SourceApplicationShortName)
    {
        public string Source => !string.IsNullOrEmpty(SourceApplicationShortName) ? SourceApplicationShortName : SourceApplicationName;
    }

    public record Source(Guid UserId, string UserName);
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests;

public class IndexModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int TasksPerPage = 20;

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public ChangeRequestsSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public SupportTaskType[]? ChangeRequestTypes { get; set; }

    public int? NameChangeRequestCount { get; set; }

    public int? DateOfBirthChangeRequestCount { get; set; }

    public ResultPage<Result>? Results { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    [BindProperty(SupportsGet = true, Name = "_f")]
    public bool FormSubmitted { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var sortDirection = SortDirection ??= SupportUi.SortDirection.Ascending;
        var sortBy = SortBy ??= ChangeRequestsSortByOption.RequestedOn;
        var tasks = dbContext.SupportTasks
            .Include(t => t.Person)
            .Where(t => (t.SupportTaskType == SupportTaskType.ChangeNameRequest || t.SupportTaskType == SupportTaskType.ChangeDateOfBirthRequest)
                        && t.Status == SupportTaskStatus.Open);

        NameChangeRequestCount = await tasks.CountAsync(t => t.SupportTaskType == SupportTaskType.ChangeNameRequest);
        DateOfBirthChangeRequestCount = await tasks.CountAsync(t => t.SupportTaskType == SupportTaskType.ChangeDateOfBirthRequest);

        if (FormSubmitted || ChangeRequestTypes?.Length > 0)
        {
            ChangeRequestTypes ??= [];
            tasks = tasks.Where(t => ChangeRequestTypes.Contains(t.SupportTaskType));
        }

        if (sortBy == ChangeRequestsSortByOption.Name)
        {
            tasks = tasks
                .OrderBy(sortDirection, t => t.Person!.FirstName)
                .ThenBy(sortDirection, t => t.Person!.MiddleName)
                .ThenBy(sortDirection, t => t.Person!.LastName);
        }
        else if (sortBy == ChangeRequestsSortByOption.RequestedOn)
        {
            tasks = tasks.OrderBy(sortDirection, t => t.CreatedOn);
        }
        else if (sortBy == ChangeRequestsSortByOption.ChangeType)
        {
            tasks = tasks.OrderBy(sortDirection, t => t.SupportTaskType);
        }

        Results = await tasks
            .Select(t => new Result(
                t.SupportTaskReference,
                t.Person!.FirstName,
                t.Person.MiddleName,
                t.Person.LastName,
                StringHelper.JoinNonEmpty(' ', t.Person.FirstName, t.Person.MiddleName, t.Person.LastName),
                t.CreatedOn,
                t.SupportTaskType))
            .GetPageAsync(PageNumber, TasksPerPage);

        Pagination = PaginationViewModel.Create(
            Results,
            pageNumber => linkGenerator.SupportTasks.ChangeRequests.Index(sortBy: SortBy.Value, sortDirection: SortDirection.Value, pageNumber: pageNumber));

        return Page();
    }

    public record Result(
        string SupportTaskReference,
        string FirstName,
        string MiddleName,
        string LastName,
        string Name,
        DateTime CreatedOn,
        SupportTaskType SupportTaskType);
}

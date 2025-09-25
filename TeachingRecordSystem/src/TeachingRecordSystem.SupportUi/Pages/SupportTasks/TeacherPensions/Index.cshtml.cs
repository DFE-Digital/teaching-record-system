using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions;

public class IndexModel(TrsDbContext dbContext) : PageModel
{
    private const int TasksPerPage = 20;

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public TeacherPensionsSortOptions? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public List<Result>? Results { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var sortDirection = SortDirection ??= SupportUi.SortDirection.Ascending;
        var sortBy = SortBy ??= TeacherPensionsSortOptions.CreatedOn;

        var tasks = await dbContext.SupportTasks
            .Include(t => t.Person)
            .Include(t => t.TrnRequestMetadata)
            .ThenInclude(m => m!.ApplicationUser)
            .Where(t => t.SupportTaskType == SupportTaskType.TeacherPensionsPotentialDuplicate && t.Status == SupportTaskStatus.Open)
            .ToListAsync();

        var unorderedResults = tasks.Select(t =>
        {
            var data = t.Data as TeacherPensionsPotentialDuplicateData;
            return new Result
            (
                t.SupportTaskReference,
                data!.FileName,
                data!.IntegrationTransactionId,
                $"{t.Person!.FirstName} {t.Person!.LastName}",
                t.CreatedOn.ToDateOnlyWithDqtBstFix(true)
            );
        }).ToList();

        Results = (sortBy, sortDirection) switch
        {
            (TeacherPensionsSortOptions.CreatedOn, SupportUi.SortDirection.Ascending)
                => unorderedResults.OrderBy(r => r.CreatedOn).ToList(),
            (TeacherPensionsSortOptions.CreatedOn, SupportUi.SortDirection.Descending)
                => unorderedResults.OrderByDescending(r => r.CreatedOn).ToList(),

            (TeacherPensionsSortOptions.Name, SupportUi.SortDirection.Ascending)
                => unorderedResults.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList(),
            (TeacherPensionsSortOptions.Name, SupportUi.SortDirection.Descending)
                => unorderedResults.OrderByDescending(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList(),

            (TeacherPensionsSortOptions.Filename, SupportUi.SortDirection.Ascending)
                => unorderedResults.OrderBy(r => r.Filename, StringComparer.OrdinalIgnoreCase).ToList(),
            (TeacherPensionsSortOptions.Filename, SupportUi.SortDirection.Descending)
                => unorderedResults.OrderByDescending(r => r.Filename, StringComparer.OrdinalIgnoreCase).ToList(),

            (TeacherPensionsSortOptions.InterfaceId, SupportUi.SortDirection.Ascending)
                => unorderedResults.OrderBy(r => r.IntergrationTransactionId).ToList(),
            (TeacherPensionsSortOptions.InterfaceId, SupportUi.SortDirection.Descending)
                => unorderedResults.OrderByDescending(r => r.IntergrationTransactionId).ToList(),

            _ => Results
        };

        return Page();
    }

    public record Result(
        string SupportTaskReference,
        string Filename,
        long IntergrationTransactionId,
        string Name,
        DateOnly CreatedOn
     );
}

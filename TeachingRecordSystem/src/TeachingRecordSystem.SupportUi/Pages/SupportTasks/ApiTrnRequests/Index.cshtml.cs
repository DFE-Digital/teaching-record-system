using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;

public class Index(TrsDbContext dbContext) : PageModel
{
    [Display(Name = "Search", Description = "By name, email or request date, for example 4/3/1975")]
    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public string? Search { get; set; }

    public Result[]? Results { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
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

        Results = await tasks
            .Include(t => t.TrnRequestMetadata)
            .ThenInclude(m => m!.ApplicationUser)
            .Select(t => new Result(
                t.SupportTaskReference,
                t.TrnRequestMetadata!.FirstName!,
                t.TrnRequestMetadata!.LastName!,
                t.TrnRequestMetadata!.EmailAddress,
                t.CreatedOn,
                t.TrnRequestMetadata.ApplicationUser.Name))
            .ToArrayAsync();

        return Page();

        bool SearchTextIsDate(out DateOnly date) =>
            DateOnly.TryParseExact(Search, UiDefaults.DateOnlyDisplayFormat, out date) ||
            DateOnly.TryParseExact(Search, "d/M/yyyy", out date);

        bool SearchTextIsEmail() => Search.Contains('@');
    }

    public record Result(
        string SupportTaskReference,
        string FirstName,
        string LastName,
        string? EmailAddress,
        DateTime CreatedOn,
        string SourceApplicationName);
}

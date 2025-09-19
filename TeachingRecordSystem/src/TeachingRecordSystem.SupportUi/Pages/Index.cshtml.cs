using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages;

public class IndexModel(TrsDbContext dbContext) : PageModel
{
    public IReadOnlyDictionary<SupportTaskType, int>? SupportTaskCounts { get; set; }

    public async Task OnGetAsync()
    {
        SupportTaskCounts = (await dbContext.SupportTasks
                .Where(t => t.Status == SupportTaskStatus.Open)
                .GroupBy(t => t.SupportTaskType)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToArrayAsync())
            .ToDictionary(t => t.Status, t => t.Count);
    }
}

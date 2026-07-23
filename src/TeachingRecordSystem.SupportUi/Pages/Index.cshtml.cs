using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages;

public class IndexModel(TrsDbContext dbContext, IAuthorizationService authorizationService) : PageModel
{
    public IReadOnlyDictionary<SupportTaskType, int>? SupportTaskCounts { get; set; }

    // Task dashboard counts
    public int MyTasksCount { get; set; }
    public int UnassignedCount { get; set; }
    public int InProgressCount { get; set; }

    public async Task OnGetAsync()
    {
        SupportTaskCounts = (await dbContext.SupportTasks
                .Where(t => t.IsOutstanding)
                .GroupBy(t => t.SupportTaskType)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToArrayAsync())
            .ToDictionary(t => t.Status, t => t.Count);

        var canViewSupportTasks = (await authorizationService.AuthorizeAsync(User, AuthorizationPolicies.SupportTasksEdit)).Succeeded;

        if (canViewSupportTasks)
        {
            var userId = User.GetUserId();

            MyTasksCount = await dbContext.SupportTasks
                .Where(t => t.AssignedToUserId == userId && t.IsOutstanding)
                .CountAsync();

            UnassignedCount = await dbContext.SupportTasks
                .Where(t => t.AssignedToUserId == null && t.IsOutstanding)
                .CountAsync();

            InProgressCount = await dbContext.SupportTasks
                .Where(t => t.Status == SupportTaskStatus.InProgress)
                .CountAsync();
        }
    }
}

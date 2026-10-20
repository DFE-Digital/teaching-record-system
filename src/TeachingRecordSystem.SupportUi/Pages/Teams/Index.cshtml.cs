using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Teams;

[Authorize(Policy = AuthorizationPolicies.TeamManagement)]
public class IndexModel(TrsDbContext dbContext) : PageModel
{
    public TeamListItem[] Teams { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Teams = await dbContext.Teams
            .OrderBy(t => t.Name)
            .Select(t => new TeamListItem(t.TeamId, t.Name))
            .ToArrayAsync(cancellationToken);
    }

    public record TeamListItem(Guid TeamId, string Name);
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Teams;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class DeleteModel(TrsDbContext dbContext) : PageModel
{
    public string TeamName { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid teamId, CancellationToken cancellationToken)
    {
        var team = await dbContext.Teams.SingleOrDefaultAsync(t => t.TeamId == teamId, cancellationToken);
        if (team is null)
        {
            return NotFound();
        }

        TeamName = team.Name;
        return Page();
    }
}

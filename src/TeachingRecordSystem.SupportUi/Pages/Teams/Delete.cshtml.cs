using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Teams;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Teams;

[Authorize(Policy = AuthorizationPolicies.TeamManagement)]
public class DeleteModel(TrsDbContext dbContext, TeamService teamService, TimeProvider timeProvider, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private Team _team = null!;

    [FromRoute]
    public Guid TeamId { get; set; }

    public string? TeamName { get; private set; }
    public string[] MemberNames { get; private set; } = [];

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var team = dbContext.Teams.SingleOrDefault(t => t.TeamId == TeamId);
        if (team is null)
        {
            context.Result = NotFound();
            return;
        }

        _team = team;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        TeamName = _team.Name;
        MemberNames = await dbContext.Users
            .Where(u => u.TeamId == TeamId)
            .OrderBy(u => u.Name)
            .Select(u => u.Name)
            .ToArrayAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var processContext = new ProcessContext(ProcessType.TeamDeleting, timeProvider.UtcNow, User.GetUserId());
        await teamService.DeleteTeamAsync(_team, processContext, cancellationToken);

        TempData.SetFlashNotificationBanner($"{_team.Name} team deleted");
        return Redirect(linkGenerator.Teams.Index());
    }
}

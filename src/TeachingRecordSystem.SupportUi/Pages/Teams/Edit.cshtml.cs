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
public class EditModel(TrsDbContext dbContext, TeamService teamService, TimeProvider timeProvider, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<EditModel> _validator = new()
    {
        v => v.RuleFor(m => m.Name)
            .NotEmpty().WithMessage("Enter a team name")
            .MaximumLength(Team.NameMaxLength).WithMessage("Team name must be 200 characters or less")
    };

    private Team _team = null!;

    [FromRoute]
    public Guid TeamId { get; set; }

    [BindProperty]
    public string? Name { get; set; }

    public string[] MemberNames { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Name = _team.Name;
        await LoadMemberNamesAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Name = Name?.Trim();
        await LoadMemberNamesAsync(cancellationToken);
        await this.ThrowIfInvalidAsync(_validator, cancellationToken);

        if (await dbContext.Teams.AnyAsync(t => t.TeamId != TeamId && t.Name == Name, cancellationToken))
        {
            ModelState.AddModelError(nameof(Name), "Team name must be unique.");
            return this.PageWithErrors();
        }

        if (_team.Name != Name)
        {
            var processContext = new ProcessContext(ProcessType.TeamUpdating, timeProvider.UtcNow, User.GetUserId());
            try
            {
                await teamService.UpdateTeamAsync(_team, Name!, processContext, cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueIndexViolation("ix_teams_name"))
            {
                ModelState.AddModelError(nameof(Name), "Team name must be unique.");
                return this.PageWithErrors();
            }
        }

        TempData.SetFlashNotificationBanner($"Team updated to {Name}");
        return Redirect(linkGenerator.Teams.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var cancellationToken = context.HttpContext.RequestAborted;
        var team = await dbContext.Teams.SingleOrDefaultAsync(t => t.TeamId == TeamId, cancellationToken);
        if (team is null)
        {
            context.Result = NotFound();
            return;
        }

        _team = team;
        await next();
    }

    private async Task LoadMemberNamesAsync(CancellationToken cancellationToken)
    {
        MemberNames = await dbContext.Users
            .Where(u => u.TeamId == TeamId)
            .OrderBy(u => u.Name)
            .Select(u => u.Name)
            .ToArrayAsync(cancellationToken);
    }
}

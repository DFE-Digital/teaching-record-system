using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Teams;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Teams;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class AddModel(TrsDbContext dbContext, TeamService teamService, TimeProvider timeProvider, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<AddModel> _validator = new()
    {
        v => v.RuleFor(m => m.Name)
            .NotEmpty().WithMessage("Enter a team name")
            .MaximumLength(Team.NameMaxLength).WithMessage("Team name must be 200 characters or less")
    };

    [BindProperty]
    public string? Name { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Name = Name?.Trim();
        await this.ThrowIfInvalidAsync(_validator, cancellationToken);

        if (await dbContext.Teams.AnyAsync(t => t.Name == Name, cancellationToken))
        {
            ModelState.AddModelError(nameof(Name), "Team name must be unique.");
            return this.PageWithErrors();
        }

        var processContext = new ProcessContext(ProcessType.TeamCreating, timeProvider.UtcNow, User.GetUserId());
        try
        {
            await teamService.CreateTeamAsync(Name!, processContext, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueIndexViolation("ix_teams_name"))
        {
            ModelState.AddModelError(nameof(Name), "Team name must be unique.");
            return this.PageWithErrors();
        }

        TempData.SetFlashNotificationBanner($"{Name} team added");
        return Redirect(linkGenerator.Teams.Index());
    }
}

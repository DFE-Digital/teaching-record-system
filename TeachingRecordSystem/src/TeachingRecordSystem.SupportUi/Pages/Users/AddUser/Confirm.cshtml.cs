using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

namespace TeachingRecordSystem.SupportUi.Pages.Users.AddUser;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class ConfirmModel(
    TrsDbContext dbContext,
    IAadUserService userService,
    IClock clock,
    TrsLinkGenerator linkGenerator) : PageModel
{
    private User? _user;

    [BindProperty(SupportsGet = true)]
    public string? UserId { get; set; }

    [Display(Name = "Email address")]
    public string? Email { get; set; }

    [BindProperty]
    [Display(Name = "Name")]
    [Required(ErrorMessage = "Enter a name")]
    [MaxLength(Core.DataStore.Postgres.Models.UserBase.NameMaxLength, ErrorMessage = "Name must be 200 characters or less")]
    public string? Name { get; set; }

    [BindProperty]
    [Display(Name = "Role")]
    [Required(ErrorMessage = "Select a role")]
    public string? Role { get; set; }

    public string? AzureAdUserId { get; set; }

    public IEnumerable<UserRoleViewModel> RoleOptions { get; set; } = [];

    public IActionResult OnGet()
    {
        Name = _user!.Name;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Ensure submitted role is valid
        if (!string.IsNullOrWhiteSpace(Role) && !UserRoles.All.Contains(Role))
        {
            return BadRequest();
        }

        // Only admins can create new admins
        if (!User.IsInRole(UserRoles.Administrator) && Role == UserRoles.Administrator)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var newUser = new Core.DataStore.Postgres.Models.User()
        {
            Active = true,
            AzureAdUserId = AzureAdUserId,
            Email = _user!.Email,
            Name = Name!,
            Role = Role,
            UserId = Guid.NewGuid()
        };

        dbContext.Users.Add(newUser);

        await dbContext.AddEventAndBroadcastAsync(new UserAddedEvent()
        {
            EventId = Guid.NewGuid(),
            User = EventModels.User.FromModel(newUser),
            RaisedBy = User.GetUserId(),
            CreatedUtc = clock.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var roleText = UserRoles.GetDisplayNameForRole(Role!)
            .ToLowerInvariantFirstLetter()
            .WithIndefiniteArticle();

        TempData.SetFlashSuccess($"{Name} has been added as {roleText}.");
        return Redirect(linkGenerator.Users());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _user = await userService.GetUserByIdAsync(UserId!);

        if (_user is null)
        {
            context.Result = NotFound();
            return;
        }

        Email = _user.Email;
        AzureAdUserId = _user.UserId;

        var showAdminRole = User.IsInRole(UserRoles.Administrator);
        var userRoles = UserRoles.All.Where(r => showAdminRole || r != UserRoles.Administrator);

        RoleOptions = userRoles.Select(r => new UserRoleViewModel
        {
            Name = r,
            DisplayName = UserRoles.GetDisplayNameForRole(r),
            Permissions = UserRoles.GetPermissionsForRole(r).Select(p => new UserPermissionViewModel
            {
                Type = UserPermissionTypes.GetDisplayNameForPermissionType(p.Type),
                Level = p.Level
            })

        });

        await next();
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Pages.Users.AddUser;

namespace TeachingRecordSystem.SupportUi.Pages.Users;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class EditUser(
    TrsDbContext dbContext,
    IClock clock,
    TrsLinkGenerator linkGenerator) : PageModel
{
    private Core.DataStore.Postgres.Models.User? _user;

    [FromRoute]
    public Guid UserId { get; set; }

    [Display(Name = "Email address")]
    public string? Email { get; set; }

    [BindProperty]
    [Display(Name = "Name")]
    [Required(ErrorMessage = "Enter a name")]
    [MaxLength(Core.DataStore.Postgres.Models.User.NameMaxLength, ErrorMessage = "Name must be 200 characters or less")]
    public string? Name { get; set; }

    [BindProperty]
    [Display(Name = "Role")]
    [Required(ErrorMessage = "Select a role")]
    public string? Role { get; set; }

    public bool IsActiveUser { get; set; }

    public IEnumerable<UserRoleViewModel> RoleOptions { get; set; } = [];

    public Task OnGetAsync()
    {
        Name = _user!.Name;
        IsActiveUser = _user.Active;
        Role = _user.Role;

        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Ensure submitted roles is valid
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

        var user = await dbContext.Users.SingleAsync(u => u.UserId == UserId);

        var changes = UserUpdatedEventChanges.None |
            (user.Name != Name ? UserUpdatedEventChanges.Name : UserUpdatedEventChanges.None) |
            (user.Role != Role ? UserUpdatedEventChanges.Roles : UserUpdatedEventChanges.None);

        if (changes != UserUpdatedEventChanges.None)
        {
            user.Role = Role;
            user.Name = Name!;

            await dbContext.AddEventAndBroadcastAsync(new UserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                User = Core.Events.Models.User.FromModel(user),
                RaisedBy = User.GetUserId(),
                CreatedUtc = clock.UtcNow,
                Changes = changes
            });

            await dbContext.SaveChangesAsync();
        }

        if ((changes & UserUpdatedEventChanges.Roles) != UserUpdatedEventChanges.None)
        {
            var roleText = UserRoles.GetDisplayNameForRole(Role!)
                .ToLowerInvariantFirstLetter()
                .WithIndefiniteArticle();

            TempData.SetFlashSuccess(message: $"{Name} has been changed to {roleText}.");
        }
        else
        {
            TempData.SetFlashSuccess(message: $"{Name} has been updated.");
        }

        return Redirect(linkGenerator.Users());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _user = await dbContext.Users.SingleOrDefaultAsync(u => u.UserId == UserId);

        if (_user is null)
        {
            context.Result = NotFound();
            return;
        }

        Email = _user.Email;

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

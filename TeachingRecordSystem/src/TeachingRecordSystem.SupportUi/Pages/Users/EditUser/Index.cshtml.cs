using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Users.EditUser;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class IndexModel(
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
    [MaxLength(Core.DataStore.Postgres.Models.UserBase.NameMaxLength, ErrorMessage = "Name must be 200 characters or less")]
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

        var changes = UserUpdatedEventChanges.None |
            (_user!.Name != Name ? UserUpdatedEventChanges.Name : UserUpdatedEventChanges.None) |
            (_user.Role != Role ? UserUpdatedEventChanges.Roles : UserUpdatedEventChanges.None);

        if (changes != UserUpdatedEventChanges.None)
        {
            _user.Role = Role;
            _user.Name = Name!;

            await dbContext.AddEventAndBroadcastAsync(new UserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                User = EventModels.User.FromModel(_user),
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

            TempData.SetFlashSuccess(messageText: $"{Name} has been changed to {roleText}.");
        }
        else
        {
            TempData.SetFlashSuccess(messageText: $"{Name} has been updated.");
        }

        return Redirect(linkGenerator.Users());
    }

    public async Task<IActionResult> OnPostActivateAsync()
    {
        if (_user!.Active)
        {
            return BadRequest();
        }

        // Only admins can reactivate admins
        if (!User.IsInRole(UserRoles.Administrator) && _user.Role == UserRoles.Administrator)
        {
            return BadRequest();
        }

        _user.Active = true;

        await dbContext.AddEventAndBroadcastAsync(new UserActivatedEvent
        {
            EventId = Guid.NewGuid(),
            User = EventModels.User.FromModel(_user),
            RaisedBy = User.GetUserId(),
            CreatedUtc = clock.UtcNow
        });

        await dbContext.SaveChangesAsync();
        TempData.SetFlashSuccess(messageText: $"{_user.Name}\u2019s account has been reactivated.");

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

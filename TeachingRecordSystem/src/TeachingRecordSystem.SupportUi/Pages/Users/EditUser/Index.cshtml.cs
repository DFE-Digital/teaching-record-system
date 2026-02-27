using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Users.EditUser;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class IndexModel(
    TrsDbContext dbContext,
    TimeProvider timeProvider,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.Name)
            .NotEmpty().WithMessage("Enter a name")
            .MaximumLength(Core.DataStore.Postgres.Models.UserBase.NameMaxLength).WithMessage("Name must be 200 characters or less"),
        v => v.RuleFor(m => m.Role)
            .NotEmpty().WithMessage("Select a role")
    };

    private Core.DataStore.Postgres.Models.User? _user;

    [FromRoute]
    public Guid UserId { get; set; }

    public string? Email { get; set; }

    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
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

        _validator.ValidateAndThrow(this);

        var changes = UserUpdatedEventChanges.None |
            (_user!.Name != Name ? UserUpdatedEventChanges.Name : UserUpdatedEventChanges.None) |
            (_user.Role != Role ? UserUpdatedEventChanges.Roles : UserUpdatedEventChanges.None);

        if (changes != UserUpdatedEventChanges.None)
        {
            _user.Role = Role;
            _user.Name = Name!;

            dbContext.AddEventWithoutBroadcast(new UserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                User = EventModels.User.FromModel(_user),
                RaisedBy = User.GetUserId(),
                CreatedUtc = timeProvider.UtcNow,
                Changes = changes
            });

            await dbContext.SaveChangesAsync();
        }

        if ((changes & UserUpdatedEventChanges.Roles) != UserUpdatedEventChanges.None)
        {
            var roleText = UserRoles.GetDisplayNameForRole(Role!)
                .ToLowerInvariantFirstLetter()
                .WithIndefiniteArticle();

            TempData.SetFlashSuccess($"{Name} has been changed to {roleText}");
        }
        else
        {
            TempData.SetFlashSuccess($"{Name} has been updated.");
        }

        return Redirect(linkGenerator.Users.Index());
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

        dbContext.AddEventWithoutBroadcast(new UserActivatedEvent
        {
            EventId = Guid.NewGuid(),
            User = EventModels.User.FromModel(_user),
            RaisedBy = User.GetUserId(),
            CreatedUtc = timeProvider.UtcNow
        });

        await dbContext.SaveChangesAsync();
        TempData.SetFlashSuccess($"{_user.Name}\u2019s account has been reactivated");

        return Redirect(linkGenerator.Users.Index());
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

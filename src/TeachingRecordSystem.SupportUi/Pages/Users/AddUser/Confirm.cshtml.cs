using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Users;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

namespace TeachingRecordSystem.SupportUi.Pages.Users.AddUser;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class ConfirmModel(
    IAadUserService aadUserService,
    TimeProvider timeProvider,
    UserService userService,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<ConfirmModel> _validator = new()
    {
        v => v.RuleFor(m => m.Name)
            .NotEmpty().WithMessage("Enter a name")
            .MaximumLength(Core.DataStore.Postgres.Models.UserBase.NameMaxLength).WithMessage("Name must be 200 characters or less"),
        v => v.RuleFor(m => m.Role)
            .NotEmpty().WithMessage("Select a role")
    };

    private User? _user;

    [BindProperty(SupportsGet = true)]
    public string? UserId { get; set; }

    public string? Email { get; set; }

    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
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

        _validator.ValidateAndThrow(this);

        var processContext = new ProcessContext(ProcessType.UserAdding, timeProvider.UtcNow, User.GetUserId());

        await userService.CreateUserAsync(
            new CreateUserOptions
            {
                Name = Name!,
                Email = _user!.Email,
                AzureAdUserId = AzureAdUserId,
                Role = Role
            },
            processContext);

        var roleText = UserRoles.GetDisplayNameForRole(Role!)
            .ToLowerInvariantFirstLetter()
            .WithIndefiniteArticle();

        TempData.SetFlashNotificationBanner($"{Name} has been added as {roleText}.");
        return Redirect(linkGenerator.Users.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _user = await aadUserService.GetUserByIdAsync(UserId!);

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

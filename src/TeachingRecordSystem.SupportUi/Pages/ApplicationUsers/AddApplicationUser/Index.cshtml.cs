using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Users;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers.AddApplicationUser;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class IndexModel(SupportUiLinkGenerator linkGenerator, TimeProvider timeProvider, UserService userService) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.Name)
            .NotEmpty().WithMessage("Enter a name")
            .MaximumLength(UserBase.NameMaxLength).WithMessage("Name must be 200 characters or less"),

        v => v.RuleFor(m => m.ShortName)
            .MaximumLength(ApplicationUser.ShortNameMaxLength).WithMessage("Short name must be 25 characters or less")
    };

    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
    public string? ShortName { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _validator.ValidateAndThrow(this);

        var processContext = new ProcessContext(ProcessType.ApplicationUserCreating, timeProvider.UtcNow, User.GetUserId());

        ApplicationUser newUser;

        try
        {
            newUser = await userService.CreateApplicationUserAsync(
                new CreateApplicationUserOptions
                {
                    Name = Name!,
                    ShortName = ShortName
                },
                processContext);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueIndexViolation(ApplicationUser.NameUniqueIndexName))
        {
            ModelState.AddModelError(nameof(Name), "Name must be unique.");
            return this.PageWithErrors();
        }

        TempData.SetFlashNotificationBanner("Application user added");
        return Redirect(linkGenerator.ApplicationUsers.EditApplicationUser.Index(newUser.UserId));
    }
}

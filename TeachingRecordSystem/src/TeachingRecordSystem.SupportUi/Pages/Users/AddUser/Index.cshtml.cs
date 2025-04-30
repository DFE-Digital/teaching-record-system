using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

namespace TeachingRecordSystem.SupportUi.Pages.Users.AddUser;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
[RequireFeatureEnabledFilterFactory(FeatureNames.NewUserRoles)]
public class IndexModel(
    TrsDbContext dbContext,
    IAadUserService userService,
    TrsLinkGenerator trsLinkGenerator) : PageModel
{
    [Display(Name = "Email address")]
    [Required(ErrorMessage = "Enter an email address")]
    [BindProperty]
    public string? Email { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var email = Email!;
        if (!email.Contains('@'))
        {
            email += "@education.gov.uk";
        }

        var user = await userService.GetUserByEmailAsync(email);

        if (user is null)
        {
            ModelState.AddModelError(nameof(Email), "User does not exist");
            return this.PageWithErrors();
        }

        var existingUser = await dbContext.Users.SingleOrDefaultAsync(u => u.AzureAdUserId == user.UserId);
        if (existingUser is not null)
        {
            return Redirect(trsLinkGenerator.EditUser(existingUser.UserId));
        }

        return Redirect(trsLinkGenerator.AddUserConfirm(user.UserId));
    }
}

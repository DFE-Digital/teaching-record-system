using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

namespace TeachingRecordSystem.SupportUi.Pages.Users.AddUser;

[Authorize(Roles = UserRoles.Administrator)]
public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    private readonly TrsLinkGenerator _trsLinkGenerator;

    public IndexModel(IUserService userService, TrsLinkGenerator trsLinkGenerator)
    {
        _userService = userService;
        _trsLinkGenerator = trsLinkGenerator;
    }

    [Display(Name = "Email address")]
    [Required(ErrorMessage = "Enter an email address")]
    [BindProperty]
    public string? Email { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
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

        var user = await _userService.GetUserByEmail(email);

        if (user is null)
        {
            ModelState.AddModelError(nameof(Email), "User does not exist");
            return this.PageWithErrors();
        }

        return Redirect(_trsLinkGenerator.AddUser(user.UserId));
    }
}

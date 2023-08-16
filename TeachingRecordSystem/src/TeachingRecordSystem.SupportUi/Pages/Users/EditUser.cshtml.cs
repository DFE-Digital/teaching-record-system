using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

namespace TeachingRecordSystem.SupportUi.Pages.Users;

[Authorize(Roles = UserRoles.Administrator)]
public class EditUser : PageModel
{
    private readonly TrsDbContext _dbContext;
    private readonly IClock _clock;
    private readonly TrsLinkGenerator _linkGenerator;
    private Core.DataStore.Postgres.Models.User? _user;

    public EditUser(TrsDbContext dbContext,
        IUserService userService,
        IClock clock,
        TrsLinkGenerator linkGenerator)
    {
        _dbContext = dbContext;
        _clock = clock;
        _linkGenerator = linkGenerator;
    }

    [FromRoute]
    public Guid UserId { get; set; }

    public string? Email { get; set; }

    [BindProperty]
    [Display(Name = "Name")]
    [Required(ErrorMessage = "Enter a name")]
    [MaxLength(Core.DataStore.Postgres.Models.User.NameMaxLength, ErrorMessage = "Name must be 200 characters or less")]
    public string? Name { get; set; }

    [BindProperty]
    [Display(Name = "Roles")]
    public string[]? Roles { get; set; }

    public IActionResult OnGet()
    {
        Name = _user!.Name;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Roles?.Length == 0)
        {
            ModelState.AddModelError(nameof(Roles), "Select at least one role");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserId == UserId);

        var rolesChanged = Roles!.SequenceEqual(user!.Roles);
        var nameChanged = !Name!.Equals(user.Name, StringComparison.OrdinalIgnoreCase);

        if (!(rolesChanged | nameChanged))
        {
            return Redirect(_linkGenerator.Users());
        }

        user.Roles = Roles!;
        user.Name = Name;

        _dbContext.AddEvent(new UserEditedEvent
        {
            User = Core.Events.User.FromModel(user),
            AddedByUserId = User.GetUserId(),
            CreatedUtc = _clock.UtcNow
        });

        await _dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess("User updated");
        return Redirect(_linkGenerator.Users());

    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserId == UserId);

        if (_user is null)
        {
            context.Result = NotFound();
            return;
        }

        Email = _user.Email;

        await next();
    }
}

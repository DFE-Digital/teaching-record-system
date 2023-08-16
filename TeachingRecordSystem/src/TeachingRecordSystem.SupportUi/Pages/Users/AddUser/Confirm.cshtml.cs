using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

namespace TeachingRecordSystem.SupportUi.Pages.Users.AddUser;

[AuthorizeForScopes(Scopes = new[] { "User.ReadBasic.All" })]
public class ConfirmModel : PageModel
{
    private readonly TrsDbContext _dbContext;
    private readonly IAadUserService _userService;
    private readonly IClock _clock;
    private readonly TrsLinkGenerator _linkGenerator;
    private Services.AzureActiveDirectory.User? _user;

    public ConfirmModel(
        TrsDbContext dbContext,
        IAadUserService userService,
        IClock clock,
        TrsLinkGenerator linkGenerator)
    {
        _dbContext = dbContext;
        _userService = userService;
        _clock = clock;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public string? UserId { get; set; }

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
        var roles = Roles ?? Array.Empty<string>();

        // Ensure submitted roles are valid
        if (roles.Any(r => !UserRoles.All.Contains(r)))
        {
            return BadRequest();
        }

        if (roles.Length == 0)
        {
            ModelState.AddModelError(nameof(Roles), "Select at least one role");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var newUser = new Core.DataStore.Postgres.Models.User()
        {
            Active = true,
            AzureAdUserId = _user!.UserId,
            Email = _user.Email,
            Name = Name!,
            Roles = roles,
            UserId = Guid.NewGuid(),
            UserType = UserType.Person
        };

        _dbContext.Users.Add(newUser);

        _dbContext.AddEvent(new UserAddedEvent()
        {
            User = Core.Events.User.FromModel(newUser),
            AddedByUserId = User.GetUserId(),
            CreatedUtc = _clock.UtcNow
        });

        await _dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess("User added");
        return Redirect(_linkGenerator.Users());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _user = await _userService.GetUserById(UserId!);

        if (_user is null)
        {
            context.Result = NotFound();
            return;
        }

        Email = _user.Email;

        await next();
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.SupportUi.Pages.Users;

[Authorize(Roles = UserRoles.Administrator)]
public class EditUser : PageModel
{
    private readonly TrsDbContext _dbContext;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly IClock _clock;
    private readonly TrsLinkGenerator _linkGenerator;
    private Core.DataStore.Postgres.Models.User? _user;

    public EditUser(
        TrsDbContext dbContext,
        ICrmQueryDispatcher crmQueryDispatcher,
        IClock clock,
        TrsLinkGenerator linkGenerator)
    {
        _dbContext = dbContext;
        _crmQueryDispatcher = crmQueryDispatcher;
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

    public bool IsActiveUser { get; set; }

    public bool HasCrmAccount { get; set; }

    public bool CrmAccountIsDisabled { get; set; }

    public async Task OnGet()
    {
        Name = _user!.Name;
        IsActiveUser = _user.Active;
        Roles = _user.Roles;

        if (_user.AzureAdUserId is not null)
        {
            var crmUser = await _crmQueryDispatcher.ExecuteQuery(
                new GetSystemUserByAzureActiveDirectoryObjectIdQuery(
                    _user.AzureAdUserId, new ColumnSet(SystemUser.Fields.IsDisabled)));

            HasCrmAccount = crmUser is not null;
            CrmAccountIsDisabled = crmUser?.IsDisabled == true;
        }
        else
        {
            HasCrmAccount = false;
        }
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

        var user = await _dbContext.Users.SingleAsync(u => u.UserId == UserId);

        var changes = UserUpdatedEventChanges.None |
                      (user.Name != Name ? UserUpdatedEventChanges.Name : UserUpdatedEventChanges.None) |
                      (!user.Roles.SequenceEqual(Roles!) ? UserUpdatedEventChanges.Roles : UserUpdatedEventChanges.None);

        if (changes == UserUpdatedEventChanges.None)
        {
            return Redirect(_linkGenerator.Users());
        }

        user.Roles = Roles!;
        user.Name = Name!;

        _dbContext.AddEvent(new UserUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            User = Core.Events.User.FromModel(user),
            SourceUserId = User.GetUserId(),
            CreatedUtc = _clock.UtcNow,
            Changes = changes
        });

        await _dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess("User updated");
        return Redirect(_linkGenerator.Users());

    }

    public async Task<IActionResult> OnPostDeactivate()
    {
        var user = await _dbContext.Users.SingleAsync(u => u.UserId == UserId);

        if (!user.Active)
        {
            return BadRequest();
        }

        user.Active = false;

        _dbContext.AddEvent(new UserDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            User = Core.Events.User.FromModel(user),
            SourceUserId = User.GetUserId(),
            CreatedUtc = _clock.UtcNow
        });

        await _dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess("User deactivated");
        return Redirect(_linkGenerator.Users());
    }

    public async Task<IActionResult> OnPostActivate()
    {
        var user = await _dbContext.Users.SingleAsync(u => u.UserId == UserId);

        if (user.Active)
        {
            return BadRequest();
        }

        user.Active = true;

        _dbContext.AddEvent(new UserActivatedEvent
        {
            EventId = Guid.NewGuid(),
            User = Core.Events.User.FromModel(user),
            SourceUserId = User.GetUserId(),
            CreatedUtc = _clock.UtcNow
        });

        await _dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess("Activated");
        return Redirect(_linkGenerator.EditUser(UserId));
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

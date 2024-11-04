using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Users;

[Authorize(Roles = UserRoles.Administrator)]
public class EditUser(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
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
    [Display(Name = "Roles")]
    public string[]? Roles { get; set; }

    public string[]? DqtRoles { get; set; }

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
            var crmUserInfo = await crmQueryDispatcher.ExecuteQuery(
                new GetSystemUserByAzureActiveDirectoryObjectIdQuery(_user.AzureAdUserId));

            HasCrmAccount = crmUserInfo is not null;
            CrmAccountIsDisabled = crmUserInfo?.SystemUser.IsDisabled == true;
            DqtRoles = crmUserInfo?.Roles.Select(r => r.Name).OrderBy(n => n).ToArray();
        }
        else
        {
            HasCrmAccount = false;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        // Sanitize roles
        var newRoles = Roles!.Where(r => UserRoles.All.Contains(r)).ToArray();

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var user = await dbContext.Users.SingleAsync(u => u.UserId == UserId);

        var changes = UserUpdatedEventChanges.None |
            (user.Name != Name ? UserUpdatedEventChanges.Name : UserUpdatedEventChanges.None) |
            (!new HashSet<string>(user.Roles).SetEquals(new HashSet<string>(newRoles)) ? UserUpdatedEventChanges.Roles : UserUpdatedEventChanges.None);

        if (changes != UserUpdatedEventChanges.None)
        {
            user.Roles = newRoles;
            user.Name = Name!;

            dbContext.AddEvent(new UserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                User = Core.Events.Models.User.FromModel(user),
                RaisedBy = User.GetUserId(),
                CreatedUtc = clock.UtcNow,
                Changes = changes
            });

            await dbContext.SaveChangesAsync();
        }

        TempData.SetFlashSuccess("User updated");
        return Redirect(linkGenerator.Users());
    }

    public async Task<IActionResult> OnPostDeactivate()
    {
        var user = await dbContext.Users.SingleAsync(u => u.UserId == UserId);

        if (!user.Active)
        {
            return BadRequest();
        }

        user.Active = false;

        dbContext.AddEvent(new UserDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            User = Core.Events.Models.User.FromModel(user),
            RaisedBy = User.GetUserId(),
            CreatedUtc = clock.UtcNow
        });

        await dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess("User deactivated");
        return Redirect(linkGenerator.Users());
    }

    public async Task<IActionResult> OnPostActivate()
    {
        var user = await dbContext.Users.SingleAsync(u => u.UserId == UserId);

        if (user.Active)
        {
            return BadRequest();
        }

        user.Active = true;

        dbContext.AddEvent(new UserActivatedEvent
        {
            EventId = Guid.NewGuid(),
            User = Core.Events.Models.User.FromModel(user),
            RaisedBy = User.GetUserId(),
            CreatedUtc = clock.UtcNow
        });

        await dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess("User reactivated");
        return Redirect(linkGenerator.EditUser(UserId));
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

        await next();
    }
}

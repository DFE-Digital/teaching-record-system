using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

namespace TeachingRecordSystem.SupportUi.Pages.Users.AddUser;

[Authorize(Roles = UserRoles.Administrator)]
public class ConfirmModel(
    TrsDbContext dbContext,
    IAadUserService userService,
    ICrmQueryDispatcher crmQueryDispatcher,
    IClock clock,
    TrsLinkGenerator linkGenerator) : PageModel
{
    private Services.AzureActiveDirectory.User? _user;

    [BindProperty(SupportsGet = true)]
    public string? UserId { get; set; }

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

    public Guid? DqtUserId { get; set; }

    public string? AzureAdUserId { get; set; }

    public string[]? DqtRoles { get; set; }

    public bool HasCrmAccount { get; set; }

    public bool CrmAccountIsDisabled { get; set; }

    public IActionResult OnGet()
    {
        Name = _user!.Name;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var roles = Roles ?? [];

        // Ensure submitted roles are valid
        if (roles.Any(r => !UserRoles.All.Contains(r)))
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var newUser = new Core.DataStore.Postgres.Models.User()
        {
            Active = true,
            AzureAdUserId = AzureAdUserId,
            Email = _user!.Email,
            Name = Name!,
            Roles = roles,
            UserId = Guid.NewGuid(),
            DqtUserId = DqtUserId
        };

        dbContext.Users.Add(newUser);

        await dbContext.AddEventAndBroadcastAsync(new UserAddedEvent()
        {
            EventId = Guid.NewGuid(),
            User = Core.Events.Models.User.FromModel(newUser),
            RaisedBy = User.GetUserId(),
            CreatedUtc = clock.UtcNow
        });

        await dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess("User added");
        return Redirect(linkGenerator.Users());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _user = await userService.GetUserByIdAsync(UserId!);

        if (_user is null)
        {
            context.Result = NotFound();
            return;
        }

        Email = _user.Email;
        AzureAdUserId = _user.UserId;

        if (AzureAdUserId is not null)
        {
            var crmUserInfo = await crmQueryDispatcher.ExecuteQueryAsync(
                new GetSystemUserByAzureActiveDirectoryObjectIdQuery(AzureAdUserId));

            if (crmUserInfo is not null)
            {
                DqtUserId = crmUserInfo.SystemUser.Id;
                HasCrmAccount = true;
                CrmAccountIsDisabled = crmUserInfo.SystemUser.IsDisabled == true;
                DqtRoles = crmUserInfo.Roles.Select(r => r.Name).OrderBy(n => n).ToArray();
            }
        }

        await next();
    }
}

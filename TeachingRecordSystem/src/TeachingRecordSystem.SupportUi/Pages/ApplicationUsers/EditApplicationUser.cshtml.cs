using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class EditApplicationUserModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator, IClock clock) : PageModel
{
    private ApplicationUser? _user;

    [FromRoute]
    public Guid UserId { get; set; }

    [BindProperty]
    [Display(Name = "Name")]
    [Required(ErrorMessage = "Enter a name")]
    [MaxLength(UserBase.NameMaxLength, ErrorMessage = "Name must be 200 characters or less")]
    public string? Name { get; set; }

    [BindProperty]
    [Display(Name = "API roles")]
    public string[]? ApiRoles { get; set; }

    [Display(Name = "API keys")]
    public ApiKeyInfo[]? ApiKeys { get; set; }

    [BindProperty]
    [Display(Name = "Client ID")]
    [MaxLength(ApplicationUser.OneLoginClientIdMaxLength, ErrorMessage = "One Login Client ID must be 50 characters or less")]
    public string? OneLoginClientId { get; set; }

    [BindProperty]
    [Display(Name = "Private Key PEM")]
    public string? OneLoginPrivateKeyPem { get; set; }

    public void OnGet()
    {
        Name = _user!.Name;
        ApiRoles = _user.ApiRoles;
        OneLoginClientId = _user.OneLoginClientId;
        OneLoginPrivateKeyPem = _user.OneLoginPrivateKeyPem;
    }

    public async Task<IActionResult> OnPost()
    {
        // Sanitize roles
        var newApiRoles = ApiRoles!.Where(r => Core.ApiRoles.All.Contains(r)).ToArray();

        if (OneLoginClientId is not null && OneLoginPrivateKeyPem is null)
        {
            ModelState.AddModelError(nameof(OneLoginPrivateKeyPem), "One Login Private Key PEM is required if One Login Client ID is set");
        }

        if (OneLoginPrivateKeyPem is not null && OneLoginClientId is null)
        {
            ModelState.AddModelError(nameof(OneLoginClientId), "One Login Client ID is required if One Login Private Key PEM is set");
        }

        if (OneLoginPrivateKeyPem is not null && OneLoginClientId is not null)
        {
            try
            {
                RSA.Create().ImportFromPem(OneLoginPrivateKeyPem);
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError(nameof(OneLoginPrivateKeyPem), "One Login Private Key PEM is invalid");
            }
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var applicationUser = await dbContext.ApplicationUsers.SingleAsync(u => u.UserId == UserId);

        var changes = ApplicationUserUpdatedEventChanges.None |
            (Name != applicationUser.Name ? ApplicationUserUpdatedEventChanges.Name : 0) |
            (!new HashSet<string>(applicationUser.ApiRoles).SetEquals(new HashSet<string>(newApiRoles)) ? ApplicationUserUpdatedEventChanges.ApiRoles : 0) |
            (OneLoginClientId != applicationUser.OneLoginClientId ? ApplicationUserUpdatedEventChanges.OneLoginClientId : 0) |
            (OneLoginPrivateKeyPem != applicationUser.OneLoginPrivateKeyPem ? ApplicationUserUpdatedEventChanges.OneLoginPrivateKeyPem : 0);

        if (changes != ApplicationUserUpdatedEventChanges.None)
        {
            var oldApplicationUser = Core.Events.Models.ApplicationUser.FromModel(applicationUser);

            applicationUser.Name = Name!;
            applicationUser.ApiRoles = newApiRoles;
            applicationUser.OneLoginClientId = OneLoginClientId;
            applicationUser.OneLoginPrivateKeyPem = OneLoginPrivateKeyPem;

            var @event = new ApplicationUserUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = clock.UtcNow,
                RaisedBy = User.GetUserId(),
                ApplicationUser = Core.Events.Models.ApplicationUser.FromModel(applicationUser),
                OldApplicationUser = oldApplicationUser,
                Changes = changes
            };
            dbContext.AddEvent(@event);

            await dbContext.SaveChangesAsync();
        }

        TempData.SetFlashSuccess("Application user updated");
        return Redirect(linkGenerator.ApplicationUsers());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _user = await dbContext.ApplicationUsers
            .Include(r => r.ApiKeys)
            .SingleOrDefaultAsync(u => u.UserId == UserId);

        if (_user is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        ApiKeys = _user.ApiKeys
            .OrderBy(k => k.CreatedOn)
            .Select(k => new ApiKeyInfo(k.ApiKeyId, k.Key, k.Expires)).ToArray();

        await next();
    }

    public record ApiKeyInfo(Guid ApiKeyId, string Key, DateTime? Expires);
}

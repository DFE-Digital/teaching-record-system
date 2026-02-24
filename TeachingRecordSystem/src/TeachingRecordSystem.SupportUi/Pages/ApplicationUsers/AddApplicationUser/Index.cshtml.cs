using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers.AddApplicationUser;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class IndexModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator, IClock clock) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Enter a name")]
    [MaxLength(UserBase.NameMaxLength, ErrorMessage = "Name must be 200 characters or less")]
    public string? Name { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var newUser = new ApplicationUser()
        {
            ApiRoles = [],
            Name = Name!,
            UserId = Guid.NewGuid()
        };

        dbContext.ApplicationUsers.Add(newUser);

        dbContext.AddEventWithoutBroadcast(new ApplicationUserCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            ApplicationUser = EventModels.ApplicationUser.FromModel(newUser),
            RaisedBy = User.GetUserId(),
            CreatedUtc = clock.UtcNow
        });

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueIndexViolation(ApplicationUser.NameUniqueIndexName))
        {
            ModelState.AddModelError(nameof(Name), "Name must be unique.");
            return this.PageWithErrors();
        }

        TempData.SetFlashSuccess("Application user added");
        return Redirect(linkGenerator.ApplicationUsers.EditApplicationUser.Index(newUser.UserId));
    }
}

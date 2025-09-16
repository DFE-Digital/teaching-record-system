using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class AddApplicationUserModel(TrsDbContext dbContext, IEventPublisher eventPublisher, TrsLinkGenerator linkGenerator, IClock clock) : PageModel
{
    [BindProperty]
    [Display(Name = "Name")]
    [Required(ErrorMessage = "Enter a name")]
    [MaxLength(UserBase.NameMaxLength, ErrorMessage = "Name must be 200 characters or less")]
    public string? Name { get; set; }

    [BindProperty]
    [Display(Name = "Short name")]
    [MaxLength(ApplicationUser.ShortNameMaxLength, ErrorMessage = "Short name must be 25 characters or less")]
    public string? ShortName { get; set; }

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
            UserId = Guid.NewGuid(),
            ShortName = ShortName
        };

        dbContext.ApplicationUsers.Add(newUser);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueIndexViolation(ApplicationUser.NameUniqueIndexName))
        {
            ModelState.AddModelError(nameof(Name), "Name must be unique.");
            return this.PageWithErrors();
        }

        await eventPublisher.PublishEventAsync(new ApplicationUserCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            ApplicationUser = Core.Events.Models.ApplicationUser.FromModel(newUser),
            RaisedBy = User.GetUserId(),
            CreatedUtc = clock.UtcNow
        });

        TempData.SetFlashSuccess("Application user added");
        return Redirect(linkGenerator.EditApplicationUser(newUser.UserId));
    }
}

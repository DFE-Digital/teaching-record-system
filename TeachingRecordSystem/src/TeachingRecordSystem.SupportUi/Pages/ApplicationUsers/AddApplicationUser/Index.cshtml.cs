using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers.AddApplicationUser;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class IndexModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator, TimeProvider timeProvider) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.Name)
            .NotEmpty().WithMessage("Enter a name")
            .MaximumLength(UserBase.NameMaxLength).WithMessage("Name must be 200 characters or less")
    };

    [BindProperty]
    public string? Name { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _validator.ValidateAndThrow(this);

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
            CreatedUtc = timeProvider.UtcNow
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

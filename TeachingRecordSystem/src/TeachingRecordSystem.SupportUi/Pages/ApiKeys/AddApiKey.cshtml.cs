using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApiKeys;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class AddApiKeyModel(TrsDbContext dbContext, IClock clock, TrsLinkGenerator linkGenerator) : PageModel
{
    [FromQuery]
    public Guid ApplicationUserId { get; set; }

    public string? ApplicationUserName { get; set; }

    [BindProperty]
    [Display(Name = "Key")]
    [Required(ErrorMessage = "Enter a key")]
    [MaxLength(ApiKey.KeyMaxLength, ErrorMessage = "Key must be 100 characters or less")]
    [MinLength(ApiKey.KeyMinLength, ErrorMessage = "Key must be at least 16 characters")]
    public string? Key { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var apiKey = new ApiKey()
        {
            ApiKeyId = Guid.NewGuid(),
            CreatedOn = clock.UtcNow,
            UpdatedOn = clock.UtcNow,
            ApplicationUserId = ApplicationUserId,
            Key = Key!,
            Expires = null
        };

        dbContext.ApiKeys.Add(apiKey);

        var @event = new ApiKeyCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = User.GetUserId(),
            ApiKey = EventModels.ApiKey.FromModel(apiKey)
        };
        await dbContext.AddEventAndBroadcastAsync(@event);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueIndexViolation(ApiKey.KeyUniqueIndexName))
        {
            ModelState.AddModelError(nameof(Key), "Key is already in use");
            return this.PageWithErrors();
        }

        TempData.SetFlashSuccess("API key added");
        return Redirect(linkGenerator.EditApplicationUser(ApplicationUserId));
    }

    public async override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var user = await dbContext.ApplicationUsers.SingleOrDefaultAsync(a => a.UserId == ApplicationUserId);

        if (user is null)
        {
            context.Result = new BadRequestResult();
            return;
        }

        ApplicationUserName = user.Name;

        await next();
    }
}

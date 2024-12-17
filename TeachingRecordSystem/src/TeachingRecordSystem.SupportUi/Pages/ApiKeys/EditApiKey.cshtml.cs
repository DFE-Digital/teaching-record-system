using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApiKeys;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class EditApiKeyModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator, IClock clock) : PageModel
{
    private ApiKey? _apiKey;

    [FromRoute]
    public Guid ApiKeyId { get; set; }

    public Guid ApplicationUserId { get; set; }

    public string? ApplicationUserName { get; set; }

    public DateTime? Expires { get; set; }

    public string? Key { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostExpireAsync()
    {
        if (Expires.HasValue)
        {
            return BadRequest();
        }

        var oldApiKey = Core.Events.Models.ApiKey.FromModel(_apiKey!);

        _apiKey!.Expires = clock.UtcNow;

        var @event = new ApiKeyUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = User.GetUserId(),
            ApiKey = Core.Events.Models.ApiKey.FromModel(_apiKey),
            OldApiKey = oldApiKey,
            Changes = ApiKeyUpdatedEventChanges.Expires
        };
        await dbContext.AddEventAndBroadcastAsync(@event);

        await dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess("API key expired");
        return Redirect(linkGenerator.EditApplicationUser(ApplicationUserId));
    }

    public async override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _apiKey = await dbContext.ApiKeys
            .Include(k => k.ApplicationUser)
            .SingleOrDefaultAsync(k => k.ApiKeyId == ApiKeyId);

        if (_apiKey is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        ApplicationUserId = _apiKey.ApplicationUserId;
        ApplicationUserName = _apiKey.ApplicationUser.Name;
        Expires = _apiKey.Expires;
        Key = _apiKey.Key;

        await next();
    }
}

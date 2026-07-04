using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Users;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApiKeys.EditApiKey;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class IndexModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator, TimeProvider timeProvider, UserService userService) : PageModel
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

        var processContext = new ProcessContext(ProcessType.ApiKeyUpdating, timeProvider.UtcNow, User.GetUserId());

        await userService.UpdateApiKeyAsync(
            new UpdateApiKeyOptions
            {
                ApiKeyId = ApiKeyId,
                Expires = Option.Some<DateTime?>(timeProvider.UtcNow)
            },
            processContext);

        TempData.SetFlashNotificationBanner("API key expired");
        return Redirect(linkGenerator.ApplicationUsers.EditApplicationUser.Index(ApplicationUserId));
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
        ApplicationUserName = _apiKey.ApplicationUser!.Name;
        Expires = _apiKey.Expires;
        Key = _apiKey.Key;

        await next();
    }
}

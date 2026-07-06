using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Users;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApiKeys.AddApiKey;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class IndexModel(TrsDbContext dbContext, TimeProvider timeProvider, UserService userService, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.Key)
            .NotEmpty().WithMessage("Enter a key")
            .MaximumLength(ApiKey.KeyMaxLength).WithMessage("Key must be 100 characters or less")
            .MinimumLength(ApiKey.KeyMinLength).WithMessage("Key must be at least 16 characters")
    };

    [FromQuery]
    public Guid ApplicationUserId { get; set; }

    public string? ApplicationUserName { get; set; }

    [BindProperty]
    public string? Key { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _validator.ValidateAndThrow(this);

        var processContext = new ProcessContext(ProcessType.ApiKeyCreating, timeProvider.UtcNow, User.GetUserId());

        try
        {
            await userService.CreateApiKeyAsync(
                new CreateApiKeyOptions
                {
                    ApplicationUserId = ApplicationUserId,
                    Key = Key!
                },
                processContext);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueIndexViolation(ApiKey.KeyUniqueIndexName))
        {
            ModelState.AddModelError(nameof(Key), "Key is already in use");
            return this.PageWithErrors();
        }

        TempData.SetFlashNotificationBanner("API key added");
        return Redirect(linkGenerator.ApplicationUsers.EditApplicationUser.Index(ApplicationUserId));
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

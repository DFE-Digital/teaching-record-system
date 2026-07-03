using System.Buffers;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Users;
using TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers.EditApplicationUser;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
[BindProperties]
public class IndexModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator, TimeProvider timeProvider, UserService userService) : PageModel
{
    // From PathString
    private static readonly SearchValues<char> _validPathChars =
        SearchValues.Create("!$&'()*+,-./0123456789:;=@ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz~");

    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.Name)
            .NotEmpty().WithMessage("Enter a name")
            .MaximumLength(UserBase.NameMaxLength).WithMessage("Name must be 200 characters or less"),
        v => v.RuleFor(m => m.ClientId)
            .NotEmpty().WithMessage("Enter a client ID").When(m => m.IsOidcClient)
            .MaximumLength(ApplicationUser.ClientIdMaxLength).WithMessage("Client ID must be 50 characters or less").When(m => m.IsOidcClient),
        v => v.RuleFor(m => m.ClientSecret)
            .NotEmpty().WithMessage("Enter a client secret").When(m => m.IsOidcClient)
            .MinimumLength(ApplicationUser.ClientSecretMinLength).WithMessage("Client secret must be at least 16 characters").When(m => m.IsOidcClient)
            .MaximumLength(ApplicationUser.ClientSecretMaxLength).WithMessage("Client secret must be 200 characters or less").When(m => m.IsOidcClient),
        v => v.RuleFor(m => m.OneLoginAuthenticationSchemeName)
            .NotEmpty().WithMessage("Enter an authentication scheme name").When(m => m.IsOidcClient)
            .MaximumLength(ApplicationUser.AuthenticationSchemeNameMaxLength).WithMessage("Authentication scheme name must be 50 characters or less").When(m => m.IsOidcClient),
        v => v.RuleFor(m => m.OneLoginClientId)
            .NotEmpty().WithMessage("Enter the One Login client ID").When(m => m.IsOidcClient)
            .MaximumLength(ApplicationUser.OneLoginClientIdMaxLength).WithMessage("One Login client ID must be 50 characters or less").When(m => m.IsOidcClient),
        v => v.RuleFor(m => m.UseSharedOneLoginSigningKeys)
            .NotNull().WithMessage("Select whether to use shared One Login signing keys").When(m => m.IsOidcClient),
        v => v.RuleFor(m => m.OneLoginRedirectUriPath)
            .NotEmpty().WithMessage("Enter the One Login redirect URI").When(m => m.IsOidcClient)
            .MaximumLength(ApplicationUser.RedirectUriPathMaxLength).WithMessage("One Login redirect URI must be 150 characters or less").When(m => m.IsOidcClient),
        v => v.RuleFor(m => m.OneLoginPostLogoutRedirectUriPath)
            .NotEmpty().WithMessage("Enter the One Login post logout redirect URI").When(m => m.IsOidcClient)
            .MaximumLength(ApplicationUser.RedirectUriPathMaxLength).WithMessage("One Login post logout redirect URI must be 150 characters or less").When(m => m.IsOidcClient),
        v => v.RuleFor(m => m.ShortName)
            .MaximumLength(ApplicationUser.ShortNameMaxLength).WithMessage("Short name must be 25 characters or less"),
        v => v.RuleFor(m => m.SupportEmailAddressNotifyId)
            .Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _)).WithMessage("Support email address Notify ID must be a valid GUID")
            .When(m => m.IsOidcClient),
        v => v.RuleFor(m => m.SupportEmailAddress)
            .EmailAddress().WithMessage("Enter a valid email address")
            .When(m => m.IsOidcClient && !string.IsNullOrEmpty(m.SupportEmailAddress))
    };

    private ApplicationUser? _user;

    [FromRoute]
    public Guid UserId { get; set; }

    public string? Name { get; set; }

    public string[]? ApiRoles { get; set; }

    [BindNever]
    public ApiKeyInfo[]? ApiKeys { get; set; }

    public bool IsOidcClient { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    [ModelBinder(BinderType = typeof(MultiLineStringModelBinder))]
    public string[]? RedirectUris { get; set; }

    [ModelBinder(BinderType = typeof(MultiLineStringModelBinder))]
    public string[]? PostLogoutRedirectUris { get; set; }

    public string? OneLoginAuthenticationSchemeName { get; set; }

    public string? OneLoginClientId { get; set; }

    public bool? UseSharedOneLoginSigningKeys { get; set; }

    public string? OneLoginPrivateKeyPem { get; set; }

    public string? OneLoginRedirectUriPath { get; set; }

    public string? OneLoginPostLogoutRedirectUriPath { get; set; }

    public RecordMatchingPolicy RecordMatchingPolicy { get; set; }

    public string? OneLoginCannotFindRecordEmailTemplateId { get; set; }

    public string? OneLoginNotVerifiedEmailTemplateId { get; set; }

    public string? OneLoginRecordMatchedEmailTemplateId { get; set; }

    public string? OneLoginNotConnectedEmailTemplateId { get; set; }

    public string? OneLoginNoMatchesPageContentHtml { get; set; }

    public string? OneLoginNoMatchesEmailSentFlashMessage { get; set; }

    public string? OneLoginNotConnectedEmailSentFlashMessage { get; set; }

    public string? OneLoginFoundPageLinkText { get; set; }

    public string? ShortName { get; set; }

    public string? SignInUrl { get; set; }

    public string? SupportEmailAddressNotifyId { get; set; }

    public string? SupportEmailAddress { get; set; }

    public void OnGet()
    {
        Name = _user!.Name;
        ApiRoles = _user.ApiRoles;
        IsOidcClient = _user.IsOidcClient;
        ClientId = _user.ClientId;
        ClientSecret = _user.ClientSecret;
        RedirectUris = _user.RedirectUris?.ToArray() ?? [];
        PostLogoutRedirectUris = _user.PostLogoutRedirectUris?.ToArray() ?? [];
        OneLoginAuthenticationSchemeName = _user.OneLoginAuthenticationSchemeName;
        OneLoginClientId = _user.OneLoginClientId;
        UseSharedOneLoginSigningKeys = _user.UseSharedOneLoginSigningKeys;
        OneLoginPrivateKeyPem = _user.OneLoginPrivateKeyPem;
        OneLoginRedirectUriPath = _user.OneLoginRedirectUriPath;
        OneLoginPostLogoutRedirectUriPath = _user.OneLoginPostLogoutRedirectUriPath;
        RecordMatchingPolicy = _user.RecordMatchingPolicy;
        OneLoginCannotFindRecordEmailTemplateId = _user.AppContent?.OneLoginCannotFindRecordEmailTemplateId;
        OneLoginNotVerifiedEmailTemplateId = _user.AppContent?.OneLoginNotVerifiedEmailTemplateId;
        OneLoginRecordMatchedEmailTemplateId = _user.AppContent?.OneLoginRecordMatchedEmailTemplateId;
        OneLoginNotConnectedEmailTemplateId = _user.AppContent?.OneLoginNotConnectedEmailTemplateId;
        OneLoginNoMatchesPageContentHtml = _user.AppContent?.OneLoginNoMatchesPageContentHtml;
        OneLoginNoMatchesEmailSentFlashMessage = _user.AppContent?.OneLoginNoMatchesEmailSentFlashMessage;
        OneLoginNotConnectedEmailSentFlashMessage = _user.AppContent?.OneLoginNotConnectedEmailSentFlashMessage;
        OneLoginFoundPageLinkText = _user.AppContent?.OneLoginFoundPageLinkText;
        ShortName = _user.ShortName;
        SignInUrl = _user.AppContent?.SignInUrl;
        SupportEmailAddressNotifyId = _user.AppContent?.SupportEmailAddressNotifyId;
        SupportEmailAddress = _user.AppContent?.SupportEmailAddress;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Sanitize roles
        var newApiRoles = ApiRoles!.Where(r => Core.ApiRoles.All.Contains(r)).ToArray();

        if (IsOidcClient)
        {
            foreach (var redirectUri in RedirectUris!)
            {
                if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != "http" && uri.Scheme != "https"))
                {
                    ModelState.AddModelError(nameof(RedirectUris), "One or more redirect URIs are not valid");
                    break;
                }
            }

            foreach (var redirectUri in PostLogoutRedirectUris!)
            {
                if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != "http" && uri.Scheme != "https"))
                {
                    ModelState.AddModelError(nameof(PostLogoutRedirectUris), "One or more post logout redirect URIs are not valid");
                    break;
                }
            }

            if (UseSharedOneLoginSigningKeys == false)
            {
                if (string.IsNullOrEmpty(OneLoginPrivateKeyPem))
                {
                    ModelState.AddModelError(nameof(OneLoginPrivateKeyPem), "Enter the One Login private key");
                }
                else
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
            }
        }

        _validator.ValidateAndThrow(this);

        if (IsOidcClient)
        {
            if (ModelState[nameof(OneLoginRedirectUriPath)]!.Errors.Count == 0 &&
                !OneLoginRedirectUriPath!.All(c => _validPathChars.Contains(c)))
            {
                ModelState.AddModelError(nameof(OneLoginRedirectUriPath), "Enter a valid redirect URI path");
            }

            if (ModelState[nameof(OneLoginPostLogoutRedirectUriPath)]!.Errors.Count == 0 &&
                !OneLoginPostLogoutRedirectUriPath!.All(c => _validPathChars.Contains(c)))
            {
                ModelState.AddModelError(nameof(OneLoginPostLogoutRedirectUriPath), "Enter a valid post logout redirect URI path");
            }
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        AppContent? newAppContent = null;

        if (IsOidcClient)
        {
            newAppContent = new AppContent
            {
                OneLoginCannotFindRecordEmailTemplateId = OneLoginCannotFindRecordEmailTemplateId,
                OneLoginNotVerifiedEmailTemplateId = OneLoginNotVerifiedEmailTemplateId,
                OneLoginRecordMatchedEmailTemplateId = OneLoginRecordMatchedEmailTemplateId,
                OneLoginNotConnectedEmailTemplateId = OneLoginNotConnectedEmailTemplateId,
                OneLoginNoMatchesPageContentHtml = OneLoginNoMatchesPageContentHtml,
                OneLoginNoMatchesEmailSentFlashMessage = OneLoginNoMatchesEmailSentFlashMessage,
                OneLoginNotConnectedEmailSentFlashMessage = OneLoginNotConnectedEmailSentFlashMessage,
                OneLoginFoundPageLinkText = OneLoginFoundPageLinkText,
                SignInUrl = SignInUrl,
                SupportEmailAddressNotifyId = SupportEmailAddressNotifyId,
                SupportEmailAddress = SupportEmailAddress
            };
        }

        var options = new UpdateApplicationUserOptions
        {
            UserId = UserId,
            Name = Option.Some(Name!),
            ShortName = Option.Some(ShortName),
            ApiRoles = Option.Some<string[]?>(newApiRoles),
            IsOidcClient = Option.Some(IsOidcClient)
        };

        if (IsOidcClient)
        {
            options = options with
            {
                ClientId = Option.Some(ClientId),
                ClientSecret = Option.Some(ClientSecret),
                RedirectUris = Option.Some<IReadOnlyCollection<string>?>(RedirectUris),
                PostLogoutRedirectUris = Option.Some<IReadOnlyCollection<string>?>(PostLogoutRedirectUris),
                OneLoginClientId = Option.Some(OneLoginClientId),
                UseSharedOneLoginSigningKeys = Option.Some<bool?>(UseSharedOneLoginSigningKeys),
                OneLoginAuthenticationSchemeName = Option.Some(OneLoginAuthenticationSchemeName),
                OneLoginRedirectUriPath = Option.Some(OneLoginRedirectUriPath),
                OneLoginPostLogoutRedirectUriPath = Option.Some(OneLoginPostLogoutRedirectUriPath),
                RecordMatchingPolicy = Option.Some(RecordMatchingPolicy),
                AppContent = Option.Some<AppContent?>(newAppContent)
            };

            if (UseSharedOneLoginSigningKeys == false)
            {
                options = options with { OneLoginPrivateKeyPem = Option.Some(OneLoginPrivateKeyPem) };
            }
        }

        var processContext = new ProcessContext(ProcessType.ApplicationUserUpdating, timeProvider.UtcNow, User.GetUserId());

        await userService.UpdateApplicationUserAsync(options, processContext);

        TempData.SetFlashNotificationBanner("Application user updated");
        return Redirect(linkGenerator.ApplicationUsers.Index());
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

        ApiKeys = _user.ApiKeys!
            .OrderBy(k => k.CreatedOn)
            .Select(k => new ApiKeyInfo(k.ApiKeyId, k.Key, k.Expires)).ToArray();

        await next();
    }

    public record ApiKeyInfo(Guid ApiKeyId, string Key, DateTime? Expires);
}

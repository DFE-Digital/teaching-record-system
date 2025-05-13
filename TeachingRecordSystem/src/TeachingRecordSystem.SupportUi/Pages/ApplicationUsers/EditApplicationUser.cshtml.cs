using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
[BindProperties]
public class EditApplicationUserModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator, IClock clock) : PageModel
{
    // From PathString
    private static readonly SearchValues<char> _validPathChars =
        SearchValues.Create("!$&'()*+,-./0123456789:;=@ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz~");

    private ApplicationUser? _user;

    [FromRoute]
    public Guid UserId { get; set; }

    [Display(Name = "Name")]
    [Required(ErrorMessage = "Enter a name")]
    [MaxLength(UserBase.NameMaxLength, ErrorMessage = "Name must be 200 characters or less")]
    public string? Name { get; set; }

    [BindProperty]
    [Display(Name = "Short name")]
    [MaxLength(ApplicationUser.ShortNameMaxLength, ErrorMessage = "Short name must be 25 characters or less")]
    public string? ShortName { get; set; }

    [Display(Name = "API roles")]
    public string[]? ApiRoles { get; set; }

    [BindNever]
    [Display(Name = "API keys")]
    public ApiKeyInfo[]? ApiKeys { get; set; }

    [Display(Name = "OIDC client")]
    public bool IsOidcClient { get; set; }

    [Display(Name = "Client ID")]
    [Required(ErrorMessage = "Enter a client ID")]
    [MaxLength(ApplicationUser.ClientIdMaxLength, ErrorMessage = "Client ID must be 50 characters or less")]
    public string? ClientId { get; set; }

    [Display(Name = "Client secret")]
    [Required(ErrorMessage = "Enter a client secret")]
    [MinLength(ApplicationUser.ClientSecretMinLength, ErrorMessage = "Client secret must be at least 16 characters")]
    [MaxLength(ApplicationUser.ClientSecretMaxLength, ErrorMessage = "Client secret must be 200 characters or less")]
    public string? ClientSecret { get; set; }

    [Display(Name = "Redirect URIs", Description = "Enter one per line")]
    [ModelBinder(BinderType = typeof(MultiLineStringModelBinder))]
    public string[]? RedirectUris { get; set; }

    [Display(Name = "Post logout redirect URIs", Description = "Enter one per line")]
    [ModelBinder(BinderType = typeof(MultiLineStringModelBinder))]
    public string[]? PostLogoutRedirectUris { get; set; }

    [Display(Name = "Authentication scheme name")]
    [Required(ErrorMessage = "Enter an authentication scheme name")]
    [MaxLength(ApplicationUser.AuthenticationSchemeNameMaxLength, ErrorMessage = "Authentication scheme name must be 50 characters or less")]
    public string? OneLoginAuthenticationSchemeName { get; set; }

    [Display(Name = "One Login client ID")]
    [Required(ErrorMessage = "Enter the One Login client ID")]
    [MaxLength(ApplicationUser.OneLoginClientIdMaxLength, ErrorMessage = "One Login client ID must be 50 characters or less")]
    public string? OneLoginClientId { get; set; }

    [Display(Name = "One Login private key", Description = "Enter a key in the PEM format")]
    [Required(ErrorMessage = "Enter the One Login private key")]
    public string? OneLoginPrivateKeyPem { get; set; }

    [Display(Name = "One Login redirect URI path")]
    [Required(ErrorMessage = "Enter the One Login redirect URI")]
    [MaxLength(ApplicationUser.RedirectUriPathMaxLength, ErrorMessage = "One Login redirect URI must be 100 characters or less")]
    public string? OneLoginRedirectUriPath { get; set; }

    [Display(Name = "One Login post logout redirect URI path")]
    [Required(ErrorMessage = "Enter the One Login post logout redirect URI")]
    [MaxLength(ApplicationUser.RedirectUriPathMaxLength, ErrorMessage = "One Login post logout redirect URI must be 100 characters or less")]
    public string? OneLoginPostLogoutRedirectUriPath { get; set; }

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
        OneLoginPrivateKeyPem = _user.OneLoginPrivateKeyPem;
        OneLoginRedirectUriPath = _user.OneLoginRedirectUriPath;
        OneLoginPostLogoutRedirectUriPath = _user.OneLoginPostLogoutRedirectUriPath;
        ShortName = _user.ShortName;
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

            if (ModelState[nameof(OneLoginPrivateKeyPem)]!.Errors.Count == 0)
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
        else
        {
            // Clear any errors for any OIDC-related fields (since we're not saving them if IsOidcClient is false)
            foreach (var key in ModelState.Keys)
            {
                if (key.StartsWith("OneLogin") ||
                    key is nameof(ClientId) or nameof(ClientSecret) or nameof(RedirectUris) or nameof(PostLogoutRedirectUris))
                {
                    ModelState.Remove(key);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        string? flashMessage = null;

        var changes = ApplicationUserUpdatedEventChanges.None |
            (Name != _user!.Name ? ApplicationUserUpdatedEventChanges.Name : 0) |
            (ShortName != _user!.ShortName ? ApplicationUserUpdatedEventChanges.ShortName : 0) |
            (!new HashSet<string>(_user.ApiRoles ?? []).SetEquals(new HashSet<string>(newApiRoles)) ? ApplicationUserUpdatedEventChanges.ApiRoles : 0) |
            (IsOidcClient != _user.IsOidcClient ? ApplicationUserUpdatedEventChanges.IsOidcClient : 0);

        if (IsOidcClient)
        {
            var oldChanges = changes;

            changes |=
                (ClientId != _user.ClientId ? ApplicationUserUpdatedEventChanges.ClientId : 0) |
                (ClientSecret != _user.ClientSecret ? ApplicationUserUpdatedEventChanges.ClientSecret : 0) |
                (!RedirectUris!.SequenceEqualIgnoringOrder(_user.RedirectUris ?? []) ? ApplicationUserUpdatedEventChanges.RedirectUris : 0) |
                (!PostLogoutRedirectUris!.SequenceEqualIgnoringOrder(_user.PostLogoutRedirectUris ?? []) ? ApplicationUserUpdatedEventChanges.PostLogoutRedirectUris : 0) |
                (OneLoginClientId != _user.OneLoginClientId ? ApplicationUserUpdatedEventChanges.OneLoginClientId : 0) |
                (OneLoginPrivateKeyPem != _user.OneLoginPrivateKeyPem ? ApplicationUserUpdatedEventChanges.OneLoginPrivateKeyPem : 0) |
                (OneLoginAuthenticationSchemeName != _user.OneLoginAuthenticationSchemeName ? ApplicationUserUpdatedEventChanges.OneLoginAuthenticationSchemeName : 0) |
                (OneLoginRedirectUriPath != _user.OneLoginRedirectUriPath ? ApplicationUserUpdatedEventChanges.OneLoginRedirectUriPath : 0) |
                (OneLoginPostLogoutRedirectUriPath != _user.OneLoginPostLogoutRedirectUriPath ? ApplicationUserUpdatedEventChanges.OneLoginPostLogoutRedirectUriPath : 0);

            var oneLoginPropertyHasChanged = changes != oldChanges;
            if (oneLoginPropertyHasChanged)
            {
                flashMessage = "Changes to One Login configuration make take a few minutes before they’re updated everywhere.";
            }
        }

        if (changes != ApplicationUserUpdatedEventChanges.None)
        {
            var oldApplicationUser = EventModels.ApplicationUser.FromModel(_user);

            _user.Name = Name!;
            _user.ShortName = ShortName;
            _user.ApiRoles = newApiRoles;
            _user.IsOidcClient = IsOidcClient;

            if (IsOidcClient)
            {
                _user.IsOidcClient = IsOidcClient;
                _user.ClientId = ClientId;
                _user.ClientSecret = ClientSecret;
                _user.RedirectUris = [.. RedirectUris!];
                _user.PostLogoutRedirectUris = [.. PostLogoutRedirectUris!];
                _user.OneLoginAuthenticationSchemeName = OneLoginAuthenticationSchemeName;
                _user.OneLoginClientId = OneLoginClientId;
                _user.OneLoginPrivateKeyPem = OneLoginPrivateKeyPem;
                _user.OneLoginRedirectUriPath = OneLoginRedirectUriPath;
                _user.OneLoginPostLogoutRedirectUriPath = OneLoginPostLogoutRedirectUriPath;
            }

            var @event = new ApplicationUserUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = clock.UtcNow,
                RaisedBy = User.GetUserId(),
                ApplicationUser = EventModels.ApplicationUser.FromModel(_user),
                OldApplicationUser = oldApplicationUser,
                Changes = changes
            };
            await dbContext.AddEventAndBroadcastAsync(@event);

            await dbContext.SaveChangesAsync();
        }

        TempData.SetFlashSuccess("Application user updated", messageText: flashMessage);
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

        ApiKeys = _user.ApiKeys!
            .OrderBy(k => k.CreatedOn)
            .Select(k => new ApiKeyInfo(k.ApiKeyId, k.Key, k.Expires)).ToArray();

        await next();
    }

    public record ApiKeyInfo(Guid ApiKeyId, string Key, DateTime? Expires);
}

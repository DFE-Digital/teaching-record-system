using System.Buffers;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers.EditApplicationUser;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
[BindProperties]
public class IndexModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator, TimeProvider timeProvider) : PageModel
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
            .MaximumLength(ApplicationUser.RedirectUriPathMaxLength).WithMessage("One Login redirect URI must be 100 characters or less").When(m => m.IsOidcClient),
        v => v.RuleFor(m => m.OneLoginPostLogoutRedirectUriPath)
            .NotEmpty().WithMessage("Enter the One Login post logout redirect URI").When(m => m.IsOidcClient)
            .MaximumLength(ApplicationUser.RedirectUriPathMaxLength).WithMessage("One Login post logout redirect URI must be 100 characters or less").When(m => m.IsOidcClient)
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

        var changes = ApplicationUserUpdatedEventChanges.None |
            (Name != _user!.Name ? ApplicationUserUpdatedEventChanges.Name : 0) |
            (!new HashSet<string>(_user.ApiRoles ?? []).SetEquals(new HashSet<string>(newApiRoles)) ? ApplicationUserUpdatedEventChanges.ApiRoles : 0) |
            (IsOidcClient != _user.IsOidcClient ? ApplicationUserUpdatedEventChanges.IsOidcClient : 0);

        if (IsOidcClient)
        {
            changes |=
                (ClientId != _user.ClientId ? ApplicationUserUpdatedEventChanges.ClientId : 0) |
                (ClientSecret != _user.ClientSecret ? ApplicationUserUpdatedEventChanges.ClientSecret : 0) |
                (!RedirectUris!.SequenceEqualIgnoringOrder(_user.RedirectUris ?? []) ? ApplicationUserUpdatedEventChanges.RedirectUris : 0) |
                (!PostLogoutRedirectUris!.SequenceEqualIgnoringOrder(_user.PostLogoutRedirectUris ?? []) ? ApplicationUserUpdatedEventChanges.PostLogoutRedirectUris : 0) |
                (OneLoginClientId != _user.OneLoginClientId ? ApplicationUserUpdatedEventChanges.OneLoginClientId : 0) |
                (UseSharedOneLoginSigningKeys is false && OneLoginPrivateKeyPem != _user.OneLoginPrivateKeyPem ? ApplicationUserUpdatedEventChanges.OneLoginPrivateKeyPem : 0) |
                (OneLoginAuthenticationSchemeName != _user.OneLoginAuthenticationSchemeName ? ApplicationUserUpdatedEventChanges.OneLoginAuthenticationSchemeName : 0) |
                (OneLoginRedirectUriPath != _user.OneLoginRedirectUriPath ? ApplicationUserUpdatedEventChanges.OneLoginRedirectUriPath : 0) |
                (OneLoginPostLogoutRedirectUriPath != _user.OneLoginPostLogoutRedirectUriPath ? ApplicationUserUpdatedEventChanges.OneLoginPostLogoutRedirectUriPath : 0) |
                (UseSharedOneLoginSigningKeys != _user.UseSharedOneLoginSigningKeys ? ApplicationUserUpdatedEventChanges.UseSharedOneLoginSigningKeys : 0);
        }

        if (changes != ApplicationUserUpdatedEventChanges.None)
        {
            var oldApplicationUser = EventModels.ApplicationUser.FromModel(_user);

            _user.Name = Name!;
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
                _user.UseSharedOneLoginSigningKeys = UseSharedOneLoginSigningKeys!.Value;
                if (UseSharedOneLoginSigningKeys == false)
                {
                    _user.OneLoginPrivateKeyPem = OneLoginPrivateKeyPem;
                }
                _user.OneLoginRedirectUriPath = OneLoginRedirectUriPath;
                _user.OneLoginPostLogoutRedirectUriPath = OneLoginPostLogoutRedirectUriPath;
            }

            var @event = new ApplicationUserUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = timeProvider.UtcNow,
                RaisedBy = User.GetUserId(),
                ApplicationUser = EventModels.ApplicationUser.FromModel(_user),
                OldApplicationUser = oldApplicationUser,
                Changes = changes
            };
            dbContext.AddEventWithoutBroadcast(@event);

            await dbContext.SaveChangesAsync();

            // Notify TeacherAuth about changes to the application user
            await dbContext.Database.ExecuteSqlRawAsync($"NOTIFY {ChannelNames.OneLoginClient}");
        }

        TempData.SetFlashSuccess("Application user updated");
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

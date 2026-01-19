using System.Diagnostics;
using System.Security.Claims;
using System.Transactions;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.OneLogin;
using static TeachingRecordSystem.AuthorizeAccess.IdModelTypes;

namespace TeachingRecordSystem.AuthorizeAccess;

[JourneyCoordinator(JourneyName, routeValueKeys: [])]
[UsedImplicitly]
public class SignInJourneyCoordinator(
    TrsDbContext dbContext,
    OneLoginService oneLoginService,
    IdDbContext idDbContext,
    AuthorizeAccessLinkGenerator linkGenerator,
    IOptions<AuthorizeAccessOptions> optionsAccessor,
    IClock clock) :
    JourneyCoordinator<SignInJourneyState>
{
    public const string JourneyName = "SignInJourney";

    public static class Vtrs
    {
        public const string AuthenticationOnly = "Cl.Cm";
        public const string AuthenticationAndIdentityVerification = "Cl.Cm.P2";
    }

    // ID's database has a user_id column to indicate that a TRN token has been used already.
    // This sentinel value indicates the token has been used by us, rather than a teacher ID user.
    private static readonly Guid _teacherAuthIdUserIdSentinel = Guid.Empty;

    private LinkHelper? _linkHelper;

    public LinkHelper Links => _linkHelper ??= new LinkHelper(linkGenerator, InstanceId);

    public bool ShowDebugPages => optionsAccessor.Value.ShowDebugPages;

    public AdvanceToResult AdvanceTo(Func<LinkHelper, string> getUrl, PushStepOptions pushStepOptions = default) => AdvanceTo(getUrl(Links), pushStepOptions);

    public async Task<AdvanceToResult> AdvanceToAsync(Func<LinkHelper, Task<string>> getUrl, PushStepOptions pushStepOptions = default)
    {
        var url = await getUrl(Links);
        return AdvanceTo(url, pushStepOptions);
    }

    public IResult SignInWithOneLogin() =>
        OneLoginChallenge(Vtrs.AuthenticationOnly);

    public IResult VerifyIdentityWithOneLogin() =>
        OneLoginChallenge(Vtrs.AuthenticationAndIdentityVerification);

    public async Task<IResult> OnOneLoginCallbackAsync(AuthenticationTicket ticket)
    {
        if (!ticket.Properties.TryGetVectorsOfTrust(out var vtr))
        {
            throw new InvalidOperationException("No vtr.");
        }

        if (vtr.SequenceEqual([Vtrs.AuthenticationOnly]))
        {
            await OnUserAuthenticatedAsync(ticket);
        }
        else
        {
            Debug.Assert(vtr.SequenceEqual([Vtrs.AuthenticationAndIdentityVerification]));
            Debug.Assert(State.OneLoginAuthenticationTicket is not null);
            await OnUserVerifiedAsync(ticket);
        }

        if (ShowDebugPages)
        {
            return SquashPathAndAdvanceTo(Links.DebugIdentity());
        }

        return GetNextPage();
    }

    public async Task OnUserAuthenticatedAsync(AuthenticationTicket ticket)
    {
        var sub = ticket.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");
        var email = ticket.Principal.FindFirstValue("email") ?? throw new InvalidOperationException("No email claim.");

        var oneLoginUser = await dbContext.OneLoginUsers
            .Include(u => u.Person)
            .SingleOrDefaultAsync(u => u.Subject == sub);

        if (oneLoginUser is null)
        {
            oneLoginUser = new()
            {
                Subject = sub,
                EmailAddress = email,
                FirstOneLoginSignIn = clock.UtcNow,
                LastOneLoginSignIn = clock.UtcNow
            };
            dbContext.OneLoginUsers.Add(oneLoginUser);
        }
        else
        {
            oneLoginUser.FirstOneLoginSignIn ??= clock.UtcNow;
            oneLoginUser.LastOneLoginSignIn = clock.UtcNow;

            // Email may have changed since the last sign in or we may have never had it (e.g. if we got the user ID over our API).
            // TODO Should we emit an event if it has changed?
            oneLoginUser.EmailAddress = email;

            if (oneLoginUser.PersonId is not null)
            {
                oneLoginUser.FirstSignIn ??= clock.UtcNow;
                oneLoginUser.LastSignIn = clock.UtcNow;
            }
        }

        string? trn = oneLoginUser.Person?.Trn;

        if (oneLoginUser.PersonId is null &&
            await TryMatchToTrnRequestAsync(oneLoginUser) is { } matchResult)
        {
            trn = matchResult.Trn;
            oneLoginUser.FirstSignIn ??= clock.UtcNow;
            oneLoginUser.LastSignIn = clock.UtcNow;
        }

        await dbContext.SaveChangesAsync();

        UpdateState(state =>
        {
            state.Reset();
            state.OneLoginAuthenticationTicket = ticket;

            if (oneLoginUser.VerificationRoute is not null)
            {
                Debug.Assert(oneLoginUser.VerifiedNames is not null);
                Debug.Assert(oneLoginUser.VerifiedDatesOfBirth is not null);
                state.SetVerified(oneLoginUser.VerifiedNames!, oneLoginUser.VerifiedDatesOfBirth!);
            }

            if (trn is not null && !ShowDebugPages)
            {
                Complete(state, trn);
            }
        });
    }

    public Task OnUserVerifiedAsync(AuthenticationTicket ticket)
    {
        var verifiedNames = ticket.Principal.GetCoreIdentityNames().Select(n => n.NameParts.Select(part => part.Value).ToArray()).ToArray();
        var verifiedDatesOfBirth = ticket.Principal.GetCoreIdentityBirthDates().Select(d => d.Value).ToArray();
        var coreIdentityClaimVc = ticket.Principal.FindFirstValue("vc") ?? throw new InvalidOperationException("No vc claim present.");

        return OnUserVerifiedCoreAsync(
            verifiedNames,
            verifiedDatesOfBirth,
            coreIdentityClaimVc,
            state => state.OneLoginAuthenticationTicket = ticket);
    }

    public async Task OnUserVerifiedCoreAsync(
        string[][] verifiedNames,
        DateOnly[] verifiedDatesOfBirth,
        string? coreIdentityClaimVc,
        Action<SignInJourneyState>? updateState = null)
    {
        var sub = State.OneLoginAuthenticationTicket!.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");

        var oneLoginUser = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == sub);
        oneLoginUser.SetVerified(clock.UtcNow, OneLoginUserVerificationRoute.OneLogin, verifiedByApplicationUserId: null, verifiedNames, verifiedDatesOfBirth);
        oneLoginUser.LastCoreIdentityVc = coreIdentityClaimVc;

        string? trn = null;
        string? trnTokenTrn = null;

        if (await TryMatchToIdentityUserAsync() is TryMatchToIdentityUserResult result)
        {
            if (result.MatchRoute == OneLoginUserMatchRoute.TrnToken)
            {
                trnTokenTrn = result.Trn;
            }

            if (result.MatchRoute is not null)
            {
                oneLoginUser.FirstSignIn = clock.UtcNow;
                oneLoginUser.LastSignIn = clock.UtcNow;
                oneLoginUser.SetMatched(clock.UtcNow, result.PersonId, result.MatchRoute.Value, result.MatchedAttributes!.ToArray());
                trn = result.Trn;
            }
        }

        await dbContext.SaveChangesAsync();

        UpdateState(state =>
        {
            updateState?.Invoke(state);
            state.AttemptedIdentityVerification = true;

            state.SetVerified(verifiedNames, verifiedDatesOfBirth);

            state.TrnTokenTrn = trnTokenTrn;

            if (trn is not null)
            {
                Complete(state, trn);
            }
        });

        async Task<TryMatchToIdentityUserResult?> TryMatchToIdentityUserAsync()
        {
            Person? getAnIdentityPerson = null;
            OneLoginUserMatchRoute? matchRoute = null;
            IdTrnToken? trnTokenModel = null;

            // First try and match on TRN Token
            if (State.TrnToken is string trnToken)
            {
                trnTokenModel = await WithIdDbContextAsync(c => c.TrnTokens.SingleOrDefaultAsync(
                    t => t.TrnToken == trnToken && t.ExpiresUtc > clock.UtcNow && t.UserId == null));
                if (trnTokenModel is not null)
                {
                    getAnIdentityPerson = await dbContext.Persons.SingleOrDefaultAsync(p => p.Trn == trnTokenModel.Trn);
                    matchRoute = getAnIdentityPerson is not null ? OneLoginUserMatchRoute.TrnToken : null;
                }
            }

            // Couldn't match on TRN Token, try and match on email and TRN
            if (getAnIdentityPerson is null)
            {
                var identityUser = await WithIdDbContextAsync(c => c.Users.SingleOrDefaultAsync(
                    u => u.EmailAddress == oneLoginUser.EmailAddress
                        && u.Trn != null
                        && u.IsDeleted == false
                        && (u.TrnVerificationLevel == TrnVerificationLevel.Medium
                            || u.TrnAssociationSource == TrnAssociationSource.TrnToken
                            || u.TrnAssociationSource == TrnAssociationSource.SupportUi)));
                if (identityUser is not null)
                {
                    getAnIdentityPerson = await dbContext.Persons.SingleOrDefaultAsync(p => p.Trn == identityUser.Trn);
                    matchRoute = getAnIdentityPerson is not null ? OneLoginUserMatchRoute.GetAnIdentityUser : null;
                }
            }

            if (getAnIdentityPerson is null)
            {
                return null;
            }

            // Check the record's last name and DOB match the verified details
            var matchedLastName = verifiedNames.Select(parts => parts.Last()).FirstOrDefault(name => name.Equals(getAnIdentityPerson.LastName, StringComparison.OrdinalIgnoreCase));
            var matchedDateOfBirth = verifiedDatesOfBirth.FirstOrDefault(dob => dob == getAnIdentityPerson.DateOfBirth);
            if (matchedLastName == default || matchedDateOfBirth == default)
            {
                return new(getAnIdentityPerson.PersonId, getAnIdentityPerson.Trn, MatchRoute: null, MatchedAttributes: null);
            }
            var matchedAttributes = new Dictionary<PersonMatchedAttribute, string>()
            {
                { PersonMatchedAttribute.LastName, matchedLastName },
                { PersonMatchedAttribute.DateOfBirth, matchedDateOfBirth.ToString("yyyy-MM-dd") }
            };

            if (trnTokenModel is not null)
            {
                // Invalidate the token
                trnTokenModel.UserId = _teacherAuthIdUserIdSentinel;
                await WithIdDbContextAsync(c => c.SaveChangesAsync());
            }

            return new(getAnIdentityPerson.PersonId, getAnIdentityPerson.Trn, MatchRoute: matchRoute, matchedAttributes);
        }
    }

    public IResult OnVerificationFailed()
    {
        UpdateState(state =>
        {
            state.AttemptedIdentityVerification = true;
        });

        return GetNextPage();
    }

    public void OnSignOut() => UpdateState(s => s.Reset());

    public IResult GetNextPage() => State switch
    {
        // Authentication is complete
        { AuthenticationTicket: not null } =>
            SquashPathAndAdvanceTo(GetRedirectUri(), includeRedirectUri: false),

        // Authenticated with OneLogin, identity verification succeeded, not yet matched to teaching record
        { OneLoginAuthenticationTicket: not null, IdentityVerified: true, AuthenticationTicket: null } =>
            SquashPathAndAdvanceTo(Links.Connect()),

        // Authenticated with OneLogin, not yet verified
        { OneLoginAuthenticationTicket: not null, IdentityVerified: false, AttemptedIdentityVerification: false } =>
            VerifyIdentityWithOneLogin(),

        // Authenticated with OneLogin, identity verification failed
        { OneLoginAuthenticationTicket: not null, IdentityVerified: false } =>
            SquashPathAndAdvanceTo(Links.NotVerified()),

        _ => throw new InvalidOperationException("Cannot determine next page.")
    };

    public string GetRedirectUri() => State.RedirectUri;

    public override bool StepIsValid(JourneyPathStep step)
    {
        return base.StepIsValid(step) || step.NormalizedUrl.StartsWith("/oauth2/authorize");
    }

    public async Task<bool> TryMatchToTeachingRecordAsync()
    {
        if (State.OneLoginAuthenticationTicket is null || !State.IdentityVerified)
        {
            throw new InvalidOperationException("Cannot match to a teaching record without a verified One Login user.");
        }

        var names = State.VerifiedNames;
        var datesOfBirth = State.VerifiedDatesOfBirth;
        var nationalInsuranceNumber = State.NationalInsuranceNumber;
        var trn = State.Trn;
        var trnTokenTrn = State.TrnTokenTrn;

        var matchResult = await oneLoginService.MatchPersonAsync(new(names!, datesOfBirth!, nationalInsuranceNumber, trn, trnTokenTrn));

        if (matchResult is var (matchedPersonId, matchedTrn, matchedAttributes))
        {
            var subject = State.OneLoginAuthenticationTicket.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");

            var oneLoginUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == subject);
            oneLoginUser.FirstSignIn = clock.UtcNow;
            oneLoginUser.LastSignIn = clock.UtcNow;
            oneLoginUser.SetMatched(clock.UtcNow, matchedPersonId, OneLoginUserMatchRoute.Interactive, matchedAttributes.ToArray());
            await dbContext.SaveChangesAsync();

            UpdateState(state => Complete(state, matchedTrn));

            return true;
        }

        return false;
    }

    private IResult SquashPathAndAdvanceTo(string url, bool includeRedirectUri = true)
    {
        var urls = new List<string>(2) { url };

        // Ensure the RedirectUri is included in the path
        if (includeRedirectUri)
        {
            urls.Insert(0, GetRedirectUri());
        }

        var steps = urls.Select(CreateStepFromUrl);
        var path = new JourneyPath(steps);
        UnsafeSetPath(path);

        return Results.Redirect(urls.Last());
    }

    private async Task<TryMatchToTrnRequestResult?> TryMatchToTrnRequestAsync(OneLoginUser oneLoginUser)
    {
        Debug.Assert(oneLoginUser.EmailAddress is not null);

        var requestAndResolvedPerson = await dbContext.TrnRequestMetadata
            .Join(dbContext.Persons, r => r.ResolvedPersonId, p => p.PersonId, (m, p) => new { TrnRequestMetadata = m, ResolvedPerson = p })
            .Where(m => m.TrnRequestMetadata.OneLoginUserSubject == oneLoginUser.Subject || m.TrnRequestMetadata.EmailAddress == oneLoginUser.EmailAddress)
            .ToArrayAsync();

        if (requestAndResolvedPerson is not [{ TrnRequestMetadata: var trnRequestMetadata, ResolvedPerson: var resolvedPerson }])
        {
            return null;
        }

        if (trnRequestMetadata.IdentityVerified != true)
        {
            return null;
        }

        if (trnRequestMetadata.Status is not TrnRequestStatus.Completed)
        {
            return null;
        }
        Debug.Assert(trnRequestMetadata.ResolvedPersonId.HasValue);

        oneLoginUser.SetVerified(
            verifiedOn: trnRequestMetadata.CreatedOn,
            route: OneLoginUserVerificationRoute.External,
            verifiedByApplicationUserId: trnRequestMetadata.ApplicationUserId,
            verifiedNames: [trnRequestMetadata.Name],
            verifiedDatesOfBirth: [trnRequestMetadata.DateOfBirth]);

        oneLoginUser.SetMatched(
            clock.UtcNow,
            trnRequestMetadata.ResolvedPersonId!.Value,
            route: OneLoginUserMatchRoute.TrnRequest,
            matchedAttributes: null);

        return new(resolvedPerson.Trn);
    }

    public void Complete(SignInJourneyState state, string trn)
    {
        if (state.OneLoginAuthenticationTicket is null)
        {
            throw new InvalidOperationException("User is not authenticated with One Login.");
        }

        var oneLoginPrincipal = state.OneLoginAuthenticationTicket.Principal;
        var oneLoginIdToken = state.OneLoginAuthenticationTicket.Properties.GetTokenValue(OpenIdConnectParameterNames.IdToken)!;

        var teachingRecordIdentity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Subject, oneLoginPrincipal.FindFirstValue("sub")!),
                new Claim(ClaimTypes.Trn, trn),
                new Claim(ClaimTypes.Email, oneLoginPrincipal.FindFirstValue("email")!),
                new Claim(ClaimTypes.OneLoginIdToken, oneLoginIdToken),
                new Claim(ClaimTypes.TrsUserId, state.ClientApplicationUserId.ToString())
            ],
            authenticationType: "Authorize access to a teaching record",
            nameType: ClaimTypes.Subject,
            roleType: null);

        var principal = new ClaimsPrincipal(teachingRecordIdentity);

        state.AuthenticationTicket = new AuthenticationTicket(principal, properties: null, AuthenticationSchemes.MatchToTeachingRecord);
    }

    private IResult OneLoginChallenge(string vtr)
    {
        var delegatedProperties = new AuthenticationProperties();
        delegatedProperties.SetVectorsOfTrust([vtr]);
        delegatedProperties.Items.Add(JourneySignInHandler.PropertyKeys.JourneyInstanceId, InstanceId.ToString());
        return Results.Challenge(delegatedProperties, authenticationSchemes: [State.OneLoginAuthenticationScheme]);
    }

    private async Task<T> WithIdDbContextAsync<T>(Func<IdDbContext, Task<T>> action)
    {
        using var sc = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
        return await action(idDbContext);
    }

    public sealed class LinkHelper(AuthorizeAccessLinkGenerator linkGenerator, JourneyInstanceId instanceId)
    {
        public string DebugIdentity() => linkGenerator.DebugIdentity(instanceId);

        public string Connect() => linkGenerator.Connect(instanceId);

        public string NationalInsuranceNumber(string? returnUrl = null) =>
            linkGenerator.NationalInsuranceNumber(instanceId, returnUrl);

        public string Trn(string? returnUrl = null) => linkGenerator.Trn(instanceId, returnUrl);

        public string CheckAnswers() => linkGenerator.CheckAnswers(instanceId);

        public string SupportRequestSubmitted() => linkGenerator.SupportRequestSubmitted(instanceId);

        public string Found() => linkGenerator.Found(instanceId);

        public string NotFound() => linkGenerator.NotFound(instanceId);

        public string NotVerified() => linkGenerator.NotVerified(instanceId);
    }

    private record TryMatchToIdentityUserResult(
        Guid PersonId,
        string Trn,
        OneLoginUserMatchRoute? MatchRoute,
        IReadOnlyCollection<KeyValuePair<PersonMatchedAttribute, string>>? MatchedAttributes);

    private record TryMatchToTrnRequestResult(string Trn);
}

using System.Diagnostics;
using System.Security.Claims;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.PersonSearch;
using TeachingRecordSystem.FormFlow;
using TeachingRecordSystem.FormFlow.State;

namespace TeachingRecordSystem.AuthorizeAccess;

public class SignInJourneyHelper(
    TrsDbContext dbContext,
    IPersonSearchService personSearchService,
    AuthorizeAccessLinkGenerator linkGenerator,
    IOptions<AuthorizeAccessOptions> optionsAccessor,
    IUserInstanceStateProvider userInstanceStateProvider,
    IClock clock)
{
    public const string AuthenticationOnlyVtr = @"[""Cl.Cm""]";
    public const string AuthenticationAndIdentityVerificationVtr = @"[""Cl.Cm.P2""]";

    public AuthorizeAccessLinkGenerator LinkGenerator { get; } = linkGenerator;

    public IUserInstanceStateProvider UserInstanceStateProvider { get; } = userInstanceStateProvider;

    public bool ShowDebugPages => optionsAccessor.Value.ShowDebugPages;

    public IResult SignInWithOneLogin(JourneyInstance<SignInJourneyState> journeyInstance) =>
        OneLoginChallenge(journeyInstance, AuthenticationOnlyVtr);

    public IResult VerifyIdentityWithOneLogin(JourneyInstance<SignInJourneyState> journeyInstance) =>
        OneLoginChallenge(journeyInstance, AuthenticationAndIdentityVerificationVtr);

    public async Task<IResult> OnSignedInWithOneLogin(JourneyInstance<SignInJourneyState> journeyInstance, AuthenticationTicket ticket)
    {
        if (!ticket.Properties.TryGetVectorOfTrust(out var vtr))
        {
            throw new InvalidOperationException("No vtr.");
        }

        var sub = ticket.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");
        var email = ticket.Principal.FindFirstValue("email") ?? throw new InvalidOperationException("No email claim.");

        if (vtr == AuthenticationOnlyVtr)
        {
            await OnUserAuthenticated();
        }
        else
        {
            Debug.Assert(vtr == AuthenticationAndIdentityVerificationVtr);
            Debug.Assert(journeyInstance.State.OneLoginAuthenticationTicket is not null);
            await OnUserVerified();
        }

        if (ShowDebugPages)
        {
            return Results.Redirect(LinkGenerator.DebugIdentity(journeyInstance.InstanceId));
        }

        return GetNextPage(journeyInstance);

        async Task OnUserAuthenticated()
        {
            var oneLoginUser = await dbContext.OneLoginUsers
                .Include(u => u.Person)
                .SingleOrDefaultAsync(u => u.Subject == sub);

            if (oneLoginUser is null)
            {
                oneLoginUser = new()
                {
                    Subject = sub,
                    Email = email,
                    FirstOneLoginSignIn = clock.UtcNow,
                    LastOneLoginSignIn = clock.UtcNow
                };
                dbContext.OneLoginUsers.Add(oneLoginUser);
            }
            else
            {
                oneLoginUser.LastOneLoginSignIn = clock.UtcNow;

                // Email may have changed since the last sign in - ensure we update it.
                // TODO Should we emit an event if it has changed?
                oneLoginUser.Email = email;

                if (oneLoginUser.PersonId is not null)
                {
                    oneLoginUser.LastSignIn = clock.UtcNow;
                }
            }

            await dbContext.SaveChangesAsync();

            await journeyInstance.UpdateStateAsync(state =>
            {
                state.Reset();
                state.OneLoginAuthenticationTicket = ticket;

                if (oneLoginUser.VerificationRoute is not null)
                {
                    Debug.Assert(oneLoginUser.VerifiedNames is not null);
                    Debug.Assert(oneLoginUser.VerifiedDatesOfBirth is not null);
                    state.SetVerified(oneLoginUser.VerifiedNames!, oneLoginUser.VerifiedDatesOfBirth!);
                }

                if (oneLoginUser.Person?.Trn is string trn && !ShowDebugPages)
                {
                    Complete(state, trn);
                }
            });
        }

        async Task OnUserVerified()
        {
            var verifiedNames = ticket.Principal.GetCoreIdentityNames().Select(n => n.NameParts.Select(part => part.Value).ToArray()).ToArray();
            var verifiedDatesOfBirth = ticket.Principal.GetCoreIdentityBirthDates().Select(d => d.Value).ToArray();
            var coreIdentityClaimVc = ticket.Principal.FindFirstValue("vc") ?? throw new InvalidOperationException("No vc claim present.");

            var oneLoginUser = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == sub);
            oneLoginUser.VerifiedOn = clock.UtcNow;
            oneLoginUser.VerificationRoute = OneLoginUserVerificationRoute.OneLogin;
            oneLoginUser.VerifiedNames = verifiedNames;
            oneLoginUser.VerifiedDatesOfBirth = verifiedDatesOfBirth;
            oneLoginUser.LastCoreIdentityVc = coreIdentityClaimVc;
            await dbContext.SaveChangesAsync();

            await journeyInstance.UpdateStateAsync(state =>
            {
                state.OneLoginAuthenticationTicket = ticket;
                state.AttemptedIdentityVerification = true;

                state.SetVerified(verifiedNames, verifiedDatesOfBirth);
            });
        }
    }

    public async Task<IResult> OnUserVerificationWithOneLoginFailed(JourneyInstance<SignInJourneyState> journeyInstance)
    {
        await journeyInstance.UpdateStateAsync(state =>
        {
            state.AttemptedIdentityVerification = true;
        });

        return GetNextPage(journeyInstance);
    }

    public IResult GetNextPage(JourneyInstance<SignInJourneyState> journeyInstance) => journeyInstance.State switch
    {
        // Authentication is complete
        { AuthenticationTicket: not null } => Results.Redirect(GetSafeRedirectUri(journeyInstance)),

        // Authenticated with OneLogin, identity verification succeeded, not yet matched to teaching record
        { OneLoginAuthenticationTicket: not null, IdentityVerified: true, AuthenticationTicket: null } =>
            Results.Redirect(LinkGenerator.Connect(journeyInstance.InstanceId)),

        // Authenticated with OneLogin, not yet verified
        { OneLoginAuthenticationTicket: not null, IdentityVerified: false, AttemptedIdentityVerification: false } =>
            VerifyIdentityWithOneLogin(journeyInstance),

        // Authenticated with OneLogin, identity verification failed
        { OneLoginAuthenticationTicket: not null, IdentityVerified: false } => Results.Redirect(LinkGenerator.NotVerified(journeyInstance.InstanceId)),

        _ => throw new InvalidOperationException("Cannot determine next page.")
    };

    public string GetSafeRedirectUri(JourneyInstance<SignInJourneyState> journeyInstance) =>
        EnsureUrlHasJourneyId(journeyInstance.State.RedirectUri, journeyInstance.InstanceId);

    public async Task<bool> TryMatchToTeachingRecord(JourneyInstance<SignInJourneyState> journeyInstance)
    {
        var state = journeyInstance.State;

        if (state.OneLoginAuthenticationTicket is null || !state.IdentityVerified)
        {
            throw new InvalidOperationException("Cannot match to a teaching record without a verified One Login user.");
        }

        var names = state.VerifiedNames;
        var datesOfBirth = state.VerifiedDatesOfBirth;
        var nationalInsuranceNumber = NormalizeNationalInsuranceNumber(state.NationalInsuranceNumber);
        var trn = NormalizeTrn(state.Trn);

        var personSearchResults = await personSearchService.Search(names!, datesOfBirth!, nationalInsuranceNumber, trn);

        if (personSearchResults.Count == 1)
        {
            var result = personSearchResults.Single();
            var matchedPersonId = result.PersonId;
            var matchedTrn = result.Trn;

            // It's possible we match on a record that doesn't have a TRN; we don't want to proceed in that case;
            // downstream consumers can't do anything without a TRN
            if (matchedTrn is null)
            {
                return false;
            }

            var subject = state.OneLoginAuthenticationTicket.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");

            var oneLoginUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == subject);
            oneLoginUser.PersonId = matchedPersonId;
            oneLoginUser.FirstSignIn = clock.UtcNow;
            oneLoginUser.LastSignIn = clock.UtcNow;
            await dbContext.SaveChangesAsync();

            await journeyInstance.UpdateStateAsync(state => Complete(state, matchedTrn));

            return true;
        }

        return false;

        static string? NormalizeNationalInsuranceNumber(string? value) =>
            string.IsNullOrEmpty(value) ? null : new(value.Where(char.IsAsciiLetterOrDigit).ToArray());

        static string? NormalizeTrn(string? value) =>
            string.IsNullOrEmpty(value) ? null : new(value.Where(char.IsAsciiDigit).ToArray());
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
                new Claim(ClaimTypes.OneLoginIdToken, oneLoginIdToken)
            ],
            authenticationType: "Authorize access to a teaching record",
            nameType: "sub",
            roleType: null);

        var principal = new ClaimsPrincipal(teachingRecordIdentity);

        state.AuthenticationTicket = new AuthenticationTicket(principal, properties: null, AuthenticationSchemes.MatchToTeachingRecord);
    }

    private static string EnsureUrlHasJourneyId(string url, JourneyInstanceId instanceId)
    {
        var queryParamName = Constants.UniqueKeyQueryParameterName;

        if (!url.Contains($"{queryParamName}=", StringComparison.Ordinal))
        {
            return QueryHelpers.AddQueryString(url, queryParamName, instanceId.UniqueKey!);
        }
        else
        {
            return url;
        }
    }

    private IResult OneLoginChallenge(JourneyInstance<SignInJourneyState> journeyInstance, string vtr)
    {
        var delegatedProperties = new AuthenticationProperties();
        delegatedProperties.SetVectorOfTrust(vtr);
        delegatedProperties.Items.Add(FormFlowJourneySignInHandler.PropertyKeys.JourneyInstanceId, journeyInstance.InstanceId.Serialize());
        return Results.Challenge(delegatedProperties, authenticationSchemes: [journeyInstance.State.OneLoginAuthenticationScheme]);
    }
}

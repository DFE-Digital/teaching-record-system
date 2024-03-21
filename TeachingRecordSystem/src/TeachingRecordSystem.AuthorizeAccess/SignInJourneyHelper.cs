using System.Security.Claims;
using System.Text.Json;
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
        var attemptedIdentityVerification = vtr == AuthenticationAndIdentityVerificationVtr;

        await journeyInstance.UpdateStateAsync(state =>
        {
            state.Reset();
            state.OneLoginAuthenticationTicket = ticket;
            state.AttemptedIdentityVerification = attemptedIdentityVerification;

            var vc = ticket.Principal.FindFirstValue("vc") is string vcStr ? JsonDocument.Parse(vcStr) : null;
            state.IdentityVerified = vc is not null;
            if (state.IdentityVerified)
            {
                state.VerifiedNames = ticket.Principal.GetCoreIdentityNames().Select(n => n.NameParts.Select(part => part.Value).ToArray()).ToArray();
                state.VerifiedDatesOfBirth = ticket.Principal.GetCoreIdentityBirthDates().Select(d => d.Value).ToArray();
            }
        });

        if (ShowDebugPages)
        {
            return Results.Redirect(linkGenerator.DebugIdentity(journeyInstance.InstanceId));
        }

        return await CreateOrUpdateOneLoginUser(journeyInstance);
    }

    public async Task<IResult> CreateOrUpdateOneLoginUser(JourneyInstance<SignInJourneyState> journeyInstance)
    {
        if (journeyInstance.State.OneLoginAuthenticationTicket is null)
        {
            throw new InvalidOperationException($"{nameof(journeyInstance.State.OneLoginAuthenticationTicket)} is not set.");
        }

        var subject = journeyInstance.State.OneLoginAuthenticationTicket.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");
        var email = journeyInstance.State.OneLoginAuthenticationTicket.Principal.FindFirstValue("email") ?? throw new InvalidOperationException("No email claim.");
        var vc = journeyInstance.State.OneLoginAuthenticationTicket.Principal.FindFirstValue("vc") is string vcStr ? JsonDocument.Parse(vcStr) : null;

        var oneLoginUser = await dbContext.OneLoginUsers
            .Include(o => o.Person)
            .SingleOrDefaultAsync(o => o.Subject == subject);

        if (oneLoginUser is not null)
        {
            oneLoginUser.LastOneLoginSignIn = clock.UtcNow;
            oneLoginUser.Email = email;

            if (oneLoginUser.Person is not null)
            {
                oneLoginUser.LastSignIn = clock.UtcNow;
                await dbContext.SaveChangesAsync();

                await journeyInstance.UpdateStateAsync(state => CreateAndAssignPrincipal(state, oneLoginUser.Person.Trn!));
            }
        }
        else
        {
            oneLoginUser = new()
            {
                Subject = subject,
                Email = email,
                FirstOneLoginSignIn = clock.UtcNow,
                LastOneLoginSignIn = clock.UtcNow
            };
            dbContext.OneLoginUsers.Add(oneLoginUser);
        }

        await dbContext.SaveChangesAsync();

        if (oneLoginUser.PersonId is null && !journeyInstance.State.IdentityVerified && !journeyInstance.State.AttemptedIdentityVerification)
        {
            return VerifyIdentityWithOneLogin(journeyInstance);
        }

        return Results.Redirect(GetNextPage(journeyInstance));
    }

    public string GetNextPage(JourneyInstance<SignInJourneyState> journeyInstance) => journeyInstance.State switch
    {
        // Authentication is complete
        { AuthenticationTicket: not null } => GetSafeRedirectUri(journeyInstance),

        // Authenticated with OneLogin, identity verification succeeded, not yet matched to teaching record
        { OneLoginAuthenticationTicket: not null, IdentityVerified: true, AuthenticationTicket: null } =>
            linkGenerator.NationalInsuranceNumber(journeyInstance.InstanceId),

        // Authenticated with OneLogin, identity verification failed
        { OneLoginAuthenticationTicket: not null, IdentityVerified: false } => linkGenerator.NotVerified(journeyInstance.InstanceId),

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

            await journeyInstance.UpdateStateAsync(state => CreateAndAssignPrincipal(state, matchedTrn));

            return true;
        }

        return false;

        static string? NormalizeNationalInsuranceNumber(string? value) =>
            string.IsNullOrEmpty(value) ? null : new(value.Where(char.IsAsciiLetterOrDigit).ToArray());

        static string? NormalizeTrn(string? value) =>
            string.IsNullOrEmpty(value) ? null : new(value.Where(char.IsAsciiDigit).ToArray());
    }

    private static void CreateAndAssignPrincipal(SignInJourneyState state, string trn)
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

        state.AuthenticationTicket = new AuthenticationTicket(principal, state.AuthenticationProperties, AuthenticationSchemes.MatchToTeachingRecord);
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

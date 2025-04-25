using System.Diagnostics;
using System.Security.Claims;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.WebCommon.FormFlow;
using TeachingRecordSystem.WebCommon.FormFlow.State;
using static TeachingRecordSystem.AuthorizeAccess.IdModelTypes;

namespace TeachingRecordSystem.AuthorizeAccess;

public class SignInJourneyHelper(
    TrsDbContext dbContext,
    IPersonMatchingService personMatchingService,
    IdDbContext idDbContext,
    AuthorizeAccessLinkGenerator linkGenerator,
    IOptions<AuthorizeAccessOptions> optionsAccessor,
    IUserInstanceStateProvider userInstanceStateProvider,
    IClock clock,
    TrnRequestHelper trnRequestHelper)
{
    public const string AuthenticationOnlyVtr = @"[""Cl.Cm""]";
    public const string AuthenticationAndIdentityVerificationVtr = @"[""Cl.Cm.P2""]";

    // ID's database has a user_id column to indicate that a TRN token has been used already.
    // This sentinel value indicates the token has been used by us, rather than a teacher ID user.
    private static readonly Guid _teacherAuthIdUserIdSentinel = Guid.Empty;

    public AuthorizeAccessLinkGenerator LinkGenerator { get; } = linkGenerator;

    public IUserInstanceStateProvider UserInstanceStateProvider { get; } = userInstanceStateProvider;

    public bool ShowDebugPages => optionsAccessor.Value.ShowDebugPages;

    public IResult SignInWithOneLogin(JourneyInstance<SignInJourneyState> journeyInstance) =>
        OneLoginChallenge(journeyInstance, AuthenticationOnlyVtr);

    public IResult VerifyIdentityWithOneLogin(JourneyInstance<SignInJourneyState> journeyInstance) =>
        OneLoginChallenge(journeyInstance, AuthenticationAndIdentityVerificationVtr);

    public async Task<IResult> OnOneLoginCallbackAsync(JourneyInstance<SignInJourneyState> journeyInstance, AuthenticationTicket ticket)
    {
        if (!ticket.Properties.TryGetVectorOfTrust(out var vtr))
        {
            throw new InvalidOperationException("No vtr.");
        }

        if (vtr == AuthenticationOnlyVtr)
        {
            await OnUserAuthenticatedAsync(journeyInstance, ticket);
        }
        else
        {
            Debug.Assert(vtr == AuthenticationAndIdentityVerificationVtr);
            Debug.Assert(journeyInstance.State.OneLoginAuthenticationTicket is not null);
            await OnUserVerifiedAsync(journeyInstance, ticket);
        }

        if (ShowDebugPages)
        {
            return Results.Redirect(LinkGenerator.DebugIdentity(journeyInstance.InstanceId));
        }

        return GetNextPage(journeyInstance);
    }

    public async Task OnUserAuthenticatedAsync(JourneyInstance<SignInJourneyState> journeyInstance, AuthenticationTicket ticket)
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
                Email = email,
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
            oneLoginUser.Email = email;

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

            if (trn is not null && !ShowDebugPages)
            {
                Complete(state, trn);
            }
        });
    }

    public Task OnUserVerifiedAsync(JourneyInstance<SignInJourneyState> journeyInstance, AuthenticationTicket ticket)
    {
        var verifiedNames = ticket.Principal.GetCoreIdentityNames().Select(n => n.NameParts.Select(part => part.Value).ToArray()).ToArray();
        var verifiedDatesOfBirth = ticket.Principal.GetCoreIdentityBirthDates().Select(d => d.Value).ToArray();
        var coreIdentityClaimVc = ticket.Principal.FindFirstValue("vc") ?? throw new InvalidOperationException("No vc claim present.");

        return OnUserVerifiedCoreAsync(
            journeyInstance,
            verifiedNames,
            verifiedDatesOfBirth,
            coreIdentityClaimVc,
            state => state.OneLoginAuthenticationTicket = ticket);
    }

    public async Task OnUserVerifiedCoreAsync(
        JourneyInstance<SignInJourneyState> journeyInstance,
        string[][] verifiedNames,
        DateOnly[] verifiedDatesOfBirth,
        string? coreIdentityClaimVc,
        Action<SignInJourneyState>? updateState = null)
    {
        var sub = journeyInstance.State.OneLoginAuthenticationTicket!.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");

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
                oneLoginUser.SetMatched(result.PersonId, result.MatchRoute.Value, result.MatchedAttributes!.ToArray());
                trn = result.Trn;
            }
        }

        await dbContext.SaveChangesAsync();

        await journeyInstance.UpdateStateAsync(state =>
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
            if (journeyInstance.State.TrnToken is string trnToken)
            {
                trnTokenModel = await idDbContext.TrnTokens.SingleOrDefaultAsync(
                    t => t.TrnToken == trnToken && t.ExpiresUtc > clock.UtcNow && t.UserId == null);
                if (trnTokenModel is not null)
                {
                    getAnIdentityPerson = await dbContext.Persons.SingleOrDefaultAsync(
                        p => p.Trn == trnTokenModel.Trn && p.DqtState == (int)ContactState.Active);
                    matchRoute = getAnIdentityPerson is not null ? OneLoginUserMatchRoute.TrnToken : null;
                }
            }

            // Couldn't match on TRN Token, try and match on email and TRN
            if (getAnIdentityPerson is null)
            {
                var identityUser = await idDbContext.Users.SingleOrDefaultAsync(
                    u => u.EmailAddress == oneLoginUser.Email
                        && u.Trn != null
                        && u.IsDeleted == false
                        && (u.TrnVerificationLevel == TrnVerificationLevel.Medium
                            || u.TrnAssociationSource == TrnAssociationSource.TrnToken
                            || u.TrnAssociationSource == TrnAssociationSource.SupportUi));
                if (identityUser is not null)
                {
                    getAnIdentityPerson = await dbContext.Persons.SingleOrDefaultAsync(
                        p => p.Trn == identityUser.Trn && p.DqtState == (int)ContactState.Active);
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
                return new(getAnIdentityPerson.PersonId, getAnIdentityPerson.Trn!, MatchRoute: null, MatchedAttributes: null);
            }
            var matchedAttributes = new Dictionary<OneLoginUserMatchedAttribute, string>()
            {
                { OneLoginUserMatchedAttribute.LastName, matchedLastName },
                { OneLoginUserMatchedAttribute.DateOfBirth, matchedDateOfBirth.ToString("yyyy-MM-dd") }
            };

            if (trnTokenModel is not null)
            {
                // Invalidate the token
                trnTokenModel.UserId = _teacherAuthIdUserIdSentinel;
                await idDbContext.SaveChangesAsync();
            }

            return new(getAnIdentityPerson.PersonId, getAnIdentityPerson.Trn!, MatchRoute: matchRoute, matchedAttributes);
        }
    }

    public async Task<IResult> OnVerificationFailedAsync(JourneyInstance<SignInJourneyState> journeyInstance)
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

    public async Task<bool> TryMatchToTeachingRecordAsync(JourneyInstance<SignInJourneyState> journeyInstance)
    {
        var state = journeyInstance.State;

        if (state.OneLoginAuthenticationTicket is null || !state.IdentityVerified)
        {
            throw new InvalidOperationException("Cannot match to a teaching record without a verified One Login user.");
        }

        var names = state.VerifiedNames;
        var datesOfBirth = state.VerifiedDatesOfBirth;
        var nationalInsuranceNumber = state.NationalInsuranceNumber;
        var trn = state.Trn;

        var matchResult = await personMatchingService.MatchOneLoginUserAsync(new(names!, datesOfBirth!, nationalInsuranceNumber, trn));

        if (matchResult is var (matchedPersonId, matchedTrn, matchedAttributes))
        {
            // It's possible we match on a record that doesn't have a TRN; we don't want to proceed in that case;
            // downstream consumers can't do anything without a TRN
            if (matchedTrn is null)
            {
                return false;
            }

            var subject = state.OneLoginAuthenticationTicket.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");

            var oneLoginUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == subject);
            oneLoginUser.FirstSignIn = clock.UtcNow;
            oneLoginUser.LastSignIn = clock.UtcNow;
            oneLoginUser.SetMatched(matchedPersonId, OneLoginUserMatchRoute.Interactive, matchedAttributes.ToArray());
            await dbContext.SaveChangesAsync();

            await journeyInstance.UpdateStateAsync(state => Complete(state, matchedTrn));

            return true;
        }

        return false;
    }

    private async Task<TryMatchToTrnRequestResult?> TryMatchToTrnRequestAsync(OneLoginUser oneLoginUser)
    {
        Debug.Assert(oneLoginUser.Email is not null);

        var trnRequestMetadataForUser = await dbContext.TrnRequestMetadata
            .Where(m => m.OneLoginUserSubject == oneLoginUser.Subject || m.EmailAddress == oneLoginUser.Email)
            .ToArrayAsync();

        if (trnRequestMetadataForUser is not [var trnRequestMetadata])
        {
            return null;
        }

        if (trnRequestMetadata.IdentityVerified != true)
        {
            return null;
        }

        var trnRequest = await trnRequestHelper.GetTrnRequestInfoAsync(trnRequestMetadata.ApplicationUserId, trnRequestMetadata.RequestId);
        if (trnRequest is null)
        {
            Debug.Fail("TRN request does not exist.");
            return null;
        }

        if (!trnRequest.IsCompleted)
        {
            return null;
        }

        oneLoginUser.SetVerified(
            verifiedOn: trnRequestMetadata.CreatedOn,
            route: OneLoginUserVerificationRoute.External,
            verifiedByApplicationUserId: trnRequestMetadata.ApplicationUserId,
            verifiedNames: [trnRequestMetadata.Name],
            verifiedDatesOfBirth: [trnRequestMetadata.DateOfBirth]);

        oneLoginUser.SetMatched(
            trnRequest.Contact.Id,
            route: OneLoginUserMatchRoute.TrnRequest,
            matchedAttributes: null);

        return new(trnRequest.Contact.dfeta_TRN);
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

    private record TryMatchToIdentityUserResult(
        Guid PersonId,
        string Trn,
        OneLoginUserMatchRoute? MatchRoute,
        IReadOnlyCollection<KeyValuePair<OneLoginUserMatchedAttribute, string>>? MatchedAttributes);

    private record TryMatchToTrnRequestResult(string Trn);
}

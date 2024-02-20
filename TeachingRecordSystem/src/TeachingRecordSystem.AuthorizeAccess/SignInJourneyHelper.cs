using System.Security.Claims;
using System.Text.Json;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
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
    public IUserInstanceStateProvider UserInstanceStateProvider { get; } = userInstanceStateProvider;

    public bool ShowDebugPages => optionsAccessor.Value.ShowDebugPages;

    public async Task<NextPageInfo> OnSignedInWithOneLogin(JourneyInstance<SignInJourneyState> journeyInstance, AuthenticationTicket ticket)
    {
        await journeyInstance.UpdateStateAsync(state =>
        {
            state.Reset();
            state.OneLoginAuthenticationTicket = ticket;

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
            return new NextPageInfo(linkGenerator.DebugIdentity(journeyInstance.InstanceId));
        }

        return await CreateOrUpdateOneLoginUser(journeyInstance);
    }

    public async Task<NextPageInfo> CreateOrUpdateOneLoginUser(JourneyInstance<SignInJourneyState> journeyInstance)
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
            oneLoginUser.CoreIdentityVc = vc;
            oneLoginUser.LastOneLoginSignIn = clock.UtcNow;
            oneLoginUser.Email = email;

            if (oneLoginUser.Person is not null)
            {
                oneLoginUser.LastSignIn = clock.UtcNow;
                await dbContext.SaveChangesAsync();

                await journeyInstance.UpdateStateAsync(state =>
                    CreateAndAssignPrincipal(state, oneLoginUser.Person.PersonId, oneLoginUser.Person.Trn!));
            }
        }
        else
        {
            oneLoginUser = new()
            {
                Subject = subject,
                Email = email,
                FirstOneLoginSignIn = clock.UtcNow,
                LastOneLoginSignIn = clock.UtcNow,
                CoreIdentityVc = vc
            };
            dbContext.OneLoginUsers.Add(oneLoginUser);
        }

        await dbContext.SaveChangesAsync();

        return GetNextPage(journeyInstance);
    }

    public NextPageInfo GetNextPage(JourneyInstance<SignInJourneyState> journeyInstance) => journeyInstance.State switch
    {
        // Authentication is complete
        { AuthenticationTicket: not null } => new NextPageInfo(GetSafeRedirectUri(journeyInstance)),

        // Authenticated with OneLogin, identity verification succeeded, not yet matched to teaching record
        { OneLoginAuthenticationTicket: not null, IdentityVerified: true, AuthenticationTicket: null } =>
            new NextPageInfo(linkGenerator.NationalInsuranceNumber(journeyInstance.InstanceId)),

        // Authenticated with OneLogin, identity verification failed
        { OneLoginAuthenticationTicket: not null, IdentityVerified: false } => new NextPageInfo(linkGenerator.NotVerified(journeyInstance.InstanceId)),

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

            await journeyInstance.UpdateStateAsync(state =>
                CreateAndAssignPrincipal(state, matchedPersonId, matchedTrn));

            return true;
        }

        return false;

        static string? NormalizeNationalInsuranceNumber(string? value) =>
            string.IsNullOrEmpty(value) ? null : new(value.Where(char.IsAsciiLetterOrDigit).ToArray());

        static string? NormalizeTrn(string? value) =>
            string.IsNullOrEmpty(value) ? null : new(value.Where(char.IsAsciiDigit).ToArray());
    }

    private static void CreateAndAssignPrincipal(SignInJourneyState state, Guid personId, string trn)
    {
        if (state.OneLoginAuthenticationTicket is null)
        {
            throw new InvalidOperationException("User is not authenticated with One Login.");
        }

        var oneLoginIdentity = (ClaimsIdentity)state.OneLoginAuthenticationTicket.Principal.Identity!;

        var teachingRecordIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Trn, trn),
            new Claim(ClaimTypes.PersonId, personId.ToString())
        });

        var principal = new ClaimsPrincipal(new[] { oneLoginIdentity, teachingRecordIdentity });

        state.AuthenticationTicket = new AuthenticationTicket(principal, state.AuthenticationProperties, AuthenticationSchemes.MatchToTeachingRecord);
    }

    private static string EnsureUrlHasJourneyId(string url, JourneyInstanceId instanceId)
    {
        var queryParamName = Constants.UniqueKeyQueryParameterName;

        if (url.IndexOf($"{queryParamName}=") == -1)
        {
            return QueryHelpers.AddQueryString(url, queryParamName, instanceId.UniqueKey!);
        }
        else
        {
            return url;
        }
    }
}

public sealed record NextPageInfo(string Location)
{
    public IActionResult ToActionResult() => new RedirectResult(Location);
}

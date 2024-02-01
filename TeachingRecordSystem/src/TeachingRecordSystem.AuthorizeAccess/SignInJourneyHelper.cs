using System.Security.Claims;
using System.Text.Json;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.FormFlow;
using TeachingRecordSystem.FormFlow.State;

namespace TeachingRecordSystem.AuthorizeAccess;

public class SignInJourneyHelper(
    TrsDbContext dbContext,
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

                CreateAndAssignPrincipal(journeyInstance.State, oneLoginUser.Person.PersonId, oneLoginUser.Person.Trn!);
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
        { AuthenticationTicket: not null } => new NextPageInfo(journeyInstance.State.RedirectUri),

        // Authenticated with OneLogin, identity verification succeeded, person lookup failed
        // TODO

        // Authenticated with OneLogin, identity verification succeeded, TRN not yet specified
        // TODO

        // Authenticated with OneLogin, identity verification succeeded, NINO not yet specified
        { OneLoginAuthenticationTicket: not null } => new NextPageInfo(linkGenerator.Nino(journeyInstance.InstanceId)),

        // Authenticated with OneLogin, identity verification failed
        // TODO

        _ => throw new InvalidOperationException("Cannot determine next page.")
    };

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
}

public sealed record NextPageInfo(string Location)
{
    public IActionResult ToActionResult() => new RedirectResult(Location);
}

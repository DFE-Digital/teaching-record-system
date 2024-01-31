using System.Security.Claims;
using System.Text.Json;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.FormFlow.State;

namespace TeachingRecordSystem.AuthorizeAccess;

public class SignInJourneyHelper(
    TrsDbContext dbContext,
    IOptions<AuthorizeAccessOptions> optionsAccessor,
    IUserInstanceStateProvider userInstanceStateProvider,
    IClock clock)
{
    public IUserInstanceStateProvider UserInstanceStateProvider { get; } = userInstanceStateProvider;

    public async Task OnSignedInWithOneLogin(SignInJourneyState state, AuthenticationTicket ticket)
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

        if (!optionsAccessor.Value.ShowDebugPages)
        {
            await CreateOrUpdateOneLoginUser(state);
        }
    }

    public async Task CreateOrUpdateOneLoginUser(SignInJourneyState state)
    {
        if (state.OneLoginAuthenticationTicket is null)
        {
            throw new InvalidOperationException($"{nameof(state.OneLoginAuthenticationTicket)} is not set.");
        }

        var subject = state.OneLoginAuthenticationTicket.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");
        var email = state.OneLoginAuthenticationTicket.Principal.FindFirstValue("email") ?? throw new InvalidOperationException("No email claim.");
        var vc = state.OneLoginAuthenticationTicket.Principal.FindFirstValue("vc") is string vcStr ? JsonDocument.Parse(vcStr) : null;

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

                CreateAndAssignPrincipal(state, oneLoginUser.Person.PersonId, oneLoginUser.Person.Trn!);
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
}

using System.Security.Claims;
using System.Text.Json;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.FormFlow.State;

namespace TeachingRecordSystem.AuthorizeAccess;

public class SignInJourneyHelper(TrsDbContext dbContext, IUserInstanceStateProvider userInstanceStateProvider, IClock clock)
{
    public IUserInstanceStateProvider UserInstanceStateProvider { get; } = userInstanceStateProvider;

    public async Task OnSignedInWithOneLogin(SignInJourneyState state, AuthenticationTicket ticket)
    {
        var subject = ticket.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");
        var email = ticket.Principal.FindFirstValue("email") ?? throw new InvalidOperationException("No email claim.");
        var vc = ticket.Principal.FindFirstValue("vc") is string vcStr ? JsonDocument.Parse(vcStr) : null;

        state.Reset();
        state.OneLoginAuthenticationTicket = ticket;

        if (vc is not null)
        {
            state.VerifiedNames = ticket.Principal.GetCoreIdentityNames().Select(n => n.NameParts.Select(part => part.Value).ToArray()).ToArray();
            state.VerifiedDatesOfBirth = ticket.Principal.GetCoreIdentityBirthDates().Select(d => d.Value).ToArray();
        }

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

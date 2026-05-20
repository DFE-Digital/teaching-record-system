using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName, Optional = true)]
public class TestModel(
    IJourneyInstanceProvider journeyInstanceProvider,
    IEventPublisher eventPublisher,
    TimeProvider timeProvider,
    TrsDbContext dbContext) : PageModel
{
    [FromQuery(Name = "scheme")]
    public string? AuthenticationScheme { get; set; }

    [FromQuery(Name = "trn_token")]
    public string? TrnToken { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(AuthenticationScheme))
        {
            return BadRequest();
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            var applicationUser = await dbContext.ApplicationUsers
                .Where(u => u.OneLoginAuthenticationSchemeName == AuthenticationScheme)
                .Select(u => new { u.UserId, u.ClientId, u.RecordMatchingPolicy, u.Name, u.AppContent })
                .SingleOrDefaultAsync();

            if (applicationUser is null)
            {
                return BadRequest($"Application user not found for authentication scheme: {AuthenticationScheme}.");
            }

            var signInJourneyCoordinator = (SignInJourneyCoordinator?)await journeyInstanceProvider.TryCreateNewInstanceAsync(
                HttpContext,
                async ctx =>
                {
                    var processContext = new ProcessContext(ProcessType.TeacherSigningIn, timeProvider.UtcNow, SystemUser.SystemUserId);

                    await eventPublisher.PublishSingleEventAsync(
                        new AuthorizeAccessRequestStartedEvent
                        {
                            EventId = Guid.NewGuid(),
                            ApplicationUserId = applicationUser.UserId,
                            ClientId = applicationUser.ClientId!,
                            JourneyInstanceId = ctx.InstanceId.ToString()
                        },
                        processContext);

                    var redirectUri = ctx.InstanceId.EnsureUrlHasKey(Request.GetEncodedPathAndQuery());

                    var state = new SignInJourneyState(
                        processContext.ProcessId,
                        redirectUri,
                        applicationUser.Name,
                        serviceUrl: Request.GetEncodedUrl(),
                        AuthenticationScheme,
                        clientApplicationUserId: applicationUser.UserId,
                        recordMatchingPolicy: applicationUser.RecordMatchingPolicy,
                        TrnToken,
                        applicationUser.AppContent);

                    return state;
                });
            Debug.Assert(signInJourneyCoordinator is not null);

            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = signInJourneyCoordinator.State.RedirectUri
                },
                AuthenticationSchemes.MatchToTeachingRecord);
        }

        return Page();
    }
}

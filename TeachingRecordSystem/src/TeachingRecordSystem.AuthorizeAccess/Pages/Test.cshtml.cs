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

    [FromQuery(Name = "deferred")]
    public bool Deferred { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(AuthenticationScheme))
        {
            return BadRequest();
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            var clientId = Deferred ? DeferredTestAppConfiguration.ClientId : TestAppConfiguration.ClientId;

            var testAppUser = await dbContext.ApplicationUsers
                .Where(u => u.ClientId == clientId)
                .Select(u => new { u.UserId, u.ClientId, u.RecordMatchingPolicy })
                .SingleOrDefaultAsync();

            if (testAppUser == null)
            {
                return BadRequest($"Test application user not found for {(Deferred ? "deferred" : "required")} matching policy");
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
                            ApplicationUserId = testAppUser.UserId,
                            ClientId = testAppUser.ClientId!,
                            JourneyInstanceId = ctx.InstanceId.ToString()
                        },
                        processContext);

                    var redirectUri = ctx.InstanceId.EnsureUrlHasKey(Request.GetEncodedPathAndQuery());

                    var state = new SignInJourneyState(
                        processContext.ProcessId,
                        redirectUri,
                        "Test service",
                        serviceUrl: Request.GetEncodedUrl(),
                        AuthenticationScheme,
                        clientApplicationUserId: testAppUser.UserId,
                        recordMatchingPolicy: testAppUser.RecordMatchingPolicy,
                        TrnToken);

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

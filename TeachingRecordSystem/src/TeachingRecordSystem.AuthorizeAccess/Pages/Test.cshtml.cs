using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName, Optional = true)]
public class TestModel(IJourneyInstanceProvider journeyInstanceProvider) : PageModel
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
            var signInJourneyCoordinator = (SignInJourneyCoordinator?)await journeyInstanceProvider.TryCreateNewInstanceAsync(
                HttpContext,
                ctx =>
                {
                    var redirectUri = ctx.InstanceId.EnsureUrlHasKey(Request.GetEncodedPathAndQuery());

                    var state = new SignInJourneyState(
                        redirectUri,
                        "Test service",
                        serviceUrl: Request.GetEncodedUrl(),
                        AuthenticationScheme,
                        clientApplicationUserId: Guid.NewGuid(),
                        TrnToken);

                    return Task.FromResult<object>(state);
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

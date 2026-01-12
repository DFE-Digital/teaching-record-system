using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class CheckAnswersModel(SignInJourneyCoordinator coordinator, SupportTaskService supportTaskService, IClock clock) : PageModel
{
    public string? Email { get; set; }

    public string? Name { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string? NationalInsuranceNumber => coordinator.State.NationalInsuranceNumber;

    public string? Trn => coordinator.State.Trn;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var state = coordinator.State;

        var subject = state.OneLoginAuthenticationTicket!.Principal.FindFirstValue("sub")!;
        var email = state.OneLoginAuthenticationTicket!.Principal.FindFirstValue("email")!;

        var processContext = new ProcessContext(
            ProcessType.ConnectOneLoginUserSupportTaskCreating,
            clock.UtcNow,
            SystemUser.SystemUserId);

        await supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.ConnectOneLoginUser,
                Data = new ConnectOneLoginUserData
                {
                    Verified = true,
                    OneLoginUserSubject = subject,
                    OneLoginUserEmail = email,
                    VerifiedNames = state.VerifiedNames,
                    VerifiedDatesOfBirth = state.VerifiedDatesOfBirth,
                    StatedNationalInsuranceNumber = state.NationalInsuranceNumber,
                    StatedTrn = state.Trn,
                    ClientApplicationUserId = state.ClientApplicationUserId,
                    TrnTokenTrn = state.TrnTokenTrn
                },
                PersonId = null,
                OneLoginUserSubject = subject,
                TrnRequest = null
            },
            processContext);

        coordinator.UpdateState(s => s.HasPendingSupportRequest = true);

        return Redirect(coordinator.Links.SupportRequestSubmitted());
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = coordinator.State;

        if (state.OneLoginAuthenticationTicket is null || !state.IdentityVerified)
        {
            // Not authenticated/verified with One Login
            context.Result = BadRequest();
        }
        else if (state.AuthenticationTicket is not null)
        {
            // Already matched to a Teaching Record
            context.Result = Redirect(coordinator.GetRedirectUri());
        }
        else if (!state.HaveNationalInsuranceNumber.HasValue)
        {
            // Not answered the NINO question
            context.Result = Redirect(coordinator.Links.NationalInsuranceNumber());
        }
        else if (!state.HaveTrn.HasValue)
        {
            // Not answered the TRN question
            context.Result = Redirect(coordinator.Links.Trn());
        }

        if (context.Result is null)
        {
            Email = state.OneLoginAuthenticationTicket!.Principal.FindFirstValue("email")!;
            Name = string.Join(" ", state.VerifiedNames!.First());
            DateOfBirth = state.VerifiedDatesOfBirth!.First();
        }
    }
}

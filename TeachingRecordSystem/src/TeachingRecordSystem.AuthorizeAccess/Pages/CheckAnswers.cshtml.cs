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
    public bool IdentityVerified => coordinator.State.IdentityVerified;

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

        if (IdentityVerified)
        {
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
        }
        else
        {
            var processContext = new ProcessContext(
                ProcessType.OneLoginUserIdVerificationSupportTaskCreating,
                clock.UtcNow,
                SystemUser.SystemUserId);

            await supportTaskService.CreateSupportTaskAsync(
                new CreateSupportTaskOptions
                {
                    SupportTaskType = SupportTaskType.OneLoginUserIdVerification,
                    Data = new OneLoginUserIdVerificationData
                    {
                        OneLoginUserSubject = subject,
                        StatedNationalInsuranceNumber = state.NationalInsuranceNumber,
                        StatedTrn = state.Trn,
                        ClientApplicationUserId = state.ClientApplicationUserId,
                        TrnTokenTrn = state.TrnTokenTrn,
                        StatedFirstName = state.FirstName!,
                        StatedLastName = state.LastName!,
                        StatedDateOfBirth = state.DateOfBirth!.Value,
                        EvidenceFileId = default,
                        EvidenceFileName = "TODO"
                    },
                    PersonId = null,
                    OneLoginUserSubject = subject,
                    TrnRequest = null
                },
                processContext);
        }

        return coordinator.AdvanceTo(
            links => links.SupportRequestSubmitted(),
            new PushStepOptions { SetAsFirstStep = true });  // Prevents the user from going back to any page before 'SupportRequestSubmitted'
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = coordinator.State;

        Email = state.OneLoginAuthenticationTicket!.Principal.FindFirstValue("email")!;

        if (state.IdentityVerified)
        {
            Name = string.Join(" ", state.VerifiedNames.First());
            DateOfBirth = state.VerifiedDatesOfBirth.First();
        }
        else
        {
            Name = $"{state.FirstName} {state.LastName}";
            DateOfBirth = state.DateOfBirth!.Value;
        }
    }
}

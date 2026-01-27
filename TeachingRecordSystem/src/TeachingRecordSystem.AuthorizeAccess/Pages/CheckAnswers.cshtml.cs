using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class CheckAnswersModel(SignInJourneyCoordinator coordinator, OneLoginUserMatchingSupportTaskService oneLoginUserMatchingSupportTaskService, IClock clock) : PageModel
{
    public bool IdentityVerified => coordinator.State.IdentityVerified;

    public string? Email { get; set; }

    public string? Name { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string? NationalInsuranceNumber => coordinator.State.NationalInsuranceNumber;

    public string? Trn => coordinator.State.Trn;

    public string? ProofOfIdentityFileName => coordinator.State.ProofOfIdentityFileName;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var state = coordinator.State;

        var subject = state.OneLoginAuthenticationTicket!.Principal.FindFirstValue("sub")!;
        var email = state.OneLoginAuthenticationTicket!.Principal.FindFirstValue("email")!;

        SupportTask supportTask;

        if (IdentityVerified)
        {
            var processContext = new ProcessContext(
                ProcessType.ConnectOneLoginUserSupportTaskCreating,
                clock.UtcNow,
                SystemUser.SystemUserId);

            supportTask = await oneLoginUserMatchingSupportTaskService.CreateRecordMatchingSupportTaskAsync(
                new CreateOneLoginUserRecordMatchingSupportTaskOptions
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
                processContext);
        }
        else
        {
            var processContext = new ProcessContext(
                ProcessType.OneLoginUserIdVerificationSupportTaskCreating,
                clock.UtcNow,
                SystemUser.SystemUserId);

            supportTask = await oneLoginUserMatchingSupportTaskService.CreateVerificationSupportTaskAsync(
                new CreateOneLoginUserIdVerificationSupportTaskOptions
                {
                    OneLoginUserSubject = subject,
                    StatedNationalInsuranceNumber = state.NationalInsuranceNumber,
                    StatedTrn = state.Trn,
                    ClientApplicationUserId = state.ClientApplicationUserId,
                    TrnTokenTrn = state.TrnTokenTrn,
                    StatedFirstName = state.FirstName!,
                    StatedLastName = state.LastName!,
                    StatedDateOfBirth = state.DateOfBirth!.Value,
                    EvidenceFileId = state.ProofOfIdentityFileId!.Value,
                    EvidenceFileName = state.ProofOfIdentityFileName!
                },
                processContext);
        }

        coordinator.UpdateState(s => s.CreatedSupportTaskReference = supportTask.SupportTaskReference);

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

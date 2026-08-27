using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class CheckAnswersModel(
    SignInJourneyCoordinator coordinator,
    ReferenceDataCache referenceDataCache,
    OneLoginUserMatchingSupportTaskService oneLoginUserMatchingSupportTaskService,
    TrsDbContext dbContext,
    TimeProvider timeProvider) : PageModel
{
    public bool IdentityVerified => coordinator.State.IdentityVerified;

    public string? Email { get; set; }

    public string? Name { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string? NationalInsuranceNumber => coordinator.State.NationalInsuranceNumber;

    public string? Trn => coordinator.State.Trn;

    public string? ProofOfIdentityFileName => coordinator.State.ProofOfIdentityFileName;

    public string? YearQtsReceived => coordinator.State.YearQtsReceived;

    public string? QtsTrainingProviderName { get; private set; }

    public string? QtsSubjectName { get; private set; }

    public bool HasQtsDetails => coordinator.State.HaveQts == true;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var state = coordinator.State;

        var subject = state.OneLoginAuthenticationTicket!.Principal.FindFirstValue("sub")!;
        var email = state.OneLoginAuthenticationTicket!.Principal.FindFirstValue("email")!;

        var processContext = await ProcessContext.FromDbAsync(dbContext, state.SigningInProcessId, timeProvider.UtcNow);

        SupportTask supportTask;
        string? trnRequestId = null;

        if (IdentityVerified)
        {
            if (state.RecordMatchingPolicy == RecordMatchingPolicy.Deferred)
            {
                await coordinator.UpdateStateAsync(async state =>
                {
                    trnRequestId = await coordinator.CompleteWithDeferredMatchingAsync(state);
                    return state;
                });
            }

            supportTask = await oneLoginUserMatchingSupportTaskService.CreateRecordMatchingSupportTaskAsync(
                new CreateOneLoginUserRecordMatchingSupportTaskOptions
                {
                    OneLoginUserSubject = subject,
                    OneLoginUserEmailAddress = email,
                    VerifiedNames = state.VerifiedNames,
                    VerifiedDatesOfBirth = state.VerifiedDatesOfBirth,
                    StatedNationalInsuranceNumber = state.NationalInsuranceNumber,
                    StatedTrn = state.Trn,
                    ClientApplicationUserId = state.ClientApplicationUserId,
                    TrnTokenTrn = state.TrnTokenTrn,
                    YearQtsReceived = state.YearQtsReceived,
                    TrainingProviderId = state.QtsTrainingProviderId,
                    TrainingProviderName = QtsTrainingProviderName,
                    SubjectId = state.QtsSubjectId,
                    SubjectName = QtsSubjectName,
                    TrnRequestId = trnRequestId
                },
                processContext);
        }
        else
        {
            supportTask = await oneLoginUserMatchingSupportTaskService.CreateVerificationSupportTaskAsync(
                new CreateOneLoginUserIdVerificationSupportTaskOptions
                {
                    OneLoginUserSubject = subject,
                    OneLoginUserEmailAddress = email,
                    StatedNationalInsuranceNumber = state.NationalInsuranceNumber,
                    StatedTrn = state.Trn,
                    ClientApplicationUserId = state.ClientApplicationUserId,
                    TrnTokenTrn = state.TrnTokenTrn,
                    StatedFirstName = state.FirstName!,
                    StatedLastName = state.LastName!,
                    StatedDateOfBirth = state.DateOfBirth!.Value,
                    EvidenceFileId = state.ProofOfIdentityFileId!.Value,
                    EvidenceFileName = state.ProofOfIdentityFileName!,
                    YearQtsReceived = state.YearQtsReceived,
                    TrainingProviderId = state.QtsTrainingProviderId,
                    TrainingProviderName = QtsTrainingProviderName,
                    SubjectId = state.QtsSubjectId,
                    SubjectName = QtsSubjectName
                },
                processContext);
        }

        coordinator.UpdateState(s => s.CreatedSupportTaskReference = supportTask.SupportTaskReference);

        return coordinator.AdvanceTo(
            links => links.SupportRequestSubmitted(),
            new PushStepOptions { SetAsFirstStep = true });  // Prevents the user from going back to any page before 'SupportRequestSubmitted'
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
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

        if (state.HaveQts == true)
        {
            if (state.QtsTrainingProviderId is { } trainingProviderId)
            {
                QtsTrainingProviderName = (await referenceDataCache.GetTrainingProviderByIdAsync(trainingProviderId)).Name;
            }

            if (state.QtsSubjectId is { } subjectId)
            {
                QtsSubjectName = (await referenceDataCache.GetTrainingSubjectByIdAsync(subjectId)).Name;
            }
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[Journey(JourneyNames.SetStatus), RequireJourneyInstance]
[AllowDeactivatedPerson]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    PersonService personService,
    EvidenceUploadManager evidenceController,
    TimeProvider timeProvider)
    : CommonJourneyPage(personService, linkGenerator, evidenceController)
{
    public PersonDeactivateReason? DeactivateReason { get; set; }
    public string? DeactivateReasonDetail { get; set; }
    public PersonReactivateReason? ReactivateReason { get; set; }
    public string? ReactivateReasonDetail { get; set; }
    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public string BackLink => LinkGenerator.Persons.PersonDetail.SetStatus.Reason(PersonId, TargetStatus, JourneyInstance!.InstanceId);
    public string ChangeReasonLink => LinkGenerator.Persons.PersonDetail.SetStatus.Reason(PersonId, TargetStatus, JourneyInstance!.InstanceId, fromCheckAnswers: true);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (TargetStatus == PersonStatus.Deactivated)
        {
            var processContext = new ProcessContext(
                ProcessType.PersonDeactivating,
                timeProvider.UtcNow,
                User.GetUserId(),
                new ChangeReasonWithDetailsAndEvidence
                {
                    Reason = DeactivateReason?.GetDisplayName(),
                    Details = DeactivateReasonDetail,
                    EvidenceFile = EvidenceFile?.ToEventModel()
                });

            await PersonService.DeactivatePersonAsync(new DeactivatePersonOptions(PersonId, DateOfDeath: null), processContext);
        }
        else
        {
            var processContext = new ProcessContext(
                ProcessType.PersonReactivating,
                timeProvider.UtcNow,
                User.GetUserId(),
                new ChangeReasonWithDetailsAndEvidence
                {
                    Reason = ReactivateReason?.GetDisplayName(),
                    Details = ReactivateReasonDetail,
                    EvidenceFile = EvidenceFile?.ToEventModel()
                });

            await PersonService.ReactivatePersonAsync(PersonId, processContext);
        }

        await JourneyInstance!.CompleteAsync();

        var action = TargetStatus == PersonStatus.Deactivated ? "deactivated" : "reactivated";
        TempData.SetFlashSuccess($"{PersonName}\u2019s record has been {action}");

        return Redirect(LinkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        var state = JourneyInstance!.State;

        if (!state.IsComplete)
        {
            context.Result = Redirect(LinkGenerator.Persons.PersonDetail.SetStatus.Reason(PersonId, TargetStatus, JourneyInstance.InstanceId));
            return;
        }

        DeactivateReason = state.DeactivateReason;
        DeactivateReasonDetail = state.DeactivateReasonDetail;
        ReactivateReason = state.ReactivateReason;
        ReactivateReasonDetail = state.ReactivateReasonDetail;
        EvidenceFile = state.Evidence.UploadedEvidenceFile;
    }
}

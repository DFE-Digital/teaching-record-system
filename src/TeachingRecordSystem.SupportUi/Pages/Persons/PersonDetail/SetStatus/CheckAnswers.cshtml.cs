using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[Journey(JourneyNames.SetStatus)]
[AllowDeactivatedPerson]
public class CheckAnswersModel(
    SetStatusJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    PersonService personService,
    TimeProvider timeProvider) : PageModel
{
    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromRoute]
    public PersonStatus TargetStatus { get; set; }

    public string? PersonName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Trn { get; set; }

    public Gender? Gender { get; set; }

    public string? NationalInsuranceNumber { get; set; }

    public string? EmailAddress { get; set; }

    public PersonStatus? Status { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public PersonDeactivateReason? DeactivateReason { get; set; }
    public string? DeactivateReasonDetail { get; set; }
    public PersonReactivateReason? ReactivateReason { get; set; }
    public string? ReactivateReasonDetail { get; set; }
    public string? ReactivateAdditionalInformation { get; set; }
    public string? DeactivateAdditionalInformation { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

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
                    EvidenceFile = EvidenceFile?.ToEventModel(),
                    AdditionalInformation = DeactivateAdditionalInformation
                });

            await personService.DeactivatePersonAsync(new DeactivatePersonOptions(PersonId, DateOfDeath: null), processContext);
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
                    EvidenceFile = EvidenceFile?.ToEventModel(),
                    AdditionalInformation = ReactivateAdditionalInformation
                });

            await personService.ReactivatePersonAsync(PersonId, processContext);
        }

        journey.DeleteInstance();

        var action = TargetStatus == PersonStatus.Deactivated ? "deactivated" : "reactivated";
        TempData.SetFlashNotificationBanner($"{PersonName}\u2019s record has been {action}");

        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        Status = personInfo.Status;

        var person = await journey.GetPersonAsync();

        if (person is null)
        {
            context.Result = NotFound();
            return;
        }

        if (!journey.StatusChangeIsApplicable(person))
        {
            context.Result = BadRequest();
            return;
        }

        NationalInsuranceNumber = person.NationalInsuranceNumber;
        Trn = person.Trn;
        Gender = person.Gender;
        DateOfBirth = person.DateOfBirth;
        EmailAddress = person.EmailAddress;

        BackLink = journey.GetBackLink();

        var state = journey.State;

        DeactivateReason = state.DeactivateReason;
        DeactivateReasonDetail = state.DeactivateReasonDetail;
        ReactivateReason = state.ReactivateReason;
        ReactivateReasonDetail = state.ReactivateReasonDetail;
        ReactivateAdditionalInformation = state.ReactivateAdditionalInformation;
        DeactivateAdditionalInformation = state.DeactivateAdditionalInformation;
        EvidenceFile = state.Evidence.UploadedEvidenceFile;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

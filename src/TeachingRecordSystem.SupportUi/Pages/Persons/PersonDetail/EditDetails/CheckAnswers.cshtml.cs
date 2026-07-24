using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails)]
public class CheckAnswersModel(
    EditDetailsJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    PersonService personService,
    TimeProvider timeProvider) : PageModel
{
    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public EmailAddress? EmailAddress { get; set; }
    public NationalInsuranceNumber? NationalInsuranceNumber { get; set; }
    public Gender? Gender { get; set; }
    public PersonNameChangeReason? NameChangeReason { get; set; }
    public UploadedEvidenceFile? NameChangeEvidenceFile { get; set; }
    public PersonDetailsChangeReason? OtherDetailsChangeReason { get; set; }
    public string? OtherDetailsChangeReasonDetail { get; set; }
    public UploadedEvidenceFile? OtherDetailsChangeEvidenceFile { get; set; }

    public string Name => string.JoinNonEmpty(' ', FirstName, MiddleName, LastName);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        var processContext = new ProcessContext(
            ProcessType.PersonDetailsUpdating,
            timeProvider.UtcNow,
            User.GetUserId(),
            new Core.Events.ChangeReasons.PersonDetailsChangeReasonInfo
            {
                NameChangeReason = NameChangeReason?.GetDisplayName(),
                NameChangeEvidenceFile = NameChangeEvidenceFile?.ToEventModel(),
                Reason = OtherDetailsChangeReason?.GetDisplayName(),
                Details = OtherDetailsChangeReasonDetail,
                EvidenceFile = OtherDetailsChangeEvidenceFile?.ToEventModel(),
                AdditionalInformation = null
            });

        await personService.UpdatePersonDetailsAsync(
            new UpdatePersonDetailsOptions
            {
                PersonId = PersonId,
                CreatePreviousName = NameChangeReason is PersonNameChangeReason.MarriageOrCivilPartnership or PersonNameChangeReason.DeedPollOrOtherLegalProcess,
                FirstName = Option.Some(FirstName!),
                MiddleName = Option.Some(MiddleName ?? string.Empty),
                LastName = Option.Some(LastName!),
                DateOfBirth = Option.Some(DateOfBirth),
                EmailAddress = Option.Some(EmailAddress),
                NationalInsuranceNumber = Option.Some(NationalInsuranceNumber),
                Gender = Option.Some(Gender)
            },
            processContext);

        journey.DeleteInstance();

        TempData.SetFlashNotificationBanner("Personal details have been updated");

        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        // Changing details from here can introduce a change whose reason hasn't been given yet.
        if (journey.GetUnansweredReasonPageUrl() is string unansweredReasonPageUrl)
        {
            context.Result = Redirect(unansweredReasonPageUrl);
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        BackLink = journey.GetBackLink();

        FirstName = journey.State.FirstName;
        MiddleName = journey.State.MiddleName;
        LastName = journey.State.LastName;
        DateOfBirth = journey.State.DateOfBirth;
        EmailAddress = journey.State.EmailAddress.Parsed;
        NationalInsuranceNumber = journey.State.NationalInsuranceNumber.Parsed;
        Gender = journey.State.Gender;
        NameChangeReason = journey.State.NameChangeReason;
        NameChangeEvidenceFile = journey.State.NameChangeEvidence.UploadedEvidenceFile;
        OtherDetailsChangeReason = journey.State.OtherDetailsChangeReason;
        OtherDetailsChangeReasonDetail = journey.State.OtherDetailsChangeReasonDetail;
        OtherDetailsChangeEvidenceFile = journey.State.OtherDetailsChangeEvidence.UploadedEvidenceFile;
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[Journey(JourneyNames.AddPerson)]
public class CheckAnswersModel(
    AddPersonJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    PersonService personService,
    TimeProvider timeProvider) : PageModel
{
    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public EmailAddress? EmailAddress { get; set; }
    public NationalInsuranceNumber? NationalInsuranceNumber { get; set; }
    public Gender? Gender { get; set; }
    public PersonCreateReason? Reason { get; set; }
    public string? ReasonDetail { get; set; }
    public string? AdditionalInformation { get; set; }
    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public string Name => string.JoinNonEmpty(' ', FirstName, MiddleName, LastName);

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();

        FirstName = journey.State.FirstName;
        MiddleName = journey.State.MiddleName;
        LastName = journey.State.LastName;
        DateOfBirth = journey.State.DateOfBirth;
        EmailAddress = journey.State.EmailAddress.Parsed;
        NationalInsuranceNumber = journey.State.NationalInsuranceNumber.Parsed;
        Gender = journey.State.Gender;
        Reason = journey.State.Reason;
        ReasonDetail = journey.State.ReasonDetail;
        EvidenceFile = journey.State.Evidence.UploadedEvidenceFile;
        AdditionalInformation = journey.State.AdditionalInformation;
    }

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
            ProcessType.PersonCreating,
            timeProvider.UtcNow,
            User.GetUserId(),
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = Reason?.GetDisplayName(),
                Details = ReasonDetail,
                EvidenceFile = EvidenceFile?.ToEventModel(),
                AdditionalInformation = AdditionalInformation
            });

        var person = await personService.CreatePersonAsync(
            new CreatePersonOptions
            {
                SourceTrnRequest = null,
                FirstName = FirstName!,
                MiddleName = MiddleName ?? string.Empty,
                LastName = LastName!,
                DateOfBirth = DateOfBirth,
                EmailAddress = EmailAddress,
                NationalInsuranceNumber = NationalInsuranceNumber,
                Gender = Gender
            },
            processContext);

        journey.DeleteInstance();

        TempData.SetFlashNotificationBanner($"Record created for {Name}");

        return Redirect(linkGenerator.Persons.PersonDetail.Index(person.PersonId));
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[Journey(JourneyNames.AddPerson)]
public class CheckAnswersModel(
    AddPersonJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    PersonService personService,
    TimeProvider timeProvider)
    : CommonJourneyPage(journey, linkGenerator, evidenceUploadManager)
{
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

    public string? BackLink { get; set; }

    public string ChangePersonalDetailsLink(string returnUrl) =>
        GetPageLink(AddPersonJourneyPage.PersonalDetails, returnUrl);

    public string ChangeReasonLink(string returnUrl) =>
        GetPageLink(AddPersonJourneyPage.Reason, returnUrl);

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = Journey.GetBackLink();

        FirstName = Journey.State.FirstName;
        MiddleName = Journey.State.MiddleName;
        LastName = Journey.State.LastName;
        DateOfBirth = Journey.State.DateOfBirth;
        EmailAddress = Journey.State.EmailAddress.Parsed;
        NationalInsuranceNumber = Journey.State.NationalInsuranceNumber.Parsed;
        Gender = Journey.State.Gender;
        Reason = Journey.State.Reason;
        ReasonDetail = Journey.State.ReasonDetail;
        EvidenceFile = Journey.State.Evidence.UploadedEvidenceFile;
        AdditionalInformation = Journey.State.AdditionalInformation;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
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

        Journey.DeleteInstance();

        TempData.SetFlashNotificationBanner($"Record created for {Name}");

        return Redirect(LinkGenerator.Persons.PersonDetail.Index(person.PersonId));
    }
}

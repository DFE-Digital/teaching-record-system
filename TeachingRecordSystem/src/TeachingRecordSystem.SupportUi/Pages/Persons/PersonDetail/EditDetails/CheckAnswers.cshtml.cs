using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    PersonService personService,
    EvidenceUploadManager evidenceUploadManager,
    IClock clock)
    : CommonJourneyPage(personService, linkGenerator, evidenceUploadManager)
{
    private Person? _person;

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

    public string Name => StringHelper.JoinNonEmpty(' ', FirstName, MiddleName, LastName);

    public string ChangePersonalDetailsLink =>
        GetPageLink(EditDetailsJourneyPage.PersonalDetails, true);

    public string ChangeNameChangeReasonLink =>
        GetPageLink(EditDetailsJourneyPage.NameChangeReason, true);

    public string ChangeDetailsChangeReasonLink =>
        GetPageLink(EditDetailsJourneyPage.OtherDetailsChangeReason, true);

    public string BackLink => GetPageLink(
        OtherDetailsChangeReason is not null
            ? EditDetailsJourneyPage.OtherDetailsChangeReason
            : EditDetailsJourneyPage.NameChangeReason);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var processContext = new ProcessContext(
            ProcessType.PersonDetailsUpdating,
            clock.UtcNow,
            User.GetUserId(),
            new Core.Events.ChangeReasons.PersonDetailsChangeReasonInfo
            {
                NameChangeReason = NameChangeReason?.ToString(),
                NameChangeEvidenceFile = NameChangeEvidenceFile?.ToEventModel(),
                Reason = OtherDetailsChangeReason?.ToString(),
                Details = OtherDetailsChangeReasonDetail,
                EvidenceFile = OtherDetailsChangeEvidenceFile?.ToEventModel()
            });

        await PersonService.UpdatePersonDetailsAsync(
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

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Personal details have been updated");

        return Redirect(LinkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete && NextIncompletePage < EditDetailsJourneyPage.CheckAnswers)
        {
            context.Result = Redirect(GetPageLink(NextIncompletePage));
            return;
        }

        _person = await PersonService.GetPersonAsync(PersonId);

        if (_person is null)
        {
            context.Result = NotFound();
            return;
        }

        FirstName = JourneyInstance!.State.FirstName;
        MiddleName = JourneyInstance.State.MiddleName;
        LastName = JourneyInstance.State.LastName;
        DateOfBirth = JourneyInstance.State.DateOfBirth;
        EmailAddress = JourneyInstance.State.EmailAddress.Parsed;
        NationalInsuranceNumber = JourneyInstance.State.NationalInsuranceNumber.Parsed;
        Gender = JourneyInstance.State.Gender;
        NameChangeReason = JourneyInstance.State.NameChangeReason;
        NameChangeEvidenceFile = JourneyInstance.State.NameChangeEvidence.UploadedEvidenceFile;
        OtherDetailsChangeReason = JourneyInstance.State.OtherDetailsChangeReason;
        OtherDetailsChangeReasonDetail = JourneyInstance.State.OtherDetailsChangeReasonDetail;
        OtherDetailsChangeEvidenceFile = JourneyInstance.State.OtherDetailsChangeEvidence.UploadedEvidenceFile;
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[Journey(JourneyNames.AddPerson), RequireJourneyInstance]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    PersonService personService,
    IClock clock)
    : CommonJourneyPage(linkGenerator, evidenceUploadManager)
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
    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public string Name => StringHelper.JoinNonEmpty(' ', FirstName, MiddleName, LastName);

    public string? ChangePersonalDetailsLink =>
        GetPageLink(AddPersonJourneyPage.PersonalDetails, true);

    public string? ChangeReasonLink =>
        GetPageLink(AddPersonJourneyPage.Reason, true);

    public string BackLink => GetPageLink(AddPersonJourneyPage.Reason);

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete && NextIncompletePage < AddPersonJourneyPage.CheckAnswers)
        {
            context.Result = Redirect(GetPageLink(NextIncompletePage));
            return;
        }

        FirstName = JourneyInstance!.State.FirstName;
        MiddleName = JourneyInstance.State.MiddleName;
        LastName = JourneyInstance.State.LastName;
        DateOfBirth = JourneyInstance.State.DateOfBirth;
        EmailAddress = JourneyInstance.State.EmailAddress.Parsed;
        NationalInsuranceNumber = JourneyInstance.State.NationalInsuranceNumber.Parsed;
        Gender = JourneyInstance.State.Gender;
        Reason = JourneyInstance.State.Reason;
        ReasonDetail = JourneyInstance.State.ReasonDetail;
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var processContext = new ProcessContext(ProcessType.PersonCreating, clock.UtcNow, User.GetUserId());

        var personId = await personService.CreatePersonAsync(new(
            new()
            {
                FirstName = FirstName ?? string.Empty,
                MiddleName = MiddleName ?? string.Empty,
                LastName = LastName ?? string.Empty,
                DateOfBirth = DateOfBirth,
                EmailAddress = EmailAddress,
                NationalInsuranceNumber = NationalInsuranceNumber,
                Gender = Gender,
            },
            new()
            {
                Reason = Reason!.Value,
                ReasonDetail = ReasonDetail,
                Evidence = EvidenceFile?.ToFile()
            }), processContext);

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess($"Record created for {Name}",
            buildMessageHtml: LinkTagBuilder.BuildViewRecordLink(LinkGenerator.Persons.PersonDetail.Index(personId))
            );

        return Redirect(LinkGenerator.Persons.PersonDetail.Index(personId));
    }
}

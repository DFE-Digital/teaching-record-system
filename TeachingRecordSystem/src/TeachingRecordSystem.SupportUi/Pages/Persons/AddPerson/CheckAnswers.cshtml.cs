using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[Journey(JourneyNames.AddPerson), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IClock clock,
    ITrnGenerator trnGenerator,
    EvidenceController evidenceController)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public EmailAddress? EmailAddress { get; set; }
    public NationalInsuranceNumber? NationalInsuranceNumber { get; set; }
    public Gender? Gender { get; set; }
    public AddPersonReasonOption? Reason { get; set; }
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
        var now = clock.UtcNow;

        var trn = await trnGenerator.GenerateTrnAsync();

        var (person, personAttributes) = Person.Create(
            trn,
            FirstName ?? string.Empty,
            MiddleName ?? string.Empty,
            LastName ?? string.Empty,
            DateOfBirth,
            EmailAddress,
            NationalInsuranceNumber,
            Gender,
            now);

        var createdEvent = new PersonCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = User.GetUserId(),
            PersonId = person.PersonId,
            PersonAttributes = personAttributes,
            CreateReason = Reason?.GetDisplayName(),
            CreateReasonDetail = ReasonDetail,
            EvidenceFile = EvidenceFile?.ToEventModel(),
            TrnRequestMetadata = null
        };

        DbContext.Add(person);
        await DbContext.AddEventAndBroadcastAsync(createdEvent);
        await DbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess($"Record created for {Name}",
            buildMessageHtml: LinkTagBuilder.BuildViewRecordLink(LinkGenerator.PersonDetail(person.PersonId))
            );

        return Redirect(LinkGenerator.PersonDetail(person.PersonId));
    }
}

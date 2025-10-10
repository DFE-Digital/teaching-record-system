using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IClock clock,
    EvidenceUploadManager evidenceUploadManager)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceUploadManager)
{
    private Person? _person;

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public EmailAddress? EmailAddress { get; set; }
    public NationalInsuranceNumber? NationalInsuranceNumber { get; set; }
    public Gender? Gender { get; set; }
    public EditDetailsNameChangeReasonOption? NameChangeReason { get; set; }
    public UploadedEvidenceFile? NameChangeEvidenceFile { get; set; }
    public EditDetailsOtherDetailsChangeReasonOption? OtherDetailsChangeReason { get; set; }
    public string? OtherDetailsChangeReasonDetail { get; set; }
    public UploadedEvidenceFile? OtherDetailsChangeEvidenceFile { get; set; }

    public string Name => StringHelper.JoinNonEmpty(' ', FirstName, MiddleName, LastName);

    public string? ChangePersonalDetailsLink =>
        GetPageLink(EditDetailsJourneyPage.PersonalDetails, true);

    public string? ChangeNameChangeReasonLink =>
        GetPageLink(EditDetailsJourneyPage.NameChangeReason, true);

    public string? ChangeDetailsChangeReasonLink =>
        GetPageLink(EditDetailsJourneyPage.OtherDetailsChangeReason, true);

    public string BackLink => GetPageLink(
        OtherDetailsChangeReason is not null
            ? EditDetailsJourneyPage.OtherDetailsChangeReason
            : EditDetailsJourneyPage.NameChangeReason);

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete && NextIncompletePage < EditDetailsJourneyPage.CheckAnswers)
        {
            context.Result = Redirect(GetPageLink(NextIncompletePage));
            return;
        }

        _person = await DbContext.Persons.SingleOrDefaultAsync(u => u.PersonId == PersonId);

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

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var now = clock.UtcNow;

        var updateResult = _person!.UpdateDetails(
            Option.Some(FirstName ?? string.Empty),
            Option.Some(MiddleName ?? string.Empty),
            Option.Some(LastName ?? string.Empty),
            Option.Some(DateOfBirth),
            Option.Some(EmailAddress),
            Option.Some(NationalInsuranceNumber),
            Option.Some(Gender),
            now);

        var updatedEvent = updateResult.Changes != 0 ?
            new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = User.GetUserId(),
                PersonId = PersonId,
                PersonAttributes = updateResult.PersonAttributes,
                OldPersonAttributes = updateResult.OldPersonAttributes,
                NameChangeReason = NameChangeReason?.GetDisplayName(),
                NameChangeEvidenceFile = NameChangeEvidenceFile?.ToEventModel(),
                DetailsChangeReason = OtherDetailsChangeReason?.GetDisplayName(),
                DetailsChangeReasonDetail = OtherDetailsChangeReasonDetail,
                DetailsChangeEvidenceFile = OtherDetailsChangeEvidenceFile?.ToEventModel(),
                Changes = (PersonDetailsUpdatedEventChanges)updateResult.Changes
            } :
            null;

        if (updatedEvent is not null &&
            updatedEvent.Changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange) &&
            NameChangeReason is EditDetailsNameChangeReasonOption.MarriageOrCivilPartnership or EditDetailsNameChangeReasonOption.DeedPollOrOtherLegalProcess)
        {
            DbContext.PreviousNames.Add(new PreviousName
            {
                PreviousNameId = Guid.NewGuid(),
                PersonId = PersonId,
                FirstName = updatedEvent.OldPersonAttributes.FirstName,
                MiddleName = updatedEvent.OldPersonAttributes.MiddleName,
                LastName = updatedEvent.OldPersonAttributes.LastName,
                CreatedOn = now,
                UpdatedOn = now
            });
        }

        if (updatedEvent is not null)
        {
            await DbContext.AddEventAndBroadcastAsync(updatedEvent);
            await DbContext.SaveChangesAsync();
        }

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Personal details have been updated");

        return Redirect(LinkGenerator.Persons.PersonDetail.Index(PersonId));
    }
}

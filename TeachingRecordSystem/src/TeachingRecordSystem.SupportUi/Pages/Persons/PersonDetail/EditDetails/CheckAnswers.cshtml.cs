using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[RequireFeatureEnabledFilterFactory(FeatureNames.ContactsMigrated)]
[Journey(JourneyNames.EditDetails), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IClock clock,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
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
    public Guid? NameChangeEvidenceFileId { get; set; }
    public string? NameChangeEvidenceFileName { get; set; }
    public string? NameChangeEvidenceFileSizeDescription { get; set; }
    public string? NameChangeUploadedEvidenceFileUrl { get; set; }
    public EditDetailsOtherDetailsChangeReasonOption? OtherDetailsChangeReason { get; set; }
    public Guid? OtherDetailsChangeEvidenceFileId { get; set; }
    public string? OtherDetailsChangeReasonDetail { get; set; }
    public string? OtherDetailsChangeEvidenceFileName { get; set; }
    public string? OtherDetailsChangeEvidenceFileSizeDescription { get; set; }
    public string? OtherDetailsChangeUploadedEvidenceFileUrl { get; set; }

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

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var now = clock.UtcNow;

        var updateResult = _person!.UpdateDetails(
            FirstName ?? string.Empty,
            MiddleName ?? string.Empty,
            LastName ?? string.Empty,
            DateOfBirth,
            EmailAddress,
            NationalInsuranceNumber,
            Gender,
            now);

        var updatedEvent = updateResult.Changes != 0 ?
            new PersonDetailsUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = User.GetUserId(),
                PersonId = PersonId,
                PersonAttributes = updateResult.PersonAttributes,
                OldPersonAttributes = updateResult.OldPersonAttributes,
                NameChangeReason = NameChangeReason?.GetDisplayName(),
                NameChangeEvidenceFile = NameChangeEvidenceFileId is Guid nameFileId
                    ? new EventModels.File()
                    {
                        FileId = nameFileId,
                        Name = NameChangeEvidenceFileName!
                    }
                    : null,
                DetailsChangeReason = OtherDetailsChangeReason?.GetDisplayName(),
                DetailsChangeReasonDetail = OtherDetailsChangeReasonDetail,
                DetailsChangeEvidenceFile = OtherDetailsChangeEvidenceFileId is Guid detailsFileId
                    ? new EventModels.File()
                    {
                        FileId = detailsFileId,
                        Name = OtherDetailsChangeEvidenceFileName!
                    }
                    : null,
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

        TempData.SetFlashSuccess(messageText: "Personal details have been updated successfully.");

        return Redirect(LinkGenerator.PersonDetail(PersonId));
    }

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
        NameChangeEvidenceFileId = JourneyInstance.State.NameChangeEvidenceFileId;
        NameChangeEvidenceFileName = JourneyInstance.State.NameChangeEvidenceFileName;
        NameChangeUploadedEvidenceFileUrl = JourneyInstance.State.NameChangeEvidenceFileId is not null ?
            await FileService.GetFileUrlAsync(JourneyInstance.State.NameChangeEvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
        OtherDetailsChangeReason = JourneyInstance.State.OtherDetailsChangeReason;
        OtherDetailsChangeReasonDetail = JourneyInstance.State.OtherDetailsChangeReasonDetail;
        OtherDetailsChangeEvidenceFileId = JourneyInstance.State.OtherDetailsChangeEvidenceFileId;
        OtherDetailsChangeEvidenceFileName = JourneyInstance.State.OtherDetailsChangeEvidenceFileName;
        OtherDetailsChangeUploadedEvidenceFileUrl = JourneyInstance.State.OtherDetailsChangeEvidenceFileId is not null ?
            await FileService.GetFileUrlAsync(JourneyInstance.State.OtherDetailsChangeEvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
    }
}

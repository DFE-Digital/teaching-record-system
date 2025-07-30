using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.ManualMerge;

[RequireFeatureEnabledFilterFactory(FeatureNames.ContactsMigrated)]
[Journey(JourneyNames.ManualMergePerson), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService,
    IClock clock)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    public string BackLink => GetPageLink(ManualMergeJourneyPage.Merge);
    public string ChangePrimaryRecordLink => GetPageLink(ManualMergeJourneyPage.Matches, fromCheckAnswers: true);
    public string ChangeDetailsLink => GetPageLink(ManualMergeJourneyPage.Merge, fromCheckAnswers: true);

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? EmailAddress { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public Gender? Gender { get; set; }
    public string? Trn { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }
    public string? EvidenceFileUrl { get; set; }
    public string? Comments { get; set; }

    private IReadOnlyList<PotentialDuplicate>? _potentialDuplicates;

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        var state = JourneyInstance!.State;

        if (state.PersonAId is not Guid personAId || state.PersonBId is not Guid personBId)
        {
            context.Result = Redirect(GetPageLink(ManualMergeJourneyPage.EnterTrn));
            return;
        }

        if (state.PrimaryRecordId is not Guid primaryRecordId)
        {
            context.Result = Redirect(GetPageLink(ManualMergeJourneyPage.Matches));
            return;
        }

        if (state.PersonAttributeSourcesSet is false)
        {
            context.Result = Redirect(GetPageLink(ManualMergeJourneyPage.Merge));
            return;
        }

        _potentialDuplicates = await GetPotentialDuplicatesAsync(personAId, personBId);

        var secondaryRecordId = primaryRecordId == personAId ? personBId : personAId;

        var primaryRecord = _potentialDuplicates.Single(p => p.PersonId == primaryRecordId);
        var secondaryRecord = _potentialDuplicates.Single(p => p.PersonId == secondaryRecordId);

        FirstName = state.FirstNameSource == PersonAttributeSource.PrimaryRecord ? primaryRecord.FirstName : secondaryRecord.FirstName;
        MiddleName = state.MiddleNameSource == PersonAttributeSource.PrimaryRecord ? primaryRecord.MiddleName : secondaryRecord.MiddleName;
        LastName = state.LastNameSource == PersonAttributeSource.PrimaryRecord ? primaryRecord.LastName : secondaryRecord.LastName;
        DateOfBirth = state.DateOfBirthSource == PersonAttributeSource.PrimaryRecord ? primaryRecord.DateOfBirth : secondaryRecord.DateOfBirth;
        EmailAddress = state.EmailAddressSource == PersonAttributeSource.PrimaryRecord ? primaryRecord.EmailAddress : secondaryRecord.EmailAddress;
        NationalInsuranceNumber = state.NationalInsuranceNumberSource == PersonAttributeSource.PrimaryRecord ? primaryRecord.NationalInsuranceNumber : secondaryRecord.NationalInsuranceNumber;
        Gender = state.GenderSource == PersonAttributeSource.PrimaryRecord ? primaryRecord.Gender : secondaryRecord.Gender;
        Trn = primaryRecord.Trn;
        EvidenceFileId = state.EvidenceFileId;
        EvidenceFileName = state.EvidenceFileName;
        EvidenceFileUrl = state.EvidenceFileId is not null ?
            await FileService.GetFileUrlAsync(state.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
        Comments = state.Comments;
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (_potentialDuplicates!.Any(p => p.IsInvalid))
        {
            return BadRequest();
        }

        var state = JourneyInstance!.State;
        var primaryRecordId = state.PrimaryRecordId!.Value;
        var secondaryRecordId = primaryRecordId == state.PersonAId ? state.PersonBId : state.PersonAId;

        var primaryRecord = _potentialDuplicates!.Single(p => p.PersonId == primaryRecordId);
        var secondaryRecord = _potentialDuplicates!.Single(p => p.PersonId == secondaryRecordId);

        var oldPersonAttributes = primaryRecord.Attributes;
        var newPersonAttributes = new PersonAttributes()
        {
            FirstName = FirstName ?? string.Empty,
            MiddleName = MiddleName ?? string.Empty,
            LastName = LastName ?? string.Empty,
            DateOfBirth = DateOfBirth,
            EmailAddress = EmailAddress,
            NationalInsuranceNumber = NationalInsuranceNumber,
            Gender = Gender,
        };

        // update the person
        var person = await DbContext.Persons.SingleAsync(p => p.PersonId == primaryRecordId);

        person.UpdateDetails(
            firstName: newPersonAttributes.FirstName,
            middleName: newPersonAttributes.MiddleName,
            lastName: newPersonAttributes.LastName,
            dateOfBirth: newPersonAttributes.DateOfBirth,
            emailAddress: newPersonAttributes.EmailAddress is not null ? Core.EmailAddress.Parse(newPersonAttributes.EmailAddress) : null,
            nationalInsuranceNumber: newPersonAttributes.NationalInsuranceNumber is not null ? Core.NationalInsuranceNumber.Parse(newPersonAttributes.NationalInsuranceNumber) : null,
            gender: newPersonAttributes.Gender,
            clock.UtcNow);

        var changes = PersonsMergedEventChanges.None |
            (state.FirstNameSource is PersonAttributeSource.SecondaryRecord ? PersonsMergedEventChanges.FirstName : 0) |
            (state.MiddleNameSource is PersonAttributeSource.SecondaryRecord ? PersonsMergedEventChanges.MiddleName : 0) |
            (state.LastNameSource is PersonAttributeSource.SecondaryRecord ? PersonsMergedEventChanges.LastName : 0) |
            (state.DateOfBirthSource is PersonAttributeSource.SecondaryRecord ? PersonsMergedEventChanges.DateOfBirth : 0) |
            (state.EmailAddressSource is PersonAttributeSource.SecondaryRecord ? PersonsMergedEventChanges.EmailAddress : 0) |
            (state.NationalInsuranceNumberSource is PersonAttributeSource.SecondaryRecord ? PersonsMergedEventChanges.NationalInsuranceNumber : 0) |
            (state.GenderSource is PersonAttributeSource.SecondaryRecord ? PersonsMergedEventChanges.Gender : 0);

        var @event = new PersonsMergedEvent()
        {
            PersonId = primaryRecordId,
            PrimaryRecordTrn = primaryRecord.Trn,
            SecondaryRecordTrn = secondaryRecord.Trn,
            Changes = changes,
            PersonAttributes = newPersonAttributes,
            OldPersonAttributes = oldPersonAttributes,
            EvidenceFile = EvidenceFileId is Guid nameFileId
                ? new EventModels.File()
                {
                    FileId = nameFileId,
                    Name = EvidenceFileName!
                }
                : null,
            Comments = Comments,
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = User.GetUserId()
        };

        var otherPerson = await DbContext.Persons.SingleAsync(p => p.PersonId == secondaryRecordId);
        otherPerson.Status = PersonStatus.Deactivated;

        await DbContext.AddEventAndBroadcastAsync(@event);
        await DbContext.SaveChangesAsync();

        // This is a little ugly but pushing this into a partial and executing it here is tricky
        var flashMessageHtml =
            $@"
            <a href=""{LinkGenerator.PersonDetail(primaryRecordId)}"" class=""govuk-link"">View record</a>
            ";

        TempData.SetFlashSuccess(
            $"Records merged successfully for {FirstName} {MiddleName} {LastName}",
            messageHtml: flashMessageHtml);

        await JourneyInstance!.CompleteAsync();

        return Redirect(GetPageLink(null));
    }
}

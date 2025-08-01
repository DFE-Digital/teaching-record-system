using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public string ChangePrimaryPersonLink => GetPageLink(ManualMergeJourneyPage.Matches, fromCheckAnswers: true);
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

        if (state.PrimaryPersonId is not Guid primaryPersonId)
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

        var secondaryPersonId = primaryPersonId == personAId ? personBId : personAId;

        var primaryPerson = _potentialDuplicates.Single(p => p.PersonId == primaryPersonId);
        var secondaryPerson = _potentialDuplicates.Single(p => p.PersonId == secondaryPersonId);

        FirstName = state.FirstNameSource == PersonAttributeSource.PrimaryPerson ? primaryPerson.FirstName : secondaryPerson.FirstName;
        MiddleName = state.MiddleNameSource == PersonAttributeSource.PrimaryPerson ? primaryPerson.MiddleName : secondaryPerson.MiddleName;
        LastName = state.LastNameSource == PersonAttributeSource.PrimaryPerson ? primaryPerson.LastName : secondaryPerson.LastName;
        DateOfBirth = state.DateOfBirthSource == PersonAttributeSource.PrimaryPerson ? primaryPerson.DateOfBirth : secondaryPerson.DateOfBirth;
        EmailAddress = state.EmailAddressSource == PersonAttributeSource.PrimaryPerson ? primaryPerson.EmailAddress : secondaryPerson.EmailAddress;
        NationalInsuranceNumber = state.NationalInsuranceNumberSource == PersonAttributeSource.PrimaryPerson ? primaryPerson.NationalInsuranceNumber : secondaryPerson.NationalInsuranceNumber;
        Gender = state.GenderSource == PersonAttributeSource.PrimaryPerson ? primaryPerson.Gender : secondaryPerson.Gender;
        Trn = primaryPerson.Trn;
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
        var primaryPersonId = state.PrimaryPersonId!.Value;
        var secondaryPersonId = primaryPersonId == state.PersonAId ? state.PersonBId!.Value : state.PersonAId!.Value;

        var primaryPersonAttributes = _potentialDuplicates!.Single(p => p.PersonId == primaryPersonId).Attributes;

        var newPrimaryPersonAttributes = new PersonAttributes()
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
        var primaryPerson = await DbContext.Persons.SingleAsync(p => p.PersonId == primaryPersonId);

        primaryPerson.UpdateDetails(
            firstName: newPrimaryPersonAttributes.FirstName,
            middleName: newPrimaryPersonAttributes.MiddleName,
            lastName: newPrimaryPersonAttributes.LastName,
            dateOfBirth: newPrimaryPersonAttributes.DateOfBirth,
            emailAddress: newPrimaryPersonAttributes.EmailAddress is not null ? Core.EmailAddress.Parse(newPrimaryPersonAttributes.EmailAddress) : null,
            nationalInsuranceNumber: newPrimaryPersonAttributes.NationalInsuranceNumber is not null ? Core.NationalInsuranceNumber.Parse(newPrimaryPersonAttributes.NationalInsuranceNumber) : null,
            gender: newPrimaryPersonAttributes.Gender,
            clock.UtcNow);

        var changes = PersonsMergedEventChanges.None |
            (state.FirstNameSource is PersonAttributeSource.SecondaryPerson ? PersonsMergedEventChanges.FirstName : 0) |
            (state.MiddleNameSource is PersonAttributeSource.SecondaryPerson ? PersonsMergedEventChanges.MiddleName : 0) |
            (state.LastNameSource is PersonAttributeSource.SecondaryPerson ? PersonsMergedEventChanges.LastName : 0) |
            (state.DateOfBirthSource is PersonAttributeSource.SecondaryPerson ? PersonsMergedEventChanges.DateOfBirth : 0) |
            (state.EmailAddressSource is PersonAttributeSource.SecondaryPerson ? PersonsMergedEventChanges.EmailAddress : 0) |
            (state.NationalInsuranceNumberSource is PersonAttributeSource.SecondaryPerson ? PersonsMergedEventChanges.NationalInsuranceNumber : 0) |
            (state.GenderSource is PersonAttributeSource.SecondaryPerson ? PersonsMergedEventChanges.Gender : 0);

        var secondaryPerson = await DbContext.Persons.SingleAsync(p => p.PersonId == secondaryPersonId);
        secondaryPerson.Status = PersonStatus.Deactivated;

        var @event = new PersonsMergedEvent()
        {
            PersonId = primaryPersonId,
            PersonTrn = primaryPerson.Trn!,
            SecondaryPersonId = secondaryPersonId,
            SecondaryPersonTrn = secondaryPerson.Trn!,
            SecondaryPersonStatus = secondaryPerson.Status,
            PersonAttributes = newPrimaryPersonAttributes,
            OldPersonAttributes = primaryPersonAttributes,
            EvidenceFile = EvidenceFileId is Guid nameFileId
                ? new EventModels.File()
                {
                    FileId = nameFileId,
                    Name = EvidenceFileName!
                }
                : null,
            Comments = Comments,
            Changes = changes,
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = User.GetUserId()
        };

        await DbContext.AddEventAndBroadcastAsync(@event);
        await DbContext.SaveChangesAsync();

        TempData.SetFlashSuccess(
            $"Records merged successfully for {FirstName} {MiddleName} {LastName}",
            buildMessageHtml: b =>
            {
                var link = new TagBuilder("a");
                link.AddCssClass("govuk-link");
                link.MergeAttribute("href", LinkGenerator.PersonDetail(primaryPersonId));
                link.InnerHtml.Append("View record");
                b.AppendHtml(link);
            });

        await JourneyInstance!.CompleteAsync();

        return Redirect(GetPageLink(null));
    }
}

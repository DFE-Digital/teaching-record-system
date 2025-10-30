using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;
using TeachingRecordSystem.SupportUi.Services;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

[Journey(JourneyNames.MergePerson), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    IClock clock,
    PersonChangeableAttributesService changedService)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceUploadManager)
{
    public string BackLink => GetPageLink(MergePersonJourneyPage.Merge);
    public string ChangePrimaryPersonLink => GetPageLink(MergePersonJourneyPage.Matches, fromCheckAnswers: true);
    public string ChangeDetailsLink => GetPageLink(MergePersonJourneyPage.Merge, fromCheckAnswers: true);

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? EmailAddress { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public Gender? Gender { get; set; }
    public string? Trn { get; set; }
    public UploadedEvidenceFile? EvidenceFile { get; set; }
    public string? Comments { get; set; }

    private IReadOnlyList<PotentialDuplicate>? _potentialDuplicates;

    public IEnumerable<ResolvedMergedAttribute>? ResolvableAttributes { get; private set; }

    public bool IsGenderChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.Gender) == true;

    public bool IsFirstNameChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.FirstName) == true;

    public bool IsMiddleNameChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.MiddleName) == true;

    public bool IsLastNameChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.LastName) == true;

    public bool IsDateOfBirthChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.DateOfBirth) == true;

    public bool IsNationalInsuranceNumberChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.NationalInsuranceNumber) == true;

    public bool IsEmailAddressChangeable => ResolvableAttributes?.Any(r => r.Attribute == PersonMatchedAttribute.EmailAddress) == true;

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        var state = JourneyInstance!.State;

        if (state.PersonAId is not Guid personAId || state.PersonBId is not Guid personBId)
        {
            context.Result = Redirect(GetPageLink(MergePersonJourneyPage.EnterTrn));
            return;
        }

        if (state.PrimaryPersonId is not Guid primaryPersonId)
        {
            context.Result = Redirect(GetPageLink(MergePersonJourneyPage.Matches));
            return;
        }

        if (state.PersonAttributeSourcesSet is false ||
            !state.Evidence.IsComplete)
        {
            context.Result = Redirect(GetPageLink(MergePersonJourneyPage.Merge));
            return;
        }

        _potentialDuplicates = await GetPotentialDuplicatesAsync(personAId, personBId);

        ResolvableAttributes = changedService.GetResolvableMergedAttributes(
             new List<ResolvedMergedAttribute>
             {
                 new ResolvedMergedAttribute(PersonMatchedAttribute.Gender, state.GenderSource),
                 new ResolvedMergedAttribute(PersonMatchedAttribute.FirstName, state.FirstNameSource),
                 new ResolvedMergedAttribute(PersonMatchedAttribute.MiddleName, state.MiddleNameSource),
                 new ResolvedMergedAttribute(PersonMatchedAttribute.LastName, state.LastNameSource),
                 new ResolvedMergedAttribute(PersonMatchedAttribute.DateOfBirth, state.DateOfBirthSource),
                 new ResolvedMergedAttribute(PersonMatchedAttribute.NationalInsuranceNumber, state.NationalInsuranceNumberSource),
                 new ResolvedMergedAttribute(PersonMatchedAttribute.EmailAddress, state.EmailAddressSource)
             });

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
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;
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

        var newPrimaryPersonAttributes = new PersonDetails()
        {
            FirstName = FirstName ?? string.Empty,
            MiddleName = MiddleName ?? string.Empty,
            LastName = LastName ?? string.Empty,
            DateOfBirth = DateOfBirth,
            EmailAddress = EmailAddress,
            NationalInsuranceNumber = NationalInsuranceNumber,
            Gender = Gender
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
        secondaryPerson.MergedWithPersonId = primaryPersonId;

        var @event = new PersonsMergedEvent()
        {
            PersonId = primaryPersonId,
            PersonTrn = primaryPerson.Trn,
            SecondaryPersonId = secondaryPersonId,
            SecondaryPersonTrn = secondaryPerson.Trn,
            SecondaryPersonStatus = secondaryPerson.Status,
            PersonAttributes = newPrimaryPersonAttributes,
            OldPersonAttributes = primaryPersonAttributes,
            EvidenceFile = EvidenceFile?.ToEventModel(),
            Comments = Comments,
            Changes = changes,
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = User.GetUserId()
        };

        await DbContext.AddEventAndBroadcastAsync(@event);
        await DbContext.SaveChangesAsync();

        TempData.SetFlashSuccess(
            $"Records merged for {StringHelper.JoinNonEmpty(' ', FirstName, MiddleName, LastName)}",
            buildMessageHtml: LinkTagBuilder.BuildViewRecordLink(LinkGenerator.Persons.PersonDetail.Index(primaryPersonId))
            );


        await JourneyInstance!.CompleteAsync();

        return Redirect(GetPageLink(null));
    }
}

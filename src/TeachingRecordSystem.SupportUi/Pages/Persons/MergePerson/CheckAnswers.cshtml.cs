using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;
using TeachingRecordSystem.SupportUi.Services;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

[Journey(JourneyNames.MergePerson)]
public class CheckAnswersModel(
    MergePersonJourneyCoordinator journey,
    PersonService personService,
    SupportUiLinkGenerator linkGenerator,
    TimeProvider timeProvider,
    PersonChangeableAttributesService changedService) : PageModel
{
    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

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

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        BackLink = journey.GetBackLink();

        var state = journey.State;
        var personAId = state.PersonAId!.Value;
        var personBId = state.PersonBId!.Value;
        var primaryPersonId = state.PrimaryPersonId!.Value;

        _potentialDuplicates = await journey.GetPotentialDuplicatesAsync(personAId, personBId);

        ResolvableAttributes = changedService.GetResolvableMergedAttributes(
             new List<ResolvedMergedAttribute>
             {
                 new(PersonMatchedAttribute.Gender, state.GenderSource),
                 new(PersonMatchedAttribute.FirstName, state.FirstNameSource),
                 new(PersonMatchedAttribute.MiddleName, state.MiddleNameSource),
                 new(PersonMatchedAttribute.LastName, state.LastNameSource),
                 new(PersonMatchedAttribute.DateOfBirth, state.DateOfBirthSource),
                 new(PersonMatchedAttribute.NationalInsuranceNumber, state.NationalInsuranceNumberSource),
                 new(PersonMatchedAttribute.EmailAddress, state.EmailAddressSource)
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
        EvidenceFile = state.Evidence.UploadedEvidenceFile;
        Comments = state.Comments;

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        if (_potentialDuplicates!.Any(p => p.IsInvalid))
        {
            return BadRequest();
        }

        var state = journey.State;
        var primaryPersonId = state.PrimaryPersonId!.Value;
        var secondaryPersonId = primaryPersonId == state.PersonAId ? state.PersonBId!.Value : state.PersonAId!.Value;

        var processContext = new ProcessContext(
            ProcessType.PersonMerging,
            timeProvider.UtcNow,
            User.GetUserId(),
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = null,
                Details = Comments,
                EvidenceFile = EvidenceFile?.ToEventModel(),
                AdditionalInformation = null
            });

        await personService.MergePersonsAsync(
            new MergePersonsOptions
            {
                DeactivatingPersonId = secondaryPersonId,
                RetainedPersonId = primaryPersonId,
                FirstName = state.FirstNameSource is not PersonAttributeSource.PrimaryPerson ? Option.Some(FirstName!) : default,
                MiddleName = state.MiddleNameSource is not PersonAttributeSource.PrimaryPerson ? Option.Some(MiddleName ?? string.Empty) : default,
                LastName = state.LastNameSource is not PersonAttributeSource.PrimaryPerson ? Option.Some(LastName!) : default,
                DateOfBirth = state.DateOfBirthSource is not PersonAttributeSource.PrimaryPerson ? Option.Some<DateOnly?>(DateOfBirth!.Value) : default,
                EmailAddress = state.EmailAddressSource is not PersonAttributeSource.PrimaryPerson ? Option.Some<EmailAddress?>(EmailAddress is { } email ? Core.EmailAddress.Parse(email) : null) : default,
                NationalInsuranceNumber = state.NationalInsuranceNumberSource is not PersonAttributeSource.PrimaryPerson ? Option.Some<NationalInsuranceNumber?>(NationalInsuranceNumber is { } nino ? Core.NationalInsuranceNumber.Parse(nino!) : null) : default,
                Gender = state.GenderSource is not PersonAttributeSource.PrimaryPerson ? Option.Some(Gender) : default
            },
            processContext);

        TempData.SetFlashNotificationBanner(
            $"Records merged for {string.JoinNonEmpty(' ', FirstName, MiddleName, LastName)}",
            buildMessageHtml: LinkTagBuilder.BuildViewRecordLink(linkGenerator.Persons.PersonDetail.Index(primaryPersonId)));

        journey.DeleteInstance();

        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }
}

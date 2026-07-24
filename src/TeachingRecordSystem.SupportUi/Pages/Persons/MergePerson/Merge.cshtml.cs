using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

[Journey(JourneyNames.MergePerson)]
public class MergeModel(
    MergePersonJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<MergeModel> _validator = new()
    {
        v => v.RuleFor(m => m.Evidence).Evidence()
    };
    public string? BackLink { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public PersonAttribute<string>? Trn { get; set; }
    public PersonAttribute<string>? FirstName { get; set; }
    public PersonAttribute<string>? MiddleName { get; set; }
    public PersonAttribute<string>? LastName { get; set; }
    public PersonAttribute<DateOnly?>? DateOfBirth { get; set; }
    public PersonAttribute<string?>? EmailAddress { get; set; }
    public PersonAttribute<string?>? NationalInsuranceNumber { get; set; }
    public PersonAttribute<Gender?>? Gender { get; set; }

    [BindProperty]
    public PersonAttributeSource? FirstNameSource { get; set; }

    [BindProperty]
    public PersonAttributeSource? MiddleNameSource { get; set; }

    [BindProperty]
    public PersonAttributeSource? LastNameSource { get; set; }

    [BindProperty]
    public PersonAttributeSource? DateOfBirthSource { get; set; }

    [BindProperty]
    public PersonAttributeSource? EmailAddressSource { get; set; }

    [BindProperty]
    public PersonAttributeSource? NationalInsuranceNumberSource { get; set; }

    [BindProperty]
    public PersonAttributeSource? GenderSource { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    [BindProperty]
    public string? Comments { get; set; }

    private IReadOnlyList<PotentialDuplicate>? _potentialDuplicates;

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        BackLink = journey.GetBackLink();

        var personAId = journey.State.PersonAId!.Value;
        var personBId = journey.State.PersonBId!.Value;
        var primaryPersonId = journey.State.PrimaryPersonId!.Value;

        _potentialDuplicates = await journey.GetPotentialDuplicatesAsync(personAId, personBId);

        var secondaryPersonId = primaryPersonId == personAId ? personBId : personAId;

        var primaryPerson = _potentialDuplicates.Single(p => p.PersonId == primaryPersonId);
        var secondaryPerson = _potentialDuplicates.Single(p => p.PersonId == secondaryPersonId);

        var attributeMatches = journey.GetPersonAttributeMatches(
            primaryPerson.Attributes,
            secondaryPerson.Attributes.FirstName,
            secondaryPerson.Attributes.MiddleName,
            secondaryPerson.Attributes.LastName,
            secondaryPerson.Attributes.DateOfBirth,
            secondaryPerson.Attributes.EmailAddress,
            secondaryPerson.Attributes.NationalInsuranceNumber,
            secondaryPerson.Attributes.Gender);

        Trn = new PersonAttribute<string>(
            primaryPerson.Trn,
            secondaryPerson.FirstName,
            Different: true);

        FirstName = new PersonAttribute<string>(
            primaryPerson.Attributes.FirstName,
            secondaryPerson.Attributes.FirstName,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.FirstName));

        MiddleName = new PersonAttribute<string>(
            primaryPerson.Attributes.MiddleName,
            secondaryPerson.Attributes.MiddleName,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.MiddleName));

        LastName = new PersonAttribute<string>(
            primaryPerson.Attributes.LastName,
            secondaryPerson.Attributes.LastName,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.LastName));

        DateOfBirth = new PersonAttribute<DateOnly?>(
            primaryPerson.Attributes.DateOfBirth,
            secondaryPerson.Attributes.DateOfBirth,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.DateOfBirth));

        EmailAddress = new PersonAttribute<string?>(
            primaryPerson.Attributes.EmailAddress,
            secondaryPerson.Attributes.EmailAddress,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.EmailAddress));

        NationalInsuranceNumber = new PersonAttribute<string?>(
            primaryPerson.Attributes.NationalInsuranceNumber,
            secondaryPerson.Attributes.NationalInsuranceNumber,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.NationalInsuranceNumber));

        Gender = new PersonAttribute<Gender?>(
            primaryPerson.Attributes.Gender,
            secondaryPerson.Attributes.Gender,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.Gender));

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public void OnGet()
    {
        FirstNameSource = journey.State.FirstNameSource;
        MiddleNameSource = journey.State.MiddleNameSource;
        LastNameSource = journey.State.LastNameSource;
        DateOfBirthSource = journey.State.DateOfBirthSource;
        EmailAddressSource = journey.State.EmailAddressSource;
        NationalInsuranceNumberSource = journey.State.NationalInsuranceNumberSource;
        GenderSource = journey.State.GenderSource;
        Comments = journey.State.Comments;
        Evidence = journey.State.Evidence;
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

        if (FirstName!.Different && FirstNameSource is null)
        {
            ModelState.AddModelError(nameof(FirstNameSource), "Select a first name");
        }

        if (MiddleName!.Different && MiddleNameSource is null)
        {
            ModelState.AddModelError(nameof(MiddleNameSource), "Select a middle name");
        }

        if (LastName!.Different && LastNameSource is null)
        {
            ModelState.AddModelError(nameof(LastNameSource), "Select a last name");
        }

        if (DateOfBirth!.Different && DateOfBirthSource is null)
        {
            ModelState.AddModelError(nameof(DateOfBirthSource), "Select a date of birth");
        }

        if (EmailAddress!.Different && EmailAddressSource is null)
        {
            ModelState.AddModelError(nameof(EmailAddressSource), "Select an email");
        }

        if (NationalInsuranceNumber!.Different && NationalInsuranceNumberSource is null)
        {
            ModelState.AddModelError(nameof(NationalInsuranceNumberSource), "Select a National Insurance number");
        }

        if (Gender!.Different && GenderSource is null)
        {
            ModelState.AddModelError(nameof(GenderSource), "Select a gender");
        }

        // Upload the evidence file before validating so that it's retained if the form is re-rendered
        // with errors.
        await evidenceUploadManager.UploadAsync(Evidence);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.Persons.MergePerson.CheckAnswers(journey.InstanceId),
            state =>
            {
                state.FirstNameSource = FirstNameSource;
                state.MiddleNameSource = MiddleNameSource;
                state.LastNameSource = LastNameSource;
                state.DateOfBirthSource = DateOfBirthSource;
                state.EmailAddressSource = EmailAddressSource;
                state.NationalInsuranceNumberSource = NationalInsuranceNumberSource;
                state.GenderSource = GenderSource;
                state.PersonAttributeSourcesSet = true;
                state.Evidence = Evidence;
                state.Comments = Comments;
            });
    }
}

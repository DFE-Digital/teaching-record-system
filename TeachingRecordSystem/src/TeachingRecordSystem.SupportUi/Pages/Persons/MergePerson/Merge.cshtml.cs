using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

[Journey(JourneyNames.MergePerson), RequireJourneyInstance]
public class MergeModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    public string BackLink => GetPageLink(FromCheckAnswers ? MergePersonJourneyPage.CheckAnswers : MergePersonJourneyPage.Matches);

    public PersonAttribute<string>? Trn { get; set; }
    public PersonAttribute<string>? FirstName { get; set; }
    public PersonAttribute<string>? MiddleName { get; set; }
    public PersonAttribute<string>? LastName { get; set; }
    public PersonAttribute<DateOnly?>? DateOfBirth { get; set; }
    public PersonAttribute<string?>? EmailAddress { get; set; }
    public PersonAttribute<string?>? NationalInsuranceNumber { get; set; }
    public PersonAttribute<Gender?>? Gender { get; set; }

    [BindProperty]
    [Display(Name = "First name")]
    public PersonAttributeSource? FirstNameSource { get; set; }

    [BindProperty]
    [Display(Name = "Middle name")]
    public PersonAttributeSource? MiddleNameSource { get; set; }

    [BindProperty]
    [Display(Name = "Last name")]
    public PersonAttributeSource? LastNameSource { get; set; }

    [BindProperty]
    [Display(Name = "Date of birth")]
    public PersonAttributeSource? DateOfBirthSource { get; set; }

    [BindProperty]
    [Display(Name = "Email")]
    public PersonAttributeSource? EmailAddressSource { get; set; }

    [BindProperty]
    [Display(Name = "National Insurance number")]
    public PersonAttributeSource? NationalInsuranceNumberSource { get; set; }

    [BindProperty]
    [Display(Name = "Gender")]
    public PersonAttributeSource? GenderSource { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    [BindProperty]
    [Display(Name = "Add comments (optional)")]
    public string? Comments { get; set; }

    private IReadOnlyList<PotentialDuplicate>? _potentialDuplicates;

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (JourneyInstance!.State.PersonAId is not Guid personAId || JourneyInstance!.State.PersonBId is not Guid personBId)
        {
            context.Result = Redirect(GetPageLink(MergePersonJourneyPage.EnterTrn));
            return;
        }

        if (JourneyInstance!.State.PrimaryPersonId is not Guid primaryPersonId)
        {
            context.Result = Redirect(GetPageLink(MergePersonJourneyPage.Matches));
            return;
        }

        _potentialDuplicates = await GetPotentialDuplicatesAsync(personAId, personBId);

        var secondaryPersonId = primaryPersonId == personAId ? personBId : personAId;

        var primaryPerson = _potentialDuplicates.Single(p => p.PersonId == primaryPersonId);
        var secondaryPerson = _potentialDuplicates.Single(p => p.PersonId == secondaryPersonId);

        var attributeMatches = GetPersonAttributeMatches(
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
    }

    public void OnGet()
    {
        FirstNameSource = JourneyInstance!.State.FirstNameSource;
        MiddleNameSource = JourneyInstance!.State.MiddleNameSource;
        LastNameSource = JourneyInstance!.State.LastNameSource;
        DateOfBirthSource = JourneyInstance!.State.DateOfBirthSource;
        EmailAddressSource = JourneyInstance!.State.EmailAddressSource;
        NationalInsuranceNumberSource = JourneyInstance!.State.NationalInsuranceNumberSource;
        GenderSource = JourneyInstance!.State.GenderSource;
        Comments = JourneyInstance!.State.Comments;
        Evidence = JourneyInstance!.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
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

        await EvidenceController.ValidateAndUploadAsync(Evidence, ModelState);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
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

        return Redirect(GetPageLink(MergePersonJourneyPage.CheckAnswers));
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate)]
public class MergeModel(
    ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator journey,
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : ResolveTeacherPensionsPotentialDuplicatePageModel(journey, dbContext)
{
    private readonly InlineValidator<MergeModel> _validator = new()
    {
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    [BindProperty]
    public bool Cancel { get; set; }

    public string? PersonName { get; set; }

    public string? SourceApplicationUserName { get; set; }

    public PersonAttribute<DateOnly?>? DateOfBirth { get; set; }

    public PersonAttribute<string?>? EmailAddress { get; set; }

    public PersonAttribute<string>? Trn { get; set; }

    public PersonAttribute<string?>? FirstName { get; set; }

    public PersonAttribute<string?>? LastName { get; set; }

    public PersonAttribute<string?>? NationalInsuranceNumber { get; set; }

    public PersonAttribute<Gender?>? Gender { get; set; }

    [BindProperty]
    public PersonAttributeSource? DateOfBirthSource { get; set; }

    [BindProperty]
    public PersonAttributeSource? NationalInsuranceNumberSource { get; set; }

    [BindProperty]
    public PersonAttributeSource? GenderSource { get; set; }

    [BindProperty]
    public PersonAttributeSource? FirstNameSource { get; set; }

    [BindProperty]
    public PersonAttributeSource? LastNameSource { get; set; }

    public PersonAttributeSource? TRNSource { get; set; }

    [BindProperty]
    public string? MergeComments { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = GetSupportTask();
        var requestData = supportTask.TrnRequestMetadata!;
        var state = Journey.State;
        var person = DbContext!.Persons.Single(x => x.PersonId == supportTask.PersonId);
        var personId = state.PersonId!.Value;

        BackLink = Journey.GetBackLink();

        var personAttributes = await GetPersonAttributesAsync(personId);
        var attributeMatches = state.MatchedPersons
            .Single(m => m.PersonId == personId)
            .MatchedAttributes;

        DateOfBirth = new PersonAttribute<DateOnly?>(
            personAttributes.DateOfBirth,
            requestData.DateOfBirth,
            Different: personAttributes.DateOfBirth != requestData.DateOfBirth,
            Highlight: !attributeMatches.Contains(PersonMatchedAttribute.DateOfBirth));

        NationalInsuranceNumber = new PersonAttribute<string?>(
            personAttributes.NationalInsuranceNumber,
            requestData.NationalInsuranceNumber,
            Different: !(personAttributes.NationalInsuranceNumber == requestData.NationalInsuranceNumber || (string.IsNullOrEmpty(personAttributes.NationalInsuranceNumber) && string.IsNullOrEmpty(requestData.NationalInsuranceNumber))),
            Highlight: !(attributeMatches.Contains(PersonMatchedAttribute.NationalInsuranceNumber) || (string.IsNullOrEmpty(personAttributes.NationalInsuranceNumber) && string.IsNullOrEmpty(requestData.NationalInsuranceNumber))));

        Gender = new PersonAttribute<Gender?>(
            personAttributes.Gender,
            requestData.Gender,
            Different: !(personAttributes.Gender == requestData.Gender || (personAttributes.Gender is null && requestData.Gender is null)),
            Highlight: !(attributeMatches.Contains(PersonMatchedAttribute.Gender) || (personAttributes.Gender is null && requestData.Gender is null)));

        LastName = new PersonAttribute<string?>(
            personAttributes.LastName,
            requestData.LastName,
            Different: personAttributes.LastName != requestData.LastName,
            Highlight: !attributeMatches.Contains(PersonMatchedAttribute.LastName));

        FirstName = new PersonAttribute<string?>(
            personAttributes.FirstName,
            requestData.FirstName,
            Different: personAttributes.FirstName != requestData.FirstName,
            Highlight: !attributeMatches.Contains(PersonMatchedAttribute.FirstName));

        Trn = new PersonAttribute<string>(
            personAttributes.Trn,
            person.Trn,
            Different: false,
            Highlight: false);

        PersonName = string.JoinNonEmpty(' ', personAttributes.FirstName, personAttributes.MiddleName, personAttributes.LastName);

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public void OnGet()
    {
        DateOfBirthSource = Journey.State.DateOfBirthSource;
        NationalInsuranceNumberSource = Journey.State.NationalInsuranceNumberSource;
        GenderSource = Journey.State.GenderSource;
        MergeComments = Journey.State.MergeComments;
        FirstNameSource = Journey.State.FirstNameSource;
        LastNameSource = Journey.State.LastNameSource;
        TRNSource = Journey.State.TRNSource;
        Evidence = Journey.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        await evidenceUploadManager.ValidateAndUploadAsync<MergeModel>(m => m.Evidence, ViewData);

        if (DateOfBirth!.Different && DateOfBirthSource is null)
        {
            ModelState.AddModelError(nameof(DateOfBirthSource), "Select a date of birth");
        }

        if (NationalInsuranceNumber!.Different && NationalInsuranceNumberSource is null)
        {
            ModelState.AddModelError(nameof(NationalInsuranceNumberSource), "Select a National Insurance number");
        }

        if (FirstName!.Different && FirstNameSource is null)
        {
            ModelState.AddModelError(nameof(FirstNameSource), "Select a First name");
        }

        if (LastName!.Different && LastNameSource is null)
        {
            ModelState.AddModelError(nameof(LastNameSource), "Select a Last name");
        }

        if (Gender!.Different && GenderSource is null)
        {
            ModelState.AddModelError(nameof(GenderSource), "Select a gender");
        }

        _validator.ValidateAndThrow(this);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        return Journey.AdvanceTo(
            linkGenerator.SupportTasks.TeacherPensions.Resolve.CheckAnswers(Journey.InstanceId),
            state =>
            {
                state.DateOfBirthSource = DateOfBirthSource;
                state.NationalInsuranceNumberSource = NationalInsuranceNumberSource;
                state.GenderSource = GenderSource;
                state.PersonAttributeSourcesSet = true;
                state.MergeComments = MergeComments;
                state.FirstNameSource = FirstNameSource;
                state.LastNameSource = LastNameSource;
                state.Evidence = Evidence;
            });
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(Journey.State.Evidence.UploadedEvidenceFile);
        Journey.DeleteInstance();

        return Redirect(linkGenerator.SupportTasks.TeacherPensions.Index());
    }

#pragma warning disable CA1711
    public record PersonAttribute<T>(T ExistingRecordValue, T TrnRequestValue, bool Different, bool Highlight);
#pragma warning restore CA1711
}

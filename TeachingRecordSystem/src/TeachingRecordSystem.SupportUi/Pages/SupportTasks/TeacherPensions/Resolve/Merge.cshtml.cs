using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve.ResolveTeacherPensionsPotentialDuplicateState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance]
public class Merge(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceController) : ResolveTeacherPensionsPotentialDuplicatePageModel(dbContext)
{
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
    [Display(Name = "First name")]
    public PersonAttributeSource? FirstNameSource { get; set; }

    [BindProperty]
    [Display(Name = "Last name")]
    public PersonAttributeSource? LastNameSource { get; set; }

    [Display(Name = "TRN")]
    public PersonAttributeSource? TRNSource { get; set; }

    [BindProperty]
    [Display(Name = "Add comments (optional)")]
    public string? MergeComments { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = GetSupportTask();
        var requestData = supportTask.TrnRequestMetadata!;
        var state = JourneyInstance!.State;
        var person = DbContext!.Persons.Single(x => x.PersonId == supportTask.PersonId);

        if (state.PersonId is not Guid personId)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.TeacherPensions.Resolve.Matches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (state.PersonId == CreateNewRecordPersonIdSentinel)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.TeacherPensions.Resolve.CheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }
        var personAttributes = await GetPersonAttributesAsync(personId);

        var attributeMatches = GetPersonAttributeMatches(
            personAttributes.FirstName,
            personAttributes.MiddleName,
            personAttributes.LastName,
            personAttributes.DateOfBirth,
            personAttributes.NationalInsuranceNumber,
            personAttributes.Gender);

        DateOfBirth = new PersonAttribute<DateOnly?>(
            personAttributes.DateOfBirth,
            requestData.DateOfBirth,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.DateOfBirth));

        NationalInsuranceNumber = new PersonAttribute<string?>(
            personAttributes.NationalInsuranceNumber,
            requestData.NationalInsuranceNumber,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.NationalInsuranceNumber));

        Gender = new PersonAttribute<Gender?>(
            personAttributes.Gender,
            requestData.Gender,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.Gender));

        LastName = new PersonAttribute<string?>(
            personAttributes.LastName,
            requestData.LastName,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.LastName));

        FirstName = new PersonAttribute<string?>(
            personAttributes.FirstName,
            requestData.FirstName,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.FirstName));

        Trn = new PersonAttribute<string>(
            personAttributes.Trn,
            person.Trn,
            Different: false);

        PersonName = StringHelper.JoinNonEmpty(' ', personAttributes.FirstName, personAttributes.MiddleName, personAttributes.LastName);

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public void OnGet()
    {
        DateOfBirthSource = JourneyInstance!.State.DateOfBirthSource;
        NationalInsuranceNumberSource = JourneyInstance!.State.NationalInsuranceNumberSource;
        GenderSource = JourneyInstance!.State.GenderSource;
        MergeComments = JourneyInstance!.State.MergeComments;
        FirstNameSource = JourneyInstance!.State.FirstNameSource;
        LastNameSource = JourneyInstance!.State.LastNameSource;
        TRNSource = JourneyInstance!.State.TRNSource;
        Evidence = JourneyInstance!.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await evidenceController.ValidateAndUploadAsync(Evidence, ModelState);

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

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
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

        return Redirect(linkGenerator.SupportTasks.TeacherPensions.Resolve.CheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.SupportTasks.TeacherPensions.Index());
    }

#pragma warning disable CA1711
    public record PersonAttribute<T>(T ExistingRecordValue, T TrnRequestValue, bool Different);
#pragma warning restore CA1711
}

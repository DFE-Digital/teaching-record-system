using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve.ResolveTeacherPensionsPotentialDuplicateState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance]
public class Merge(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : ResolveTeacherPensionsPotentialDuplicatePageModel(dbContext)
{
    public string? PersonName { get; set; }

    public string? SourceApplicationUserName { get; set; }

    public PersonAttribute<DateOnly?>? DateOfBirth { get; set; }

    public PersonAttribute<string?>? EmailAddress { get; set; }

    public PersonAttribute<string?>? Trn { get; set; }

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
    public string? Comments { get; set; }

    public void OnGet()
    {
        DateOfBirthSource = JourneyInstance!.State.DateOfBirthSource;
        NationalInsuranceNumberSource = JourneyInstance!.State.NationalInsuranceNumberSource;
        GenderSource = JourneyInstance!.State.GenderSource;
        Comments = JourneyInstance!.State.Comments;
        FirstNameSource = JourneyInstance!.State.FirstNameSource;
        LastNameSource = JourneyInstance!.State.LastNameSource;
        TRNSource = JourneyInstance!.State.TRNSource;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (DateOfBirth!.Different && DateOfBirthSource is null)
        {
            ModelState.AddModelError(nameof(DateOfBirthSource), "Select a date of birth");
        }

        if (NationalInsuranceNumber!.Different && NationalInsuranceNumberSource is null)
        {
            ModelState.AddModelError(nameof(NationalInsuranceNumberSource), "Select a National Insurance number");
        }

        if (Trn!.Different && TRNSource is null)
        {
            ModelState.AddModelError(nameof(TRNSource), "Select a TRN");
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
            state.Comments = Comments;
            state.TRNSource = TRNSource;
            state.FirstNameSource = FirstNameSource;
            state.LastNameSource = LastNameSource;
        });

        return Redirect(linkGenerator.TeacherPensionsCheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.TeacherPensions());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var requestData = GetRequestData();
        var state = JourneyInstance!.State;

        if (state.PersonId is not Guid personId)
        {
            context.Result = Redirect(linkGenerator.TeacherPensionsMatches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (state.PersonId == CreateNewRecordPersonIdSentinel)
        {
            context.Result = Redirect(linkGenerator.TeacherPensionsCheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId));
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

        PersonName = StringHelper.JoinNonEmpty(' ', new string?[] { personAttributes.FirstName, personAttributes.MiddleName, personAttributes.LastName });

        SourceApplicationUserName = requestData.ApplicationUser!.Name;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

public record PersonAttribute<T>(T ExistingRecordValue, T TrnRequestValue, bool Different);

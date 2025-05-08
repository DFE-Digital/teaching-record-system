using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve.ResolveApiTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

[Journey(JourneyNames.ResolveApiTrnRequest), RequireJourneyInstance]
public class Merge(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : ResolveApiTrnRequestPageModel(dbContext)
{
    [FromRoute]
    public string? SupportTaskReference { get; set; }

    public string? SourceApplicationUserName { get; set; }

    public PersonAttribute<string?>? FirstName { get; set; }

    public PersonAttribute<string?>? MiddleName { get; set; }

    public PersonAttribute<string?>? LastName { get; set; }

    public PersonAttribute<DateOnly?>? DateOfBirth { get; set; }

    public PersonAttribute<string?>? EmailAddress { get; set; }

    public PersonAttribute<string?>? NationalInsuranceNumber { get; set; }

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
    [Display(Name = "Add comments")]
    public string? Comments { get; set; }

    public void OnGet()
    {
        FirstNameSource = JourneyInstance!.State.FirstNameSource;
        MiddleNameSource = JourneyInstance!.State.MiddleNameSource;
        LastNameSource = JourneyInstance!.State.LastNameSource;
        DateOfBirthSource = JourneyInstance!.State.DateOfBirthSource;
        EmailAddressSource = JourneyInstance!.State.EmailAddressSource;
        NationalInsuranceNumberSource = JourneyInstance!.State.NationalInsuranceNumberSource;
        Comments = JourneyInstance!.State.Comments;
    }

    public async Task<IActionResult> OnPostAsync()
    {
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
            state.PersonAttributeSourcesSet = true;
            state.Comments = Comments;
        });

        return Redirect(linkGenerator.ApiTrnRequestCheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.ApiTrnRequests());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var requestData = GetRequestData();
        var state = JourneyInstance!.State;

        if (state.PersonId is not Guid personId)
        {
            context.Result = Redirect(linkGenerator.ApiTrnRequestMatches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (state.PersonId == CreateNewRecordPersonIdSentinel)
        {
            context.Result = Redirect(linkGenerator.ApiTrnRequestCheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        var personAttributes = await GetPersonAttributesAsync(personId);

        SourceApplicationUserName = requestData.ApplicationUser.Name;

        var attributeMatches = GetPersonAttributeMatches(
            personAttributes.FirstName,
            personAttributes.MiddleName,
            personAttributes.LastName,
            personAttributes.DateOfBirth,
            personAttributes.EmailAddress,
            personAttributes.NationalInsuranceNumber);

        FirstName = new PersonAttribute<string?>(
            personAttributes.FirstName,
            requestData.FirstName,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.FirstName));

        MiddleName = new PersonAttribute<string?>(
            personAttributes.MiddleName,
            requestData.MiddleName,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.MiddleName));

        LastName = new PersonAttribute<string?>(
            personAttributes.LastName,
            requestData.LastName,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.LastName));

        DateOfBirth = new PersonAttribute<DateOnly?>(
            personAttributes.DateOfBirth,
            requestData.DateOfBirth,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.DateOfBirth));

        EmailAddress = new PersonAttribute<string?>(
            personAttributes.EmailAddress,
            requestData.EmailAddress,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.EmailAddress));

        NationalInsuranceNumber = new PersonAttribute<string?>(
            personAttributes.NationalInsuranceNumber,
            requestData.NationalInsuranceNumber,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.NationalInsuranceNumber));

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public record PersonAttribute<T>(T ExistingRecordValue, T TrnRequestValue, bool Different);
}

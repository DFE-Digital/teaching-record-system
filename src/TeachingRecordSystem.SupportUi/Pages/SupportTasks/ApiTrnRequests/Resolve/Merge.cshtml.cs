using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Services;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve.ResolveApiTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

[Journey(JourneyNames.ResolveApiTrnRequest), RequireJourneyInstance]
public class Merge(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : ResolveApiTrnRequestPageModel(dbContext)
{
    [FromRoute]
    public string? SupportTaskReference { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? SourceApplicationUserName { get; set; }

    public PersonAttribute<string?>? FirstName { get; set; }

    public PersonAttribute<string?>? MiddleName { get; set; }

    public PersonAttribute<string?>? LastName { get; set; }

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
    public string? Comments { get; set; }

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
            state.FirstNameSource = FirstNameSource;
            state.MiddleNameSource = MiddleNameSource;
            state.LastNameSource = LastNameSource;
            state.DateOfBirthSource = DateOfBirthSource;
            state.EmailAddressSource = EmailAddressSource;
            state.NationalInsuranceNumberSource = NationalInsuranceNumberSource;
            state.GenderSource = GenderSource;
            state.PersonAttributeSourcesSet = true;
            state.Comments = Comments;
        });

        return Redirect(linkGenerator.SupportTasks.ApiTrnRequests.Resolve.CheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks.ApiTrnRequests.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var requestData = GetRequestData();
        var state = JourneyInstance!.State;

        if (state.PersonId is not Guid personId)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.ApiTrnRequests.Resolve.Matches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (state.PersonId == CreateNewRecordPersonIdSentinel)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.ApiTrnRequests.Resolve.CheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        var personAttributes = await GetPersonAttributesAsync(personId);
        var attributeMatches = JourneyInstance!.State.MatchedPersons
            .Single(m => m.PersonId == personId)
            .MatchedAttributes;

        SourceApplicationUserName = requestData.ApplicationUser!.Name;

        FirstName = new PersonAttribute<string?>(
            personAttributes.FirstName,
            requestData.FirstName,
            Different: personAttributes.FirstName != requestData.FirstName,
            Highlight: !attributeMatches.Contains(PersonMatchedAttribute.FirstName));

        MiddleName = new PersonAttribute<string?>(
            personAttributes.MiddleName,
            requestData.MiddleName,
            Different: !(personAttributes.MiddleName == requestData.MiddleName || (string.IsNullOrEmpty(personAttributes.MiddleName) && string.IsNullOrEmpty(requestData.MiddleName))),
            Highlight: !(attributeMatches.Contains(PersonMatchedAttribute.MiddleName) || (string.IsNullOrEmpty(personAttributes.MiddleName) && string.IsNullOrEmpty(requestData.MiddleName))));

        LastName = new PersonAttribute<string?>(
            personAttributes.LastName,
            requestData.LastName,
            Different: personAttributes.LastName != requestData.LastName,
            Highlight: !attributeMatches.Contains(PersonMatchedAttribute.LastName));

        DateOfBirth = new PersonAttribute<DateOnly?>(
            personAttributes.DateOfBirth,
            requestData.DateOfBirth,
            Different: personAttributes.DateOfBirth != requestData.DateOfBirth,
            Highlight: !attributeMatches.Contains(PersonMatchedAttribute.DateOfBirth));

        EmailAddress = new PersonAttribute<string?>(
            personAttributes.EmailAddress,
            requestData.EmailAddress,
            Different: !(personAttributes.EmailAddress == requestData.EmailAddress || (string.IsNullOrEmpty(personAttributes.EmailAddress) && string.IsNullOrEmpty(requestData.EmailAddress))),
            Highlight: !(attributeMatches.Contains(PersonMatchedAttribute.EmailAddress) || (string.IsNullOrEmpty(personAttributes.EmailAddress) && string.IsNullOrEmpty(requestData.EmailAddress))));

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

        await base.OnPageHandlerExecutionAsync(context, next);
    }

#pragma warning disable CA1711
    public record PersonAttribute<T>(T ExistingRecordValue, T TrnRequestValue, bool Different, bool Highlight);
#pragma warning restore CA1711
}

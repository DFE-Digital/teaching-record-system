using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Services;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve.ResolveNpqTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

[Journey(JourneyNames.ResolveNpqTrnRequest), RequireJourneyInstance]
public class MergeModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : ResolveNpqTrnRequestPageModel(dbContext)
{
    public string? PersonName { get; set; }

    public string? SourceApplicationUserName { get; set; }

    public PersonAttribute<DateOnly?>? DateOfBirth { get; set; }

    public PersonAttribute<string?>? EmailAddress { get; set; }

    public PersonAttribute<string?>? NationalInsuranceNumber { get; set; }

    public PersonAttribute<Gender?>? Gender { get; set; }

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

        DateOfBirthSource = JourneyInstance!.State.DateOfBirthSource;
        EmailAddressSource = JourneyInstance!.State.EmailAddressSource;
        NationalInsuranceNumberSource = JourneyInstance!.State.NationalInsuranceNumberSource;
        GenderSource = JourneyInstance!.State.GenderSource;
        Comments = JourneyInstance!.State.Comments;
    }

    public async Task<IActionResult> OnPostAsync()
    {
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
            state.DateOfBirthSource = DateOfBirthSource;
            state.EmailAddressSource = EmailAddressSource;
            state.NationalInsuranceNumberSource = NationalInsuranceNumberSource;
            state.GenderSource = GenderSource;
            state.PersonAttributeSourcesSet = true;
            state.Comments = Comments;
        });

        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Resolve.CheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var requestData = GetRequestData();
        var state = JourneyInstance!.State;

        if (state.PersonId is not Guid personId)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Resolve.Matches(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }

        if (state.PersonId == CreateNewRecordPersonIdSentinel)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Resolve.CheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId));
            return;
        }
        var personAttributes = await GetPersonAttributesAsync(personId);

        var attributeMatches = GetPersonAttributeMatches(
            personAttributes.FirstName,
            personAttributes.MiddleName,
            personAttributes.LastName,
            personAttributes.DateOfBirth,
            personAttributes.EmailAddress,
            personAttributes.NationalInsuranceNumber,
            personAttributes.Gender);

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

        Gender = new PersonAttribute<Gender?>(
            personAttributes.Gender,
            requestData.Gender,
            Different: !attributeMatches.Contains(PersonMatchedAttribute.Gender));

        PersonName = StringHelper.JoinNonEmpty(' ', personAttributes.FirstName, personAttributes.MiddleName, personAttributes.LastName);

        SourceApplicationUserName = requestData.ApplicationUser!.Name;

        await base.OnPageHandlerExecutionAsync(context, next);
    }

#pragma warning disable CA1711
    public record PersonAttribute<T>(T ExistingRecordValue, T TrnRequestValue, bool Different);
#pragma warning restore CA1711
}

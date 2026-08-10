using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

[Journey(JourneyNames.ResolveTrnRequest)]
public class Merge(
    ResolveTrnRequestJourneyCoordinator journey,
    TrsDbContext dbContext,
    SupportTaskService supportTaskService,
    TimeProvider timeProvider,
    IFeatureProvider featureProvider,
    SupportUiLinkGenerator linkGenerator) : ResolveTrnRequestPageModel(journey, dbContext)
{
    public static class Actions
    {
        public const string SaveAndComeBackLater = nameof(SaveAndComeBackLater);
        public const string Cancel = nameof(Cancel);
    }

    private SupportTask? _supportTask;


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
        Journey.State.ApplySavedModelStateValues(nameof(Merge), ModelState);
        FirstNameSource = Journey.State.FirstNameSource;
        MiddleNameSource = Journey.State.MiddleNameSource;
        LastNameSource = Journey.State.LastNameSource;
        DateOfBirthSource = Journey.State.DateOfBirthSource;
        EmailAddressSource = Journey.State.EmailAddressSource;
        NationalInsuranceNumberSource = Journey.State.NationalInsuranceNumberSource;
        GenderSource = Journey.State.GenderSource;
        Comments = Journey.State.Comments;
    }

    public async Task<IActionResult> OnPostAsync(string? action)
    {
        if (action is Actions.Cancel)
        {
            Journey.DeleteInstance();

            return Redirect(Journey.State.CompletionUrl);
        }

        if (action is Actions.SaveAndComeBackLater)
        {
            return await HandleSaveAndReturnAsync();
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

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        return Journey.AdvanceTo(
            linkGenerator.SupportTasks.TrnRequests.Resolve.CheckAnswers(Journey.InstanceId),
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
                state.Comments = Comments;
                state.ClearSavedModelStateValues(nameof(Merge));
            });
    }

    private async Task<IActionResult> HandleSaveAndReturnAsync()
    {
        var savedJourneyState = this.CreateSavedJourneyState(
            nameof(Merge),
            Journey.State,
            excludeKeys: ["Action"]);

        var processContext = new ProcessContext(ProcessType.TrnRequestSupportTaskSaving, timeProvider.UtcNow, User.GetUserId());

        await supportTaskService.SaveProgressAsync(
            new()
            {
                SupportTaskReference = _supportTask!.SupportTaskReference,
                SavedJourneyState = savedJourneyState
            },
            processContext);

        Journey.DeleteInstance();

        if (featureProvider.IsEnabled("SupportTaskDashboard"))
        {
            return Redirect(linkGenerator.SupportTasks.SupportTaskDetail.Index(_supportTask.SupportTaskReference));
        }

        return Redirect(Journey.State.CompletionUrl);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        var requestData = GetRequestData();
        var state = Journey.State;
        var personId = state.PersonId!.Value;

        BackLink = Journey.GetBackLink();

        var personAttributes = await GetPersonAttributesAsync(personId);
        var attributeMatches = state.MatchedPersons
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

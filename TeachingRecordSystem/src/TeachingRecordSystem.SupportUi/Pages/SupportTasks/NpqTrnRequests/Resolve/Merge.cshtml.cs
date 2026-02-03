using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Services;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve.ResolveNpqTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

[Journey(JourneyNames.ResolveNpqTrnRequest), RequireJourneyInstance]
public class MergeModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : ResolveNpqTrnRequestPageModel(dbContext)
{
    private readonly InlineValidator<MergeModel> _validator = new()
    {
        v => v.RuleFor(m => m.DateOfBirthSource)
            .NotNull().When(m => m.DateOfBirth!.Different).WithMessage("Select a date of birth"),
        v => v.RuleFor(m => m.EmailAddressSource)
            .NotNull().When(m => m.EmailAddress!.Different).WithMessage("Select an email"),
        v => v.RuleFor(m => m.NationalInsuranceNumberSource)
            .NotNull().When(m => m.NationalInsuranceNumber!.Different).WithMessage("Select a National Insurance number"),
        v => v.RuleFor(m => m.GenderSource)
            .NotNull().When(m => m.Gender!.Different).WithMessage("Select a gender")
    };

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
        _validator.ValidateAndThrow(this);

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
        var attributeMatches = JourneyInstance!.State.MatchedPersons
            .Single(m => m.PersonId == personId)
            .MatchedAttributes;

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

        PersonName = ' '.JoinNonEmpty(personAttributes.FirstName, personAttributes.MiddleName, personAttributes.LastName);

        SourceApplicationUserName = requestData.ApplicationUser!.Name;

        await base.OnPageHandlerExecutionAsync(context, next);
    }

#pragma warning disable CA1711
    public record PersonAttribute<T>(T ExistingRecordValue, T TrnRequestValue, bool Different, bool Highlight);
#pragma warning restore CA1711
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails), StartsJourney]
public class IndexModel(
    EditDetailsJourneyCoordinator journey,
    PersonService personService,
    TimeProvider timeProvider,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.FirstName)
            .NotEmpty().WithMessage("Enter the person’s first name")
            .MaximumLength(Person.FirstNameMaxLength).WithMessage("Person’s first name must be 100 characters or less"),
        v => v.RuleFor(m => m.MiddleName)
            .MaximumLength(Person.FirstNameMaxLength).WithMessage("Person’s middle name must be 100 characters or less"),
        v => v.RuleFor(m => m.LastName)
            .NotEmpty().WithMessage("Enter the person’s last name")
            .MaximumLength(Person.FirstNameMaxLength).WithMessage("Person’s last name must be 100 characters or less"),
        v => v.RuleFor(m => m.DateOfBirth)
            .NotNull().WithMessage("Enter the person’s date of birth")
            .Must((m, dateOfBirth) => !(dateOfBirth > m.Today)).WithMessage("Person’s date of birth must be in the past"),
        v => v.RuleFor(m => m.NationalInsuranceNumber)
            .Must((m, _) => m.ParsedNationalInsuranceNumber is not null)
                .WithMessage("Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C")
            .When(m => m.NationalInsuranceNumber is not null),
        v => v.RuleFor(m => m.EmailAddress)
            .MaximumLength(Person.EmailAddressMaxLength).WithMessage("Person’s email address must be 100 characters or less")
            .Must((m, _) => m.ParsedEmailAddress is not null).WithMessage("Enter a valid email address")
            .When(m => m.EmailAddress is not null),
        v => v.RuleFor(m => m).Must(m => m.NameChanged || m.OtherDetailsChanged)
            .WithMessage("Please change one or more of the person’s details")
            .OverridePropertyName("")
    };

    private Person? _person;

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public string? FirstName { get; set; }

    [BindProperty]
    public string? MiddleName { get; set; }

    [BindProperty]
    public string? LastName { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Person\u2019s date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    [BindProperty]
    public string? EmailAddress { get; set; }

    [BindProperty]
    public string? NationalInsuranceNumber { get; set; }

    [BindProperty]
    public Gender? Gender { get; set; }

    // Exposed so the validation rules can use them; the parsed values are reused when writing state.
    public DateOnly Today => timeProvider.Today;

    public NationalInsuranceNumber? ParsedNationalInsuranceNumber =>
        Core.NationalInsuranceNumber.TryParse(NationalInsuranceNumber, out var nationalInsuranceNumber) ? nationalInsuranceNumber : null;

    public EmailAddress? ParsedEmailAddress =>
        Core.EmailAddress.TryParse(EmailAddress, out var emailAddress) ? emailAddress : null;

    public bool NameChanged =>
        (FirstName ?? "") != journey.State.OriginalFirstName ||
        (MiddleName ?? "") != journey.State.OriginalMiddleName ||
        (LastName ?? "") != journey.State.OriginalLastName;

    public bool OtherDetailsChanged =>
        DateOfBirth != journey.State.OriginalDateOfBirth ||
        EditDetailsFieldState<EmailAddress>.FromRawValue(EmailAddress) != journey.State.OriginalEmailAddress ||
        EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(NationalInsuranceNumber) != journey.State.OriginalNationalInsuranceNumber ||
        Gender != journey.State.OriginalGender;

    public IActionResult OnGet()
    {
        FirstName = journey.State.FirstName;
        MiddleName = journey.State.MiddleName;
        LastName = journey.State.LastName;
        DateOfBirth = journey.State.DateOfBirth;
        EmailAddress = journey.State.EmailAddress.Parsed?.ToDisplayString() ?? journey.State.EmailAddress.Raw;
        NationalInsuranceNumber = journey.State.NationalInsuranceNumber.Parsed?.ToDisplayString() ?? journey.State.NationalInsuranceNumber.Raw;
        Gender = journey.State.Gender;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        // NotAvailable is not a value the user is allowed to select in the UI. We only allow it
        // if it's a pre-existing value on the Person record and the user is leaving it unchanged.
        if (Gender == Core.Models.Gender.NotAvailable && _person!.Gender != Core.Models.Gender.NotAvailable)
        {
            return BadRequest();
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceToNextQuestion(
            NameChanged
                ? linkGenerator.Persons.PersonDetail.EditDetails.NameChangeReason(journey.InstanceId)
                : linkGenerator.Persons.PersonDetail.EditDetails.OtherDetailsChangeReason(journey.InstanceId),
            state =>
            {
                state.FirstName = FirstName ?? "";
                state.MiddleName = MiddleName ?? "";
                state.LastName = LastName ?? "";
                state.DateOfBirth = DateOfBirth;
                state.EmailAddress = new(EmailAddress ?? "", ParsedEmailAddress);
                state.NationalInsuranceNumber = new(NationalInsuranceNumber ?? "", ParsedNationalInsuranceNumber);
                state.Gender = Gender;

                if (!NameChanged && state.NameChangeReason is not null)
                {
                    state.NameChangeReason = null;
                    state.NameChangeEvidence.Clear();
                }

                if (!OtherDetailsChanged && state.OtherDetailsChangeReason is not null)
                {
                    state.OtherDetailsChangeReason = null;
                    state.OtherDetailsChangeReasonDetail = null;
                    state.OtherDetailsChangeEvidence.Clear();
                }
            });
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        BackLink = journey.GetBackLink() ?? linkGenerator.Persons.PersonDetail.Index(PersonId);

        _person = await personService.GetPersonAsync(PersonId);

        if (_person is null)
        {
            context.Result = NotFound();
            return;
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

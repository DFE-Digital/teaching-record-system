using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[Journey(JourneyNames.AddPerson)]
public class PersonalDetailsModel(
    AddPersonJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    TimeProvider timeProvider) : PageModel
{
    private readonly InlineValidator<PersonalDetailsModel> _validator = new()
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
            .When(m => m.EmailAddress is not null)
    };

    public string? BackLink { get; set; }

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

    public IActionResult OnGet()
    {
        BackLink = GetBackLink();

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

        BackLink = GetBackLink();

        // NotAvailable is not a value the user is allowed to select in the UI.
        if (Gender == Core.Models.Gender.NotAvailable)
        {
            return BadRequest();
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.Persons.AddPerson.Reason(journey.InstanceId),
            state =>
            {
                state.FirstName = FirstName ?? "";
                state.MiddleName = MiddleName ?? "";
                state.LastName = LastName ?? "";
                state.DateOfBirth = DateOfBirth;
                state.EmailAddress = new(EmailAddress ?? "", ParsedEmailAddress);
                state.NationalInsuranceNumber = new(NationalInsuranceNumber ?? "", ParsedNationalInsuranceNumber);
                state.Gender = Gender;
            });
    }

    private string GetBackLink() => journey.GetBackLink() ?? linkGenerator.Index();
}

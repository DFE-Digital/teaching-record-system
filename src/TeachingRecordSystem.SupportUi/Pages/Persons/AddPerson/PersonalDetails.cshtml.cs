using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[Journey(JourneyNames.AddPerson), ActivatesJourney, RequireJourneyInstance]
public class PersonalDetailsModel(
    TimeProvider timeProvider,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager)
    : CommonJourneyPage(linkGenerator, evidenceUploadManager)
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
            .NotNull().WithMessage("Enter the person’s date of birth"),
        v => v.RuleFor(m => m.EmailAddress)
            .MaximumLength(Person.EmailAddressMaxLength).WithMessage("Person’s email address must be 100 characters or less")
    };

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

    public string BackLink => FromCheckAnswers ? GetPageLink(AddPersonJourneyPage.CheckAnswers) : LinkGenerator.Index();

    public string NextPage => GetPageLink(
        FromCheckAnswers
            ? AddPersonJourneyPage.CheckAnswers
            : AddPersonJourneyPage.Reason,
        FromCheckAnswers is true ? true : null);

    public IActionResult OnGet()
    {
        FirstName = JourneyInstance!.State.FirstName;
        MiddleName = JourneyInstance.State.MiddleName;
        LastName = JourneyInstance.State.LastName;
        DateOfBirth = JourneyInstance.State.DateOfBirth;
        EmailAddress = JourneyInstance.State.EmailAddress.Parsed?.ToDisplayString() ?? JourneyInstance.State.EmailAddress.Raw;
        NationalInsuranceNumber = JourneyInstance.State.NationalInsuranceNumber.Parsed?.ToDisplayString() ?? JourneyInstance.State.NationalInsuranceNumber.Raw;
        Gender = JourneyInstance.State.Gender;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // NotAvailable is not a value the user is allowed to select in the UI.
        if (Gender == Core.Models.Gender.NotAvailable)
        {
            return BadRequest();
        }

        if (DateOfBirth.HasValue && DateOfBirth.Value > timeProvider.Today)
        {
            ModelState.AddModelError(nameof(DateOfBirth), "Person\u2019s date of birth must be in the past");
        }

        NationalInsuranceNumber? nationalInsuranceNumber = null;
        if (NationalInsuranceNumber is not null && !Core.NationalInsuranceNumber.TryParse(NationalInsuranceNumber, out nationalInsuranceNumber))
        {
            ModelState.AddModelError(nameof(NationalInsuranceNumber), "Enter a National Insurance number that is 2 letters, 6 numbers, then A, B, C or D, like QQ 12 34 56 C");
        }

        EmailAddress? emailAddress = null;
        if (EmailAddress is not null && !Core.EmailAddress.TryParse(EmailAddress, out emailAddress))
        {
            ModelState.AddModelError(nameof(EmailAddress), "Enter a valid email address");
        }

        _validator.ValidateAndThrow(this);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var nextPage = NextPage;

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.FirstName = FirstName ?? "";
            state.MiddleName = MiddleName ?? "";
            state.LastName = LastName ?? "";
            state.DateOfBirth = DateOfBirth;
            state.EmailAddress = new(EmailAddress ?? "", emailAddress);
            state.NationalInsuranceNumber = new(NationalInsuranceNumber ?? "", nationalInsuranceNumber);
            state.Gender = Gender;
        });

        return Redirect(nextPage);
    }
}

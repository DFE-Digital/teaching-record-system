using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(
    TrsDbContext dbContext,
    IClock clock,
    TrsLinkGenerator linkGenerator,
    EvidenceController evidenceController)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    private Person? _person;

    [BindProperty]
    [Display(Name = "First name")]
    [Required(ErrorMessage = "Enter the person\u2019s first name")]
    [MaxLength(Person.FirstNameMaxLength, ErrorMessage = $"Person\u2019s first name must be 100 characters or less")]
    public string? FirstName { get; set; }

    [BindProperty]
    [Display(Name = "Middle name (optional)")]
    [MaxLength(Person.FirstNameMaxLength, ErrorMessage = $"Person\u2019s middle name must be 100 characters or less")]
    public string? MiddleName { get; set; }

    [BindProperty]
    [Display(Name = "Last name")]
    [Required(ErrorMessage = "Enter the person\u2019s last name")]
    [MaxLength(Person.FirstNameMaxLength, ErrorMessage = $"Person\u2019s last name must be 100 characters or less")]
    public string? LastName { get; set; }

    [BindProperty]
    [Display(Name = "Date of birth")]
    [DateInput(ErrorMessagePrefix = "Person\u2019s date of birth")]
    [Required(ErrorMessage = "Enter the person\u2019s date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    [BindProperty]
    [Display(Name = "Email address (optional)")]
    [MaxLength(Person.EmailAddressMaxLength, ErrorMessage = $"Person\u2019s email address must be 100 characters or less")]
    public string? EmailAddress { get; set; }

    [BindProperty]
    [Display(Name = "National Insurance number (optional)")]
    public string? NationalInsuranceNumber { get; set; }

    [BindProperty]
    [Display(Name = "Gender (optional)")]
    public Gender? Gender { get; set; }

    public bool NameChanged =>
        (FirstName ?? "") != JourneyInstance!.State.OriginalFirstName ||
        (MiddleName ?? "") != JourneyInstance!.State.OriginalMiddleName ||
        (LastName ?? "") != JourneyInstance!.State.OriginalLastName;

    public bool OtherDetailsChanged =>
        DateOfBirth != JourneyInstance!.State.OriginalDateOfBirth ||
        EditDetailsFieldState<EmailAddress>.FromRawValue(EmailAddress) != JourneyInstance!.State.OriginalEmailAddress ||
        EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(NationalInsuranceNumber) != JourneyInstance!.State.OriginalNationalInsuranceNumber ||
        Gender != JourneyInstance!.State.OriginalGender;

    public string BackLink => GetPageLink(
        FromCheckAnswers
            ? EditDetailsJourneyPage.CheckAnswers
            : null);

    public string NextPage => GetPageLink(
        FromCheckAnswers
            ? !JourneyInstance!.State.NameChanged && NameChanged
                ? EditDetailsJourneyPage.NameChangeReason
                : !JourneyInstance!.State.OtherDetailsChanged && OtherDetailsChanged
                    ? EditDetailsJourneyPage.OtherDetailsChangeReason
                    : EditDetailsJourneyPage.CheckAnswers
            : NameChanged
                ? EditDetailsJourneyPage.NameChangeReason
                : EditDetailsJourneyPage.OtherDetailsChangeReason,
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
        // NotAvailable is not a value the user is allowed to select in the UI. We only allow it
        // if it's a pre-existing value on the Person record and the user is leaving it unchanged.
        if (Gender == Core.Models.Gender.NotAvailable && _person!.Gender != Core.Models.Gender.NotAvailable)
        {
            return BadRequest();
        }

        if (!NameChanged && !OtherDetailsChanged)
        {
            ModelState.AddModelError("", "Please change one or more of the person\u2019s details");
        }

        if (DateOfBirth.HasValue && DateOfBirth.Value > clock.Today)
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

        return Redirect(nextPage);
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        _person = await DbContext.Persons.SingleOrDefaultAsync(u => u.PersonId == PersonId);

        if (_person is null)
        {
            context.Result = NotFound();
            return;
        }
    }
}

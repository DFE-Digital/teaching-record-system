using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.Create;

[Journey(JourneyNames.CreatePerson), ActivatesJourney, RequireJourneyInstance]
public class PersonalDetailsModel(
    TrsDbContext dbContext,
    IClock clock,
    TrsLinkGenerator linkGenerator,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
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

    public string BackLink => FromCheckAnswers ? GetPageLink(CreateJourneyPage.CheckAnswers) : LinkGenerator.Index();

    public string NextPage => GetPageLink(
        FromCheckAnswers
            ? CreateJourneyPage.CheckAnswers
            : CreateJourneyPage.CreateReason,
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
        });

        return Redirect(nextPage);
    }
}

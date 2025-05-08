using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[RequireFeatureEnabledFilterFactory(FeatureNames.NewPersonDetails)]
[Journey(JourneyNames.EditDetails), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator)
    : CommonJourneyPage(dbContext, linkGenerator)
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

    public string BackLink =>
        FromCheckAnswers ? GetPageLink(EditDetailsJourneyPage.CheckAnswers) : LinkGenerator.PersonDetail(PersonId);

    public EditDetailsJourneyPage NextPage =>
        FromCheckAnswers ? EditDetailsJourneyPage.CheckAnswers : EditDetailsJourneyPage.ChangeReason;

    public IActionResult OnGet()
    {
        FirstName = JourneyInstance!.State.FirstName;
        MiddleName = JourneyInstance!.State.MiddleName;
        LastName = JourneyInstance!.State.LastName;
        DateOfBirth = JourneyInstance!.State.DateOfBirth;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.FirstName = FirstName;
            state.MiddleName = MiddleName;
            state.LastName = LastName;
            state.DateOfBirth = DateOfBirth;
        });

        return Redirect(GetPageLink(NextPage));
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        _person = await DbContext.Persons.SingleOrDefaultAsync(u => u.PersonId == PersonId);

        if (_person is null)
        {
            context.Result = NotFound();
            return;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

[TeachingRecordSystem.WebCommon.FormFlow.Journey(JourneyNames.ConnectPerson)]
[ActivatesJourney, RequireJourneyInstance]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class IndexModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.Trn)
            .NotEmpty().WithMessage("Enter a TRN"),
        v => v.RuleFor(m => m.Trn)
            .Matches(@"^\d+$").WithMessage("TRN must be a number")
            .When(m => !string.IsNullOrEmpty(m.Trn)),
        v => v.RuleFor(m => m.Trn)
            .Length(Person.TrnExactLength).WithMessage("TRN must be 7 digits long")
            .When(m => !string.IsNullOrEmpty(m.Trn))
    };

    public JourneyInstance<ConnectPersonState>? JourneyInstance { get; set; }

    [FromRoute]
    public string OneLoginUserSubject { get; set; } = null!;

    [BindProperty]
    public string? Trn { get; set; }

    public void OnGet()
    {
        Trn = JourneyInstance?.State.PersonTrn;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        var person = await dbContext.Persons
            .Where(p => p.Trn == Trn)
            .SingleOrDefaultAsync();

        if (person is null)
        {
            ModelState.AddModelError(nameof(Trn), "The TRN you entered does not exist");
            return this.PageWithErrors();
        }

        var oneLoginUserFeature = HttpContext.GetCurrentOneLoginUserFeature();

        if (oneLoginUserFeature.PersonId == person.PersonId)
        {
            ModelState.AddModelError(nameof(Trn), "This GOV.UK One Login is already connected to this record");
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.PersonId = person.PersonId;
            state.PersonTrn = person.Trn;
        });

        return Redirect(linkGenerator.OneLogins.OneLoginDetail.ConnectPerson.Match(OneLoginUserSubject, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
    }
}

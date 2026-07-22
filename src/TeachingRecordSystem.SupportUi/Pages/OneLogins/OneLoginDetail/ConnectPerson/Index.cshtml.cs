using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

[Journey(JourneyNames.ConnectPerson), StartsJourney]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class IndexModel(
    ConnectPersonJourneyCoordinator journey,
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

    [FromRoute]
    public string OneLoginUserSubject { get; set; } = null!;

    [BindProperty]
    public string? Trn { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? BackLink { get; set; }

    public void OnGet()
    {
        Trn = journey.State.PersonTrn;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return CancelJourney();
        }

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

        return journey.AdvanceTo(
            linkGenerator.OneLogins.OneLoginDetail.ConnectPerson.Match(journey.InstanceId),
            state =>
            {
                state.PersonId = person.PersonId;
                state.PersonTrn = person.Trn;
            });
    }

    private IActionResult CancelJourney()
    {
        journey.DeleteInstance();
        return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
    }

    public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink() ?? linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject);
    }
}

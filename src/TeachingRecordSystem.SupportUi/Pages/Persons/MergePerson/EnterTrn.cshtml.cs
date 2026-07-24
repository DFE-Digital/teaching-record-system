using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

[Journey(JourneyNames.MergePerson)]
public class EnterTrnModel(
    MergePersonJourneyCoordinator journey,
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<EnterTrnModel> _validator = new()
    {
        v => v.RuleFor(m => m.OtherTrn)
            .NotEmpty().WithMessage("Enter a TRN"),
        v => v.RuleFor(m => m.OtherTrn)
            .Matches(@"^\d+$").WithMessage("TRN must be a number")
            .When(m => !string.IsNullOrEmpty(m.OtherTrn)),
        v => v.RuleFor(m => m.OtherTrn)
            .Length(Person.TrnExactLength).WithMessage("TRN must be 7 digits long")
            .When(m => !string.IsNullOrEmpty(m.OtherTrn))
    };

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? ThisTrn { get; set; }

    [BindProperty]
    public string? OtherTrn { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink() ?? linkGenerator.Persons.PersonDetail.Index(PersonId);

        ThisTrn = journey.State.PersonATrn;
    }

    public IActionResult OnGet()
    {
        OtherTrn = journey.State.PersonBTrn;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        var potentialDuplicates = await journey.GetPotentialDuplicatesAsync(journey.State.PersonAId!.Value);

        if (potentialDuplicates.Any(p => p.IsInvalid))
        {
            return BadRequest();
        }

        _validator.ValidateAndThrow(this);

        if (OtherTrn == ThisTrn)
        {
            ModelState.AddModelError(nameof(OtherTrn), "TRN must be for a different record");
        }

        var otherPerson = await dbContext.Persons
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.Trn == OtherTrn);

        if (otherPerson is null)
        {
            ModelState.AddModelError(nameof(OtherTrn), "No record found with that TRN");
        }
        else if (otherPerson.Status != PersonStatus.Active)
        {
            ModelState.AddModelError(nameof(OtherTrn), "The TRN you entered belongs to a deactivated record");
        }

        if (!ModelState.IsValid || otherPerson is null)
        {
            return this.PageWithErrors();
        }

        return journey.AdvanceTo(
            linkGenerator.Persons.MergePerson.Matches(journey.InstanceId),
            state =>
            {
                state.PersonBId = otherPerson.PersonId;
                state.PersonBTrn = otherPerson.Trn;
            });
    }
}

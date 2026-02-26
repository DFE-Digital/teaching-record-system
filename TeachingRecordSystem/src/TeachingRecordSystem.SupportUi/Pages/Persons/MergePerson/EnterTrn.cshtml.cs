using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

[Journey(JourneyNames.MergePerson), RequireJourneyInstance]
public class EnterTrnModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceUploadManager)
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

    public string? ThisTrn { get; set; }

    [BindProperty]
    public string? OtherTrn { get; set; }

    public string BackLink => GetPageLink(FromCheckAnswers ? MergePersonJourneyPage.CheckAnswers : null);

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        ThisTrn = JourneyInstance!.State.PersonATrn;
    }

    public IActionResult OnGet()
    {
        OtherTrn = JourneyInstance!.State.PersonBTrn;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var potentialDuplicates = await GetPotentialDuplicatesAsync(JourneyInstance!.State.PersonAId!.Value);

        if (potentialDuplicates.Any(p => p.IsInvalid))
        {
            return BadRequest();
        }

        _validator.ValidateAndThrow(this);

        if (OtherTrn == ThisTrn)
        {
            ModelState.AddModelError(nameof(OtherTrn), "TRN must be for a different record");
        }

        var otherPerson = await DbContext.Persons
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
        else
        {
            await JourneyInstance!.UpdateStateAsync(state =>
            {
                state.PersonBId = otherPerson.PersonId;
                state.PersonBTrn = otherPerson.Trn;
            });

            return Redirect(GetPageLink(FromCheckAnswers ? MergePersonJourneyPage.CheckAnswers : MergePersonJourneyPage.Matches));
        }
    }
}

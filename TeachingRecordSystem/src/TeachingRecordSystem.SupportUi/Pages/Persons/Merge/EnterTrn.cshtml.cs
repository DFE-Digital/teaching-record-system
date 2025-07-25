using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

[RequireFeatureEnabledFilterFactory(FeatureNames.ContactsMigrated)]
[Journey(JourneyNames.MergePerson), ActivatesJourney, RequireJourneyInstance]
public class EnterTrnModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    private static readonly InductionStatus[] _invalidInductionStatusesForMerge = [InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed];

    public string? ThisTrn { get; set; }

    [Display(Name = "Enter the TRN of the other record you want to merge")]
    [Required(ErrorMessage = "Enter a TRN")]
    [RegularExpression(@"^\d+$", ErrorMessage = "TRN must be a number")]
    [MaxLength(Person.TrnExactLength, ErrorMessage = "TRN must be 7 digits long")]
    [MinLength(Person.TrnExactLength, ErrorMessage = "TRN must be 7 digits long")]
    [BindProperty]
    public string? OtherTrn { get; set; }

    public string BackLink => GetPageLink(null);

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
        var personA = (await DbContext.Persons
            .SingleOrDefaultAsync(p =>
                p.PersonId == JourneyInstance!.State.PersonAId &&
                !p.Alerts!.Any(a => a.IsOpen) &&
                !_invalidInductionStatusesForMerge.Contains(p.InductionStatus)));

        if (personA is null)
        {
            return BadRequest();
        }

        if (OtherTrn == ThisTrn)
        {
            ModelState.AddModelError(nameof(OtherTrn), "TRN must be for a different record");
        }

        var otherPerson = OtherTrn is null
            ? null
            : await DbContext.Persons
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

            return Redirect(GetPageLink(MergeJourneyPage.CompareMatchingRecords));
        }
    }
}

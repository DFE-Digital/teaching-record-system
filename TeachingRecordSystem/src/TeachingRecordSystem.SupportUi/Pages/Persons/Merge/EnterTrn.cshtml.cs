using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
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
    [Display(Name = "Enter the TRN of the other record you want to merge")]
    [Required(ErrorMessage = "Enter a TRN")]
    [RegularExpression(@"^\d+$", ErrorMessage = "TRN must be a number")]
    [MaxLength(Person.TrnExactLength, ErrorMessage = "TRN must be 7 digits long")]
    [MinLength(Person.TrnExactLength, ErrorMessage = "TRN must be 7 digits long")]
    [BindProperty]
    public string? OtherTrn { get; set; }

    public string BackLink => GetPageLink(null);

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (OtherTrn == ThisTrn)
        {
            ModelState.AddModelError(nameof(OtherTrn), "TRN must be for a different record");
        }

        if (ModelState.IsValid)
        {
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
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.OtherTrn = OtherTrn;
        });

        return Redirect(GetPageLink(MergeJourneyPage.CompareMatchingRecords));
    }
}

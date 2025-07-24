using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

[RequireFeatureEnabledFilterFactory(FeatureNames.ContactsMigrated)]
[Journey(JourneyNames.MergePerson), RequireJourneyInstance]
public class SelectDetailsToMergeModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    public string BackLink => GetPageLink(MergeJourneyPage.CompareMatchingRecords);

    public IActionResult OnGet()
    {
        return Page();
    }

    public IActionResult OnPost()
    {
        return Page();
    }
}

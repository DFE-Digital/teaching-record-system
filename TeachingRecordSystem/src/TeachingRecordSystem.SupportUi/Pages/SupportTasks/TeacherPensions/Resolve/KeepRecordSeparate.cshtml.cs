using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance]
public class KeepRecordSeparate(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : ResolveTeacherPensionsPotentialDuplicatePageModel(dbContext)
{
    [BindProperty]
    [Display(Name = "Add comments (optional)")]
    public bool? RecordsMatch { get; set; }

    public void OnGet()
    {
    }
}

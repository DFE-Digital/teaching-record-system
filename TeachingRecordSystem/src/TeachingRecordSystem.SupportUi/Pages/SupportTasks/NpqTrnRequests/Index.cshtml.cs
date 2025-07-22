using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests;

public class IndexModel(TrsLinkGenerator linkGenerator, IFileService fileService) : PageModel
{
    public string PersonName => string.Join(" ", SupportTask!.TrnRequestMetadata!.Name);

    public SupportTask? SupportTask { get; set; }

    public string SourceApplicationUserName => SupportTask!.TrnRequestMetadata!.ApplicationUser!.Name;
    public bool? NpqWorkingInEducationalSetting { get; set; }
    public string? NpqApplicationId { get; set; }
    public string? NpqName { get; set; }
    public string? NpqTrainingProvider { get; set; }

    public Guid? NpqEvidenceFileId { get; set; }
    public string? NpqEvidenceFileName { get; set; }
    public string? NpqEvidenceFileUrl { get; set; }

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you want to create a record from this request")]
    public bool CreateRecord { get; set; }

    public async Task OnGetAsync()
    {
        NpqWorkingInEducationalSetting = SupportTask!.TrnRequestMetadata?.NpqWorkingInEducationalSetting;
        NpqApplicationId = SupportTask.TrnRequestMetadata?.NpqApplicationId;
        NpqName = SupportTask.TrnRequestMetadata?.NpqName;
        NpqTrainingProvider = SupportTask.TrnRequestMetadata?.NpqTrainingProvider;
        NpqEvidenceFileId = SupportTask.TrnRequestMetadata?.NpqEvidenceFileId;
        NpqEvidenceFileName = SupportTask.TrnRequestMetadata?.NpqEvidenceFileName;

        NpqEvidenceFileUrl = NpqEvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(NpqEvidenceFileId!.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid == false)
        {
            return this.PageWithErrors();
        }

        if (CreateRecord)
        {
            // CML TODO later this will conditionally redirect to the matches page or the create new record page if there are no matches
            return Redirect(linkGenerator.NpqTrnRequestMatches(SupportTaskReference));
        }
        else
        {
            return Redirect(linkGenerator.NpqTrnRequestRejectionReason(SupportTaskReference));
        }
    }

    public IActionResult OnPostCancel()
    {
        return Redirect(linkGenerator.SupportTasks());
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTaskFeature = context.HttpContext.GetCurrentSupportTaskFeature();
        SupportTask = supportTaskFeature.SupportTask;

        return base.OnPageHandlerExecutionAsync(context, next);
    }
}

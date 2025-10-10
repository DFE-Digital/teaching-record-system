using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests;

public class DetailsModel(TrsLinkGenerator linkGenerator, IFileService fileService) : PageModel
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

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid == false)
        {
            return this.PageWithErrors();
        }

        if (CreateRecord)
        {
            return (SupportTask!.TrnRequestMetadata!.PotentialDuplicate ?? false) ?
               Redirect(linkGenerator.NpqTrnRequestResolve(SupportTaskReference)) :
               Redirect(linkGenerator.NpqTrnRequestNoMatches(SupportTaskReference));
        }
        else
        {
            return Redirect(linkGenerator.NpqTrnRequestReject(SupportTaskReference));
        }
    }

    public IActionResult OnPostCancel()
    {
        return Redirect(linkGenerator.NpqTrnRequests());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTaskFeature = context.HttpContext.GetCurrentSupportTaskFeature();
        SupportTask = supportTaskFeature.SupportTask;

        NpqWorkingInEducationalSetting = SupportTask!.TrnRequestMetadata?.NpqWorkingInEducationalSetting;
        NpqApplicationId = SupportTask.TrnRequestMetadata?.NpqApplicationId;
        NpqName = SupportTask.TrnRequestMetadata?.NpqName;
        NpqTrainingProvider = SupportTask.TrnRequestMetadata?.NpqTrainingProvider;
        NpqEvidenceFileId = SupportTask.TrnRequestMetadata?.NpqEvidenceFileId;
        NpqEvidenceFileName = SupportTask.TrnRequestMetadata?.NpqEvidenceFileName;

        NpqEvidenceFileUrl = NpqEvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(NpqEvidenceFileId!.Value, UiDefaults.FileUrlExpiry) :
            null;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

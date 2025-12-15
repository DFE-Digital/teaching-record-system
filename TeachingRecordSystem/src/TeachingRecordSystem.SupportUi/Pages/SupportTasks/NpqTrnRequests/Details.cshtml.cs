using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests;

public class DetailsModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<DetailsModel> _validator = new()
    {
        v => v.RuleFor(m => m.CreateRecord)
            .NotNull().WithMessage("Select yes if you want to create a record from this request")
    };

    public string PersonName => string.Join(" ", SupportTask!.TrnRequestMetadata!.Name);

    public SupportTask? SupportTask { get; set; }

    public string SourceApplicationUserName => SupportTask!.TrnRequestMetadata!.ApplicationUser!.Name;
    public bool? NpqWorkingInEducationalSetting { get; set; }
    public string? NpqApplicationId { get; set; }
    public string? NpqName { get; set; }
    public string? NpqTrainingProvider { get; set; }

    public UploadedEvidenceFile? NpqEvidenceFile { get; set; }

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    [BindProperty(SupportsGet = true)]
    public bool? CreateRecord { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        if (CreateRecord!.Value)
        {
            return SupportTask!.TrnRequestMetadata!.PotentialDuplicate ?
               Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Resolve.Index(SupportTaskReference)) :
               Redirect(linkGenerator.SupportTasks.NpqTrnRequests.NoMatches.Index(SupportTaskReference));
        }
        else
        {
            return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Reject.Index(SupportTaskReference));
        }
    }

    public IActionResult OnPostCancel()
    {
        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Index());
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var supportTaskFeature = context.HttpContext.GetCurrentSupportTaskFeature();
        SupportTask = supportTaskFeature.SupportTask;
        var metadata = SupportTask!.TrnRequestMetadata;

        NpqWorkingInEducationalSetting = metadata?.NpqWorkingInEducationalSetting;
        NpqApplicationId = metadata?.NpqApplicationId;
        NpqName = metadata?.NpqName;
        NpqTrainingProvider = metadata?.NpqTrainingProvider;
        NpqEvidenceFile = (metadata?.NpqEvidenceFileId, metadata?.NpqEvidenceFileName) is (Guid fileId, string fileName)
            ? new(fileId, fileName) : null;
    }
}

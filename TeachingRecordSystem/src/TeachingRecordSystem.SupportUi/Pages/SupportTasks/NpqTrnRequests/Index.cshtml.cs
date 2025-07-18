using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests;

public class IndexModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : PageModel
{
    public string PersonName => string.Join(" ", SupportTask.TrnRequestMetadata!.Name);

    public TrnRequestMetadata? RequestData { get; set; } // CML TODO - need this - this page needs the SupportTask, so it's part of that
    public SupportTask SupportTask { get; set; } // CML TODO - need this? or use the getRequestData method below when needed?

    public string SourceApplicationUserName => RequestData!.ApplicationUser!.Name;

    [FromRoute]
    public string SupportTaskReference { get; set; }

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
            // later this will conditionally redirect to the matches page or the create new record page if there are no matches
            return Redirect(linkGenerator.NpqTrnRequestMatches(SupportTaskReference));
        }
        else
        {
            return Redirect(linkGenerator.NpqTrnRequestRejectionReason(SupportTaskReference));
        }
    }

    public void OnPostCancel()
    {
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTaskFeature = context.HttpContext.GetCurrentSupportTaskFeature();
        SupportTask = supportTaskFeature.SupportTask;

        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        RequestData = supportTask.TrnRequestMetadata!;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

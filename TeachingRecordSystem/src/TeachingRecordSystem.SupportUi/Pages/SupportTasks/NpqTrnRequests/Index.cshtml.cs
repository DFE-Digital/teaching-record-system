using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests;

public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public string PersonName => string.Join(" ", SupportTask!.TrnRequestMetadata!.Name);

    public SupportTask? SupportTask { get; set; }

    public string SourceApplicationUserName => SupportTask!.TrnRequestMetadata!.ApplicationUser!.Name;

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
            // later this will conditionally redirect to the matches page or the create new record page if there are no matches
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

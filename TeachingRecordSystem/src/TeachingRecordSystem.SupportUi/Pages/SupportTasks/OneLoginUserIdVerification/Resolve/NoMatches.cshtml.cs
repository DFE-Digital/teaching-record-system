using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserIdVerification;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), RequireJourneyInstance]
public class NoMatches(
    SupportUiLinkGenerator linkGenerator,
    OneLoginUserIdVerificationSupportTaskService supportTaskService,
    OneLoginService oneLoginService,
    IClock clock) : PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public JourneyInstance<ResolveOneLoginUserIdVerificationState> JourneyInstance { get; set; } = null!;

    public string? Name { get; set; }

    public string? EmailContent { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(bool cancel)
    {
        if (cancel)
        {
            await JourneyInstance.DeleteAsync();

            return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Index());
        }

        var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, clock.UtcNow, User.GetUserId());

        await supportTaskService.ResolveSupportTaskAsync(
            (VerifiedOnlyWithoutMatchesOutcomeOptions)new()
            {
                SupportTask = _supportTask!
            },
            processContext);

        await JourneyInstance.DeleteAsync();

        TempData.SetFlashSuccess(
            "Email sent",
            $"Request closed for {Name}.");

        return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (JourneyInstance.State.Verified is not true)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Index(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        if (JourneyInstance.State.MatchedPersons.Count > 0)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Matches(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        _supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var data = _supportTask.GetData<OneLoginUserIdVerificationData>();

        Name = $"{data.StatedFirstName} {data.StatedLastName}";

        EmailContent = await oneLoginService.GetRecordNotFoundEmailContentAsync(Name);

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

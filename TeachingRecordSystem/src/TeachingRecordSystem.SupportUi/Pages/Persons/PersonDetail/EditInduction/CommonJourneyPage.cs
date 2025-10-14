using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public abstract class CommonJourneyPage(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController) : PageModel
{
    public JourneyInstance<EditInductionState>? JourneyInstance { get; set; }

    protected TrsDbContext DbContext { get; } = dbContext;
    protected TrsLinkGenerator LinkGenerator { get; } = linkGenerator;
    protected EvidenceUploadManager EvidenceController { get; } = evidenceController;

    [FromRoute]
    public Guid PersonId { get; set; }
    public string? PersonName { get; set; }

    [FromQuery]
    public JourneyFromCheckYourAnswersPage? FromCheckAnswers { get; set; }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await EvidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.PersonInduction(PersonId));
    }

    protected string GetPageLink(InductionJourneyPage? pageName, JourneyFromCheckYourAnswersPage? fromCheckYourAnswersPage = null)
    {
        return pageName switch
        {
            InductionJourneyPage.Status => LinkGenerator.PersonInductionEditStatus(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.CompletedDate => LinkGenerator.PersonInductionEditCompletedDate(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.ExemptionReason => LinkGenerator.PersonInductionEditExemptionReason(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.StartDate => LinkGenerator.PersonInductionEditStartDate(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.ChangeReasons => LinkGenerator.PersonInductionChangeReason(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.CheckAnswers => LinkGenerator.PersonInductionCheckYourAnswers(PersonId, JourneyInstance!.InstanceId),
            _ => LinkGenerator.PersonInduction(PersonId)
        };
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        OnPageHandlerExecuting(context);
        await OnPageHandlerExecutingAsync(context);
        if (context.Result == null)
        {
            var executedContext = await next();
            OnPageHandlerExecuted(executedContext);
            await OnPageHandlerExecutedAsync(executedContext);
        }
    }

    protected virtual InductionJourneyPage StartPage { get => InductionJourneyPage.Status; }

    protected virtual async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(DbContext, PersonId, StartPage);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
    }

    protected virtual Task OnPageHandlerExecutedAsync(PageHandlerExecutedContext context)
        => Task.CompletedTask;
}

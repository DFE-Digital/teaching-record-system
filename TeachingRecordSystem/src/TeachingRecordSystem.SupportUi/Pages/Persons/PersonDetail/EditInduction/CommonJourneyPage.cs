using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public abstract class CommonJourneyPage(
    PersonService personService,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public JourneyInstance<EditInductionState>? JourneyInstance { get; set; }

    protected PersonService PersonService { get; } = personService;
    protected SupportUiLinkGenerator LinkGenerator { get; } = linkGenerator;
    protected EvidenceUploadManager EvidenceUploadManager { get; } = evidenceUploadManager;

    [FromRoute]
    public Guid PersonId { get; set; }
    public string? PersonName { get; set; }

    [FromQuery]
    public JourneyFromCheckAnswersPage? FromCheckAnswers { get; set; }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await EvidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.Persons.PersonDetail.Induction(PersonId));
    }

    protected string GetPageLink(InductionJourneyPage? pageName, JourneyFromCheckAnswersPage? fromCheckYourAnswersPage = null)
    {
        return pageName switch
        {
            InductionJourneyPage.Status => LinkGenerator.Persons.PersonDetail.EditInduction.Status(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.CompletedDate => LinkGenerator.Persons.PersonDetail.EditInduction.CompletedDate(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.ExemptionReason => LinkGenerator.Persons.PersonDetail.EditInduction.ExemptionReasons(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.StartDate => LinkGenerator.Persons.PersonDetail.EditInduction.StartDate(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.ChangeReasons => LinkGenerator.Persons.PersonDetail.EditInduction.Reason(PersonId, JourneyInstance!.InstanceId, fromCheckYourAnswersPage),
            InductionJourneyPage.CheckAnswers => LinkGenerator.Persons.PersonDetail.EditInduction.CheckAnswers(PersonId, JourneyInstance!.InstanceId),
            _ => LinkGenerator.Persons.PersonDetail.Induction(PersonId)
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
        if (!JourneyInstance!.State.Initialized)
        {
            var person = await PersonService.GetPersonAsync(PersonId);

            if (person is null)
            {
                context.Result = NotFound();
                return;
            }

            JourneyInstance!.State.EnsureInitialized(person, StartPage);
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
    }

    protected virtual Task OnPageHandlerExecutedAsync(PageHandlerExecutedContext context)
        => Task.CompletedTask;
}

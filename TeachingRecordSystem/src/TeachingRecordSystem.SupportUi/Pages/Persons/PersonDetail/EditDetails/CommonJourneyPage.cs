using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public abstract class CommonJourneyPage(
    PersonService personService,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public JourneyInstance<EditDetailsState>? JourneyInstance { get; set; }

    protected PersonService PersonService { get; } = personService;
    protected SupportUiLinkGenerator LinkGenerator { get; } = linkGenerator;
    protected EvidenceUploadManager EvidenceUploadManager { get; } = evidenceUploadManager;

    [FromRoute]
    public Guid PersonId { get; set; }
    public string? PersonName { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await EvidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.NameChangeEvidence.UploadedEvidenceFile);
        await EvidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.OtherDetailsChangeEvidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    protected string GetPageLink(EditDetailsJourneyPage? pageName, bool? fromCheckAnswers = null)
    {
        return pageName switch
        {
            EditDetailsJourneyPage.PersonalDetails => LinkGenerator.Persons.PersonDetail.EditDetails.Index(PersonId, JourneyInstance!.InstanceId, fromCheckAnswers),
            EditDetailsJourneyPage.NameChangeReason => LinkGenerator.Persons.PersonDetail.EditDetails.NameChangeReason(PersonId, JourneyInstance!.InstanceId, fromCheckAnswers),
            EditDetailsJourneyPage.OtherDetailsChangeReason => LinkGenerator.Persons.PersonDetail.EditDetails.OtherDetailsChangeReason(PersonId, JourneyInstance!.InstanceId, fromCheckAnswers),
            EditDetailsJourneyPage.CheckAnswers => LinkGenerator.Persons.PersonDetail.EditDetails.CheckAnswers(PersonId, JourneyInstance!.InstanceId),
            _ => LinkGenerator.Persons.PersonDetail.Index(PersonId)
        };
    }

    public EditDetailsJourneyPage NextIncompletePage =>
        !JourneyInstance!.State.IsPersonalDetailsComplete
            ? EditDetailsJourneyPage.PersonalDetails
            : !JourneyInstance!.State.IsNameChangeReasonComplete
                ? EditDetailsJourneyPage.NameChangeReason
                : !JourneyInstance!.State.IsOtherDetailsChangeReasonComplete
                    ? EditDetailsJourneyPage.OtherDetailsChangeReason
                    : EditDetailsJourneyPage.CheckAnswers;

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var person = await PersonService.GetPersonAsync(PersonId);

        if (person is null)
        {
            context.Result = NotFound();
            return;
        }

        JourneyInstance!.State.EnsureInitialized(person);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        OnPageHandlerExecuting(context);
        await OnPageHandlerExecutingAsync(context);
        if (context.Result == null)
        {
            var executedContext = await next();
            OnPageHandlerExecuted(executedContext);
            await OnPageHandlerExecutedAsync(executedContext);
        }
    }

    protected virtual Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
        => Task.CompletedTask;

    protected virtual Task OnPageHandlerExecutedAsync(PageHandlerExecutedContext context)
        => Task.CompletedTask;
}

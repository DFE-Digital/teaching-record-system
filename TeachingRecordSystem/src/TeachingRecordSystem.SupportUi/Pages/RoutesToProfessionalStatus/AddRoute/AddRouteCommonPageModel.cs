using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public abstract class AddRouteCommonPageModel(
    AddRoutePage currentPage,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceController)
    : PageModel
{
    public AddRoutePage CurrentPage => currentPage;

    public abstract AddRoutePage? NextPage { get; }
    public abstract AddRoutePage? PreviousPage { get; }

    public string BackLink
    {
        get
        {
            var previousPage = PreviousPage ??
                               (FromCheckAnswers ?? false ? AddRoutePage.CheckAnswers : AddRoutePage.Route);

            return (currentPage, FromCheckAnswers) switch
            {
                (_, true) => LinkGenerator.RoutesToProfessionalStatus.AddRoute.CheckAnswers(PersonId, JourneyInstance!.InstanceId),
                (AddRoutePage.Route, _) => LinkGenerator.Persons.PersonDetail.Qualifications(PersonId),
                _ => LinkGenerator.RoutesToProfessionalStatus.AddRoute.AddRoutePage(previousPage, PersonId, JourneyInstance!.InstanceId, FromCheckAnswers)
            };
        }
    }

    protected SupportUiLinkGenerator LinkGenerator => linkGenerator;

    protected ReferenceDataCache ReferenceDataCache => referenceDataCache;
    protected EvidenceUploadManager EvidenceUploadManager { get; } = evidenceController;

    public JourneyInstance<AddRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public string PageCaption => $"Add a route - {PersonName}";

    protected Task<IActionResult> ContinueAsync()
    {
        IActionResult nextPage = Redirect((currentPage, FromCheckAnswers) switch
        {
            (AddRoutePage.CheckAnswers, _) => LinkGenerator.Persons.PersonDetail.Qualifications(PersonId),
            (_, true) => LinkGenerator.RoutesToProfessionalStatus.AddRoute.CheckAnswers(PersonId, JourneyInstance!.InstanceId),
            _ => LinkGenerator.RoutesToProfessionalStatus.AddRoute.AddRoutePage(NextPage ?? AddRoutePage.CheckAnswers, PersonId, JourneyInstance!.InstanceId, FromCheckAnswers)
        });

        return Task.FromResult(nextPage);
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await EvidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.ChangeReasonDetail.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

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

    public virtual Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
        => Task.CompletedTask;

    public virtual Task OnPageHandlerExecutedAsync(PageHandlerExecutedContext context)
        => Task.CompletedTask;
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public abstract class AddRouteCommonPageModel(AddRoutePage currentPage, TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
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
                               (FromCheckAnswers ?? false ? AddRoutePage.CheckYourAnswers : AddRoutePage.Route);

            return (currentPage, FromCheckAnswers) switch
            {
                (_, true) => LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId),
                (AddRoutePage.Route, _) => LinkGenerator.PersonQualifications(PersonId),
                _ => LinkGenerator.RouteAddPage(previousPage, PersonId, JourneyInstance!.InstanceId, FromCheckAnswers)
            };
        }
    }

    protected TrsLinkGenerator LinkGenerator => linkGenerator;

    protected ReferenceDataCache ReferenceDataCache => referenceDataCache;

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
            (AddRoutePage.CheckYourAnswers, _) => LinkGenerator.PersonQualifications(PersonId),
            (_, true) => LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId),
            _ => LinkGenerator.RouteAddPage(NextPage ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId, FromCheckAnswers)
        });

        return Task.FromResult(nextPage);
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.PersonQualifications(PersonId));
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

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
            var previousPage = GetPreviousPage() ??
                               (FromCheckAnswers ? AddRoutePage.CheckYourAnswers : AddRoutePage.Route);

            return (currentPage, FromCheckAnswers) switch
            {
                // (_, true) => LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId),

                (AddRoutePage.Route, true) => LinkGenerator.RouteAddCheckYourAnswers(PersonId,
                    JourneyInstance!.InstanceId,
                    PreviousHistory.Any() ? PreviousHistory : History, []),

                (AddRoutePage.Route, false) => LinkGenerator.PersonQualifications(PersonId),

                _ => previousPage == AddRoutePage.CheckYourAnswers
                    ? LinkGenerator.RouteAddPage(previousPage, PersonId, JourneyInstance!.InstanceId, FromCheckAnswers, PreviousHistory.Any() ? PreviousHistory : History, [])
                    : LinkGenerator.RouteAddPage(previousPage, PersonId, JourneyInstance!.InstanceId, FromCheckAnswers, History, PreviousHistory),
            };
        }
    }

    protected TrsLinkGenerator LinkGenerator => linkGenerator;

    protected ReferenceDataCache ReferenceDataCache => referenceDataCache;

    public JourneyInstance<AddRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromQuery]
    public List<AddRoutePage> History { get; set; } = new();

    [FromQuery]
    public List<AddRoutePage> PreviousHistory { get; set; } = new();

    private AddRoutePage? GetPreviousPage() =>
        History.OrderByDescending(p => p).Select(p => (AddRoutePage?)p).FirstOrDefault(p => p < currentPage);

    [FromQuery]

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    protected async Task<IActionResult> ContinueAsync()
    {
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            // if (state.History.Contains(AddRoutePage.CheckYourAnswers))
            // {
            //     state.History.Clear();
            //     state.History.Add(CurrentPage);
            // }
            //
            // if (CurrentPage != AddRoutePage.CheckYourAnswers)
            // {
            //     var nextPage = FromCheckAnswers
            //         ? AddRoutePage.CheckYourAnswers
            //         : NextPage;
            //
            //     state.History.Add(nextPage ?? AddRoutePage.CheckYourAnswers);
            // }
        });

        var history = History.Union([CurrentPage]).ToList();

        return Redirect((currentPage, FromCheckAnswers) switch
        {
            (AddRoutePage.CheckYourAnswers, _) => LinkGenerator.PersonQualifications(PersonId),
            (_, true) => LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId, history, PreviousHistory),
            _ => LinkGenerator.RouteAddPage(NextPage ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId, FromCheckAnswers, history, PreviousHistory)
        });
    }

    public AddRoutePage? NextPage(AddRoutePage currentPage)
    {
        return PageDriver.NextPage(Route, Status, currentPage);
    }

    public AddRoutePage? PreviousPage(AddRoutePage currentPage)
    {
        return PageDriver.PreviousPage(Route, Status, currentPage);
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

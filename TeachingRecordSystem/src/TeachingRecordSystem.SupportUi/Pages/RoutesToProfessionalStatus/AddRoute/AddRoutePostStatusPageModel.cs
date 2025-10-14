using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public abstract class AddRoutePostStatusPageModel(
    AddRoutePage currentPage,
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceController)
    : AddRouteCommonPageModel(currentPage, linkGenerator, referenceDataCache, evidenceController)
{
    public RouteToProfessionalStatusType RouteType { get; set; } = null!;

    public RouteToProfessionalStatusStatus Status { get; set; }

    public override AddRoutePage? NextPage => PageDriver.NextPage(RouteType, Status, CurrentPage);

    public override AddRoutePage? PreviousPage => PageDriver.PreviousPage(RouteType, Status, CurrentPage);

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        if (!(JourneyInstance!.State.RouteToProfessionalStatusId.HasValue && JourneyInstance!.State.Status.HasValue))
        {
            context.Result = BadRequest();
            return;
        }

        RouteType = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId.Value);
        Status = JourneyInstance!.State.Status!.Value;

        await base.OnPageHandlerExecutingAsync(context);
    }
}

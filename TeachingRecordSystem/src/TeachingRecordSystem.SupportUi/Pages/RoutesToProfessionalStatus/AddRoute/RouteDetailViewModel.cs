using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public class RouteDetailViewModel : RouteDetailModel
{
    public required Guid PersonId { get; init; }
    public required List<AddRoutePage> ChangeLinkHistory { get; init; }
    public required List<AddRoutePage> ChangeLinkPreviousHistory { get; init; }
}

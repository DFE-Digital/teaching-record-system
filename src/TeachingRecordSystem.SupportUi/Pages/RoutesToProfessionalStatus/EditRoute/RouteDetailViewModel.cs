using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

public class RouteDetailViewModel : RouteDetailModel
{
    public required JourneyInstanceId InstanceId { get; init; }

    // The URL a change link brings the user back to once they've answered the question again; null on
    // the detail page, which is where they end up by default.
    public string? ReturnUrl { get; init; }
}

using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

public class RoutesToProfessionalStatusLinkGenerator(LinkGenerator linkGenerator)
{
    public AddRouteLinkGenerator AddRoute => new(linkGenerator);
    public EditRouteLinkGenerator EditRoute => new(linkGenerator);
    public DeleteRouteLinkGenerator DeleteRoute => new(linkGenerator);
}

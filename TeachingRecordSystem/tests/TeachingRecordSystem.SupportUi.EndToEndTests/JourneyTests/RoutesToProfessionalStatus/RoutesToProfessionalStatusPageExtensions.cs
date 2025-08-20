using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.RoutesToProfessionalStatus;

public static class RoutesToProfessionalStatusPageExtensions
{
    public static Task AssertOnRouteDetailPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/detail");
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute
{
    [Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
    public class StatusModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : PageModel
    {
        public JourneyInstance<AddRouteState>? JourneyInstance { get; set; }

        [FromQuery]
        public bool FromCheckAnswers { get; set; }

        [FromQuery]
        public Guid PersonId { get; set; }

        public string? PersonName { get; set; }

        [BindProperty]
        ProfessionalStatusStatus Status { get; set; }

        public string BackLink => FromCheckAnswers ?
            linkGenerator.RouteAddCheckAnswers(PersonId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteAddRoute(PersonId, JourneyInstance!.InstanceId);

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // CML TODO - undo all these hard-coded values
            Status = ProfessionalStatusStatus.Approved;
            await JourneyInstance!.UpdateStateAsync(x => x.Status = Status);
            return Redirect(linkGenerator.RouteAddStartDate(PersonId, JourneyInstance!.InstanceId));
        }
    }
}

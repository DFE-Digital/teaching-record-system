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
            linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteAddRoute(PersonId, JourneyInstance!.InstanceId);

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // CML TODO - undo all these hard-coded values
            // this page is only a stub at the moment to allow for testing of start-date
            Status = ProfessionalStatusStatus.InTraining;
            var route = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId!.Value);
            await JourneyInstance!.UpdateStateAsync(x => x.Status = Status);
            var nextPage = PageDriver.NextPage(route, Status, AddRoutePage.Status) ?? AddRoutePage.CheckYourAnswers;
            return Redirect(linkGenerator.RouteAddPage(nextPage, PersonId, JourneyInstance!.InstanceId));
        }
    }
}

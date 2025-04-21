using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

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

        public RouteToProfessionalStatus Route { get; set; } = null!;

        public ProfessionalStatusStatusInfo[] Statuses { get; set; } = [];

        [BindProperty]
        public ProfessionalStatusStatus? Status { get; set; }

        public string BackLink => FromCheckAnswers ?
            linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteAddRoute(PersonId, JourneyInstance!.InstanceId);

        public void OnGet()
        {
            Status = JourneyInstance!.State.Status;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await JourneyInstance!.UpdateStateAsync(x => x.Status = Status);
            // CML TODO needs all the logic that's in the edit, because of the ability to change the status from CYA
            // - probably virtuall impossible to deal with if ca edit route as well - many combinations of change of fields needed
            // - could ask whether can make status and route not editable
            var nextPage = PageDriver.NextPage(Route, Status!.Value, AddRoutePage.Status) ?? AddRoutePage.CheckYourAnswers;
            return Redirect(FromCheckAnswers ?
                linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
                linkGenerator.RouteAddPage(nextPage, PersonId, JourneyInstance!.InstanceId));
        }

        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            Statuses = ProfessionalStatusStatusRegistry.All.ToArray();
            Route = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId!.Value);
            var personInfo = context.HttpContext.GetCurrentPersonFeature();
            PersonName = personInfo.Name;
            PersonId = personInfo.PersonId;

            await next();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[Journey(JourneyNames.AddPerson), StartsJourney]
public class IndexModel(AddPersonJourneyCoordinator journey, SupportUiLinkGenerator linkGenerator) : PageModel
{
    public IActionResult OnGet() =>
        journey.AdvanceTo(
            linkGenerator.Persons.AddPerson.PersonalDetails(journey.InstanceId),
            new PushStepOptions { SetAsFirstStep = true });
}

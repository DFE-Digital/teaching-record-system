using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

[Journey(JourneyNames.MergePerson), StartsJourney]
public class IndexModel(MergePersonJourneyCoordinator journey, SupportUiLinkGenerator linkGenerator) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    public IActionResult OnGet() =>
        journey.AdvanceTo(
            linkGenerator.Persons.MergePerson.EnterTrn(journey.InstanceId),
            new PushStepOptions { SetAsFirstStep = true });
}

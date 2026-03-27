using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class TrnDeferredModel(SignInJourneyCoordinator coordinator) : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        await coordinator.UpdateStateAsync(async state =>
        {
            await coordinator.CompleteWithDeferredMatchingAsync(state);
        });
        return coordinator.GetNextPage().ToActionResult();
    }
}

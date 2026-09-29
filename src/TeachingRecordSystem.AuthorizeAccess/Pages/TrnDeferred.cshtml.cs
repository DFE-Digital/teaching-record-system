using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class TrnDeferredModel(SignInJourneyCoordinator coordinator) : PageModel
{
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        // The user may have been connected to a teaching record in another journey while this page was
        // open, which makes a TRN request redundant
        if (await coordinator.TryAdvanceIfVerifiedOrConnectedAsync(cancellationToken) is { } nextPage)
        {
            return nextPage.ToActionResult();
        }

        await coordinator.UpdateStateAsync(async state =>
        {
            await coordinator.CompleteWithDeferredMatchingAsync(state, cancellationToken: cancellationToken);
        });
        return coordinator.GetNextPage().ToActionResult();
    }
}

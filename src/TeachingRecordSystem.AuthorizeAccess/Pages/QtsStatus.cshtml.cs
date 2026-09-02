using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class QtsStatusModel(SignInJourneyCoordinator coordinator) : PageModel
{
    private readonly InlineValidator<QtsStatusModel> _validator = new()
    {
        v => v.RuleFor(m => m.HaveQts)
            .NotNull()
            .WithMessage("Select yes if you have qualified teacher status (QTS)")
    };

    [BindProperty]
    public bool? HaveQts { get; set; }

    public void OnGet()
    {
        HaveQts = coordinator.State.HaveQts;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await this.ThrowIfInvalidAsync(_validator);

        coordinator.UpdateState(state => state.SetQts(HaveQts!.Value));

        return HaveQts == true
            ? coordinator.AdvanceTo(links => links.QtsDetails())
            : coordinator.AdvanceTo(links => links.CheckAnswers());
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class Name(SignInJourneyCoordinator coordinator) : PageModel
{
    private readonly InlineValidator<Name> _validator = new()
    {
        v => v.RuleFor(m => m.FirstName)
            .NotNull()
            .WithMessage("Enter your first name")
            .MaximumLength(200)
            .WithMessage("First name must be 200 characters or less"),
        v => v.RuleFor(m => m.LastName)
            .NotNull()
            .WithMessage("Enter your last name")
            .MaximumLength(200)
            .WithMessage("Last name must be 200 characters or less"),
    };

    [BindProperty]
    public string? FirstName { get; set; }

    [BindProperty]
    public string? LastName { get; set; }

    public void OnGet()
    {
        FirstName = coordinator.State.FirstName;
        LastName = coordinator.State.LastName;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        coordinator.UpdateState(state => state.SetName(FirstName!, LastName!));

        return coordinator.AdvanceTo(links => links.DateOfBirth());
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class DateOfBirthModel(SignInJourneyCoordinator coordinator) : PageModel
{
    private readonly InlineValidator<DateOfBirthModel> _validator = new()
    {
        v => v.RuleFor(m => m.DateOfBirth)
            .NotNull()
            .WithMessage("Enter your date of birth")
    };

    [BindProperty]
    public DateOnly? DateOfBirth { get; set; }

    public void OnGet()
    {
        DateOfBirth = coordinator.State.DateOfBirth;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        coordinator.UpdateState(state => state.SetDateOfBirth(DateOfBirth!.Value));

        return coordinator.AdvanceTo(links => links.NationalInsuranceNumber());
    }
}

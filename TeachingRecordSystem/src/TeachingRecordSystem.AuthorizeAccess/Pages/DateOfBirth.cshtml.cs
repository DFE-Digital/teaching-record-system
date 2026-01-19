using System.ComponentModel.DataAnnotations;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class DateOfBirthModel(SignInJourneyCoordinator coordinator) : PageModel
{
    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Date of birth")]
    [Required(ErrorMessage = "Enter your date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    public void OnGet()
    {
        DateOfBirth = coordinator.State.DateOfBirth;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        coordinator.UpdateState(state => state.SetDateOfBirth(DateOfBirth!.Value));

        return coordinator.AdvanceTo(links => links.NationalInsuranceNumber());
    }
}

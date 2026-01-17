using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class TrnModel(SignInJourneyCoordinator coordinator) : PageModel
{
    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "Do you have a teacher reference number?")]
    [Required(ErrorMessage = "Select yes if you have a teacher reference number")]
    public bool? HaveTrn { get; set; }

    [BindProperty]
    [Display(Name = "Teacher reference number")]
    [Required(ErrorMessage = "Enter your teacher reference number")]
    [RegularExpression(@"\A\D*(\d{1}\D*){7}\D*\Z", ErrorMessage = "Your teacher reference number should contain 7 digits")]
    public string? Trn { get; set; }

    public void OnGet()
    {
        HaveTrn = coordinator.State.HaveTrn;
        Trn = coordinator.State.Trn;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HaveTrn != true)
        {
            ModelState.Remove(nameof(Trn));
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        coordinator.UpdateState(state => state.SetTrn(HaveTrn!.Value, Trn));

        return await coordinator.AdvanceToAsync(async links =>
            await coordinator.TryMatchToTeachingRecordAsync() ? links.Found() :
            links.NotFound());
    }
}

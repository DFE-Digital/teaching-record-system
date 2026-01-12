using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class NationalInsuranceNumberModel(SignInJourneyCoordinator coordinator) : PageModel
{
    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "Do you have a National Insurance number?")]
    [Required(ErrorMessage = "Select yes if you have a National Insurance number")]
    public bool? HaveNationalInsuranceNumber { get; set; }

    [BindProperty]
    [Display(Name = "National Insurance number")]
    [Required(ErrorMessage = "Enter a National Insurance number")]
    public string? NationalInsuranceNumber { get; set; }

    public void OnGet()
    {
        HaveNationalInsuranceNumber = coordinator.State.HaveNationalInsuranceNumber;
        NationalInsuranceNumber = coordinator.State.NationalInsuranceNumber;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HaveNationalInsuranceNumber == true)
        {
            if (!string.IsNullOrEmpty(NationalInsuranceNumber) && !Core.NationalInsuranceNumber.TryParse(NationalInsuranceNumber, out _))
            {
                ModelState.AddModelError(nameof(NationalInsuranceNumber), "Enter a National Insurance number in the correct format");
            }
        }
        else
        {
            ModelState.Remove(nameof(NationalInsuranceNumber));
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        coordinator.UpdateState(state => state.SetNationalInsuranceNumber(HaveNationalInsuranceNumber!.Value, NationalInsuranceNumber));

        return await coordinator.AdvanceToAsync(async links =>
            await coordinator.TryMatchToTeachingRecordAsync() ? links.Found() :
            FromCheckAnswers == true ? links.CheckAnswers() :
            links.Trn());
    }
}

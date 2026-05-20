using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class NationalInsuranceNumberModel(SignInJourneyCoordinator coordinator) : PageModel
{
    private readonly InlineValidator<NationalInsuranceNumberModel> _validator = new()
    {
        v => v.RuleFor(m => m.HaveNationalInsuranceNumber)
            .NotNull()
            .WithMessage("Select yes if you have a National Insurance number"),
        v => v.RuleFor(m => m.NationalInsuranceNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Enter a National Insurance number")
            .Must(nino => Core.NationalInsuranceNumber.TryParse(nino, out _))
            .WithMessage("Enter a National Insurance number in the correct format")
            .When(m => m.HaveNationalInsuranceNumber == true)
    };

    [BindProperty]
    public bool? HaveNationalInsuranceNumber { get; set; }

    [BindProperty]
    public string? NationalInsuranceNumber { get; set; }

    public void OnGet()
    {
        HaveNationalInsuranceNumber = coordinator.State.HaveNationalInsuranceNumber;
        NationalInsuranceNumber = coordinator.State.NationalInsuranceNumber;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        coordinator.UpdateState(state => state.SetNationalInsuranceNumber(HaveNationalInsuranceNumber!.Value, NationalInsuranceNumber));

        return await coordinator.TryMatchToTeachingRecordAsync() ?? coordinator.AdvanceTo(links => links.Trn());
    }
}

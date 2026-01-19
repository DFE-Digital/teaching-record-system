using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class TrnModel(SignInJourneyCoordinator coordinator) : PageModel
{
    private readonly InlineValidator<TrnModel> _validator = new()
    {
        v => v.RuleFor(m => m.HaveTrn)
            .NotNull()
            .WithMessage("Select yes if you have a teacher reference number"),
        v => v.RuleFor(m => m.Trn)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Enter your teacher reference number")
            .Matches(@"\A\D*(\d{1}\D*){7}\D*\Z")
            .WithMessage("Your teacher reference number should contain 7 digits")
            .Must(trn => !System.Text.RegularExpressions.Regex.IsMatch(trn!, @"^\D*0{7}\D*$"))
            .WithMessage("Enter a valid teacher reference number")
            .When(m => m.HaveTrn == true)
    };

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    public bool? HaveTrn { get; set; }

    [BindProperty]
    public string? Trn { get; set; }

    public void OnGet()
    {
        HaveTrn = coordinator.State.HaveTrn;
        Trn = coordinator.State.Trn;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        coordinator.UpdateState(state => state.SetTrn(HaveTrn!.Value, Trn));

        return await coordinator.AdvanceToAsync(async links =>
            await coordinator.TryMatchToTeachingRecordAsync() ? links.Found() :
            links.NotFound());
    }
}

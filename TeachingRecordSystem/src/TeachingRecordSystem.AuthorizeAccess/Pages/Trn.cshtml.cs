using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public partial class TrnModel(SignInJourneyCoordinator coordinator) : PageModel
{
    [GeneratedRegex(@"\A\D*0{7}\D*\Z")]
    private static partial Regex TrnPattern();

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
            .Must(trn => !TrnPattern().IsMatch(trn))
            .WithMessage("Enter a valid teacher reference number")
            .When(m => m.HaveTrn == true)
    };

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

        return
            HaveTrn is false ? coordinator.AdvanceTo(links => links.NoTrn()) :
            await coordinator.TryMatchToTeachingRecordAsync() ??
            (coordinator.State.IdentityVerified
                ? coordinator.AdvanceTo(links => links.NotFound())
                : coordinator.AdvanceTo(links => links.ProofOfIdentity()));
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class InductionExemptionModel(
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceUploadManager)
    : AddRoutePostStatusPageModel(AddRoutePage.InductionExemption, linkGenerator, referenceDataCache, evidenceUploadManager)
{
    private readonly InlineValidator<InductionExemptionModel> _validator = new()
    {
        v => v.RuleFor(m => m.IsExemptFromInduction)
            .NotNull().WithMessage("Select yes if this route provides an induction exemption")
    };

    [BindProperty]
    public bool? IsExemptFromInduction { get; set; }

    public IActionResult OnGet()
    {
        IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _validator.ValidateAndThrow(this);

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.IsExemptFromInduction = IsExemptFromInduction;
        });

        return await ContinueAsync();
    }

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (RouteType.InductionExemptionRequired == FieldRequirement.NotApplicable
            || (RouteType.InductionExemptionReason is not null && RouteType.InductionExemptionReason.RouteImplicitExemption))
        {
            context.Result = BadRequest();
            return;
        }
    }
}

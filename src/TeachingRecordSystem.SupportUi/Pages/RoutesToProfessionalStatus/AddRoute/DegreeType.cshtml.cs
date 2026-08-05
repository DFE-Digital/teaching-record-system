using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus)]
public class DegreeTypeModel(AddRouteJourneyCoordinator journey, ReferenceDataCache referenceDataCache) : PageModel
{
    private readonly InlineValidator<DegreeTypeModel> _validator = new()
    {
        v => v.RuleFor(m => m.DegreeTypeId)
            .NotNull().WithMessage("Select a degree type")
            .When(m => m.DegreeTypeRequired)
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    public DegreeType[] DegreeTypes { get; set; } = [];

    public bool DegreeTypeRequired { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public Guid? DegreeTypeId { get; set; }

    public void OnGet()
    {
        DegreeTypeId = journey.State.DegreeTypeId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        return await journey.AnswerAndAdvanceAsync(AddRoutePage.DegreeType, state => state.DegreeTypeId = DegreeTypeId);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        DegreeTypeRequired = await journey.QuestionIsMandatoryAsync(AddRoutePage.DegreeType);
        DegreeTypes = await referenceDataCache.GetDegreeTypesAsync();

        BackLink = journey.GetBackLink();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

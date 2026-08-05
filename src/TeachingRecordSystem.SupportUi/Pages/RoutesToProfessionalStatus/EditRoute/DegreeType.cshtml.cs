using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus)]
public class DegreeTypeModel(
    EditRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    private readonly InlineValidator<DegreeTypeModel> _validator = new()
    {
        v => v.RuleFor(m => m.DegreeTypeId)
            .NotNull().WithMessage("Select a degree type")
            .When(m => m.DegreeTypeRequired)
    };

    public string PageCaption => journey.PageCaption;

    public string BackLink => journey.GetReturnUrlOrDefault(DetailUrl);

    public DegreeType[] DegreeTypes { get; set; } = [];

    public bool DegreeTypeRequired { get; set; }


    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public Guid? DegreeTypeId { get; set; }


    private string DetailUrl => linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId);

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

        journey.UpdateState(state => state.DegreeTypeId = DegreeTypeId);

        return Redirect(journey.GetReturnUrlOrDefault(DetailUrl));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        DegreeTypeRequired = await journey.QuestionIsMandatoryAsync(EditRoutePage.DegreeType);
        DegreeTypes = await referenceDataCache.GetDegreeTypesAsync();


        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

[Journey(JourneyNames.EditMqSpecialism), StartsJourney]
public class IndexModel(
    EditMqSpecialismJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.Specialism)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Select a specialism")
            .Must((m, specialism) => m.Specialisms!.Any(s => s.Value == specialism)).WithMessage("Select a valid specialism")
    };

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public IReadOnlyCollection<MandatoryQualificationSpecialismInfo>? Specialisms { get; set; }

    public void OnGet()
    {
        Specialism = journey.State.Specialism;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.Mqs.EditMq.Specialism.Reason(journey.InstanceId),
            state => state.Specialism = Specialism);
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var qualificationInfo = context.HttpContext.GetCurrentMandatoryQualificationFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        var migratedFromDqtWithLegacySpecialism = qualificationInfo.MandatoryQualification.DqtSpecialismValue is string dqtSpecialismValue &&
            MandatoryQualificationSpecialismRegistry.GetByDqtValue(dqtSpecialismValue).IsLegacy();

        Specialisms = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: migratedFromDqtWithLegacySpecialism);

        BackLink = journey.GetBackLink() ?? linkGenerator.Persons.PersonDetail.Qualifications(PersonId);

        await next();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq)]
public class SpecialismModel(
    AddMqJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<SpecialismModel> _validator = new()
    {
        v => v.RuleFor(m => m.Specialism)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Select a specialism")
            .Must((m, specialism) => m.Specialisms!.Any(s => s.Value == specialism)).WithMessage("Select a valid specialism")
    };

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

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
            linkGenerator.Mqs.AddMq.StartDate(journey.InstanceId),
            state => state.Specialism = Specialism);
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        Specialisms = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: false);

        PersonName = personInfo.Name;
    }
}

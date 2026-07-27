using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq)]
public class ProviderModel(
    AddMqJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<ProviderModel> _validator = new()
    {
        v => v.RuleFor(m => m.ProviderId)
            .NotNull().WithMessage("Select a training provider")
    };

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public Guid? ProviderId { get; set; }

    public ProviderInfo[]? Providers { get; set; }

    public void OnGet()
    {
        ProviderId = journey.State.ProviderId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.Mqs.AddMq.Specialism(journey.InstanceId),
            state => state.ProviderId = ProviderId);
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        Providers = MandatoryQualificationProvider.All.Select(p => new ProviderInfo(p.MandatoryQualificationProviderId, p.Name)).ToArray();

        BackLink = journey.GetBackLink() ?? linkGenerator.Persons.PersonDetail.Qualifications(PersonId);
    }

    public record ProviderInfo(Guid MandatoryQualificationProviderId, string Name);
}

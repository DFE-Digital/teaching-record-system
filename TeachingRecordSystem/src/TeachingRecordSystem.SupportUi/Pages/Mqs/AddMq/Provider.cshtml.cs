using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class ProviderModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<ProviderModel> _validator = new()
    {
        v => v.RuleFor(m => m.ProviderId)
            .NotNull().WithMessage("Select a training provider")
    };

    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public Guid? ProviderId { get; set; }

    public ProviderInfo[]? Providers { get; set; }

    public void OnGet()
    {
        ProviderId = JourneyInstance!.State.ProviderId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _validator.ValidateAndThrow(this);

        await JourneyInstance!.UpdateStateAsync(state => state.ProviderId = ProviderId);

        return Redirect(FromCheckAnswers ?
            linkGenerator.Mqs.AddMq.CheckAnswers(PersonId, JourneyInstance.InstanceId) :
            linkGenerator.Mqs.AddMq.Specialism(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        Providers = MandatoryQualificationProvider.All.Select(p => new ProviderInfo(p.MandatoryQualificationProviderId, p.Name)).ToArray();
    }

    public record ProviderInfo(Guid MandatoryQualificationProviderId, string Name);
}

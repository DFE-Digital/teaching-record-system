using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

[Journey(JourneyNames.DeleteMq), ActivatesJourney, RequireJourneyInstance]
public class IndexModel : PageModel
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly ReferenceDataCache _referenceDataCache;
    private readonly TrsLinkGenerator _linkGenerator;

    public IndexModel(
        ICrmQueryDispatcher crmQueryDispatcher,
        ReferenceDataCache referenceDataCache,
        TrsLinkGenerator linkGenerator)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _referenceDataCache = referenceDataCache;
        _linkGenerator = linkGenerator;
    }

    public JourneyInstance<DeleteMqState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? TrainingProvider { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [BindProperty]
    [Display(Name = "Reason for deleting")]
    public MqDeletionReasonOption? DeletionReason { get; set; }

    [BindProperty]
    [Display(Name = "More detail about the reason for deleting")]
    public string? DeletionReasonDetail { get; set; }

    public void OnGet()
    {
        DeletionReason ??= JourneyInstance!.State.DeletionReason;
        DeletionReasonDetail ??= JourneyInstance?.State.DeletionReasonDetail;
    }

    public async Task<IActionResult> OnPost()
    {
        if (DeletionReason is null)
        {
            ModelState.AddModelError(nameof(DeletionReason), "Select a reason for deleting");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.DeletionReason = DeletionReason;
            state.DeletionReasonDetail = DeletionReasonDetail;
        });

        return Redirect(_linkGenerator.MqDeleteConfirm(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var qualification = await _crmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(QualificationId));
        if (qualification is null || qualification.dfeta_Type != dfeta_qualification_dfeta_Type.MandatoryQualification)
        {
            context.Result = NotFound();
            return;
        }

        await JourneyInstance!.State.EnsureInitialized(_crmQueryDispatcher, _referenceDataCache, qualification);

        PersonId = JourneyInstance!.State.PersonId;
        PersonName = JourneyInstance!.State.PersonName;
        TrainingProvider = JourneyInstance!.State.MqEstablishment;
        Specialism = JourneyInstance!.State.Specialism;
        Status = JourneyInstance!.State.Status;
        StartDate = JourneyInstance!.State.StartDate;
        EndDate = JourneyInstance!.State.EndDate;

        await next();
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

[Journey(JourneyNames.DeleteRouteToProfessionalStatus), RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class ReasonModel(SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }
    public JourneyInstance<DeleteRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    public ChangeReasonOption? ChangeReason { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you want to add more information about why you\u2019re deleting this route")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [MaxLength(UiDefaults.ReasonDetailsMaxCharacterCount, ErrorMessage = $"Additional detail {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}")]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public string NextPage => linkGenerator.RoutesToProfessionalStatus.DeleteRoute.CheckAnswers(QualificationId, JourneyInstance!.InstanceId);

    public string BackLink => FromCheckAnswers == true
        ? linkGenerator.RoutesToProfessionalStatus.DeleteRoute.CheckAnswers(QualificationId, JourneyInstance!.InstanceId)
        : linkGenerator.Persons.PersonDetail.Qualifications(PersonId);

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        return next();
    }

    public void OnGet()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        HasAdditionalReasonDetail = JourneyInstance.State.ChangeReasonDetail.HasAdditionalReasonDetail;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail.ChangeReasonDetail;
        Evidence = JourneyInstance.State.ChangeReasonDetail.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HasAdditionalReasonDetail == true && ChangeReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ChangeReasonDetail), "Enter additional detail");
        }

        await evidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.ChangeReasonDetail.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail.ChangeReasonDetail = ChangeReasonDetail;
            state.ChangeReasonDetail.Evidence = Evidence;
        });

        return Redirect(NextPage);
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.ChangeReasonDetail.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }
}

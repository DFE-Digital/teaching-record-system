using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class ReasonModel(TrsLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController) : PageModel
{
    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you editing this route?")]
    public ChangeReasonOption? ChangeReason { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to provide more information?")]
    [Required(ErrorMessage = "Select yes if you want to add more information about why you’re editing this route")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Enter details about this change")]
    [MaxLength(UiDefaults.DetailMaxCharacterCount, ErrorMessage = $"Additional detail {UiDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public string NextPage => linkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId);

    public string BackLink => FromCheckAnswers == true
        ? linkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId)
        : linkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId);

    public string PageCaption => $"Edit route - {PersonName}";

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        JourneyInstance!.State.EnsureInitialized(context.HttpContext.GetCurrentProfessionalStatusFeature().RouteToProfessionalStatus);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        return next();
    }

    public void OnGet()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        HasAdditionalReasonDetail = JourneyInstance!.State.ChangeReasonDetail.HasAdditionalReasonDetail;
        ChangeReasonDetail = JourneyInstance?.State.ChangeReasonDetail.ChangeReasonDetail;
        Evidence = JourneyInstance!.State.ChangeReasonDetail.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HasAdditionalReasonDetail == true && ChangeReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ChangeReasonDetail), "Enter additional detail");
        }

        await evidenceController.ValidateAndUploadAsync(Evidence, ModelState);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.ChangeReasonDetail.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail.ChangeReasonDetail = ChangeReasonDetail;
            state.ChangeReasonDetail.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail.Evidence = Evidence;
        });

        return Redirect(NextPage);
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }
}

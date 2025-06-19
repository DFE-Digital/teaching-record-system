using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.ValidationAttributes;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
[Authorize(Policy = AuthorizationPolicies.RoutesEdit)]
public class AgeRangeSpecialismModel(
    TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public string? PersonName { get; set; }

    public Guid PersonId { get; set; }

    [BindProperty]
    [AgeRangeRequiredValidation("Select an age range")]
    [Display(Name = "Edit age range specialism")]
    public AgeRange TrainingAgeSpecialism { get; set; } = new();

    public void OnGet()
    {
        TrainingAgeSpecialism.AgeRangeFrom = JourneyInstance!.State.TrainingAgeSpecialismRangeFrom;
        TrainingAgeSpecialism.AgeRangeTo = JourneyInstance!.State.TrainingAgeSpecialismRangeTo;
        TrainingAgeSpecialism.AgeRangeType = JourneyInstance!.State.TrainingAgeSpecialismType;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }
        await JourneyInstance!.UpdateStateAsync(s =>
            {
                s.TrainingAgeSpecialismRangeFrom = TrainingAgeSpecialism!.AgeRangeFrom;
                s.TrainingAgeSpecialismRangeTo = TrainingAgeSpecialism!.AgeRangeTo;
                s.TrainingAgeSpecialismType = TrainingAgeSpecialism!.AgeRangeType;
            });
        return Redirect(FromCheckAnswers ?
            linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class AgeRangeSpecialismModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : PageModel
{
    public const string PageHeading = "Edit age range specialism";

    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public string? PersonName { get; set; }

    public Guid PersonId { get; set; }

    public RouteToProfessionalStatusType Route { get; set; } = null!;

    public RouteToProfessionalStatusStatus Status { get; set; }

    [BindProperty]
    [Display(Name = PageHeading)]
    public AgeRange TrainingAgeSpecialism { get; set; } = new();

    public bool AgeRangeSpecialismRequired => QuestionDriverHelper.FieldRequired(Route.TrainingAgeSpecialismTypeRequired, Status.GetAgeSpecialismRequirement())
        == FieldRequirement.Mandatory;

    public void OnGet()
    {
        TrainingAgeSpecialism.AgeRangeFrom = JourneyInstance!.State.TrainingAgeSpecialismRangeFrom;
        TrainingAgeSpecialism.AgeRangeTo = JourneyInstance!.State.TrainingAgeSpecialismRangeTo;
        TrainingAgeSpecialism.AgeRangeType = JourneyInstance!.State.TrainingAgeSpecialismType;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (AgeRangeSpecialismRequired && TrainingAgeSpecialism.AgeRangeType is null)
        {
            ModelState.AddModelError($"{nameof(TrainingAgeSpecialism)}.{nameof(TrainingAgeSpecialism.AgeRangeType)}", "Enter an age range specialism");
        }

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
            linkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        Route = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId);
        Status = JourneyInstance!.State.Status;

        await next();
    }
}

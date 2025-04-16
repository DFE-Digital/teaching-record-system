using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class EndDateModel : AddRouteCommonPageModel
{
    [BindProperty]
    [DateInput(ErrorMessagePrefix = "End date")]
    [Required(ErrorMessage = "Enter an end date")]
    [Display(Name = "Enter the route end date")]
    public DateOnly? TrainingEndDate { get; set; }

    public EndDateModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : base(linkGenerator, referenceDataCache)
    {
    }

    public string BackLink => FromCheckAnswers ?
        _linkGenerator.RouteAddCheckAnswers(PersonId, JourneyInstance!.InstanceId) :
        _linkGenerator.RouteAddPage(PreviousPage(AddRoutePage.EndDate) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public void OnGet()
    {
        TrainingEndDate = JourneyInstance!.State.TrainingEndDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (TrainingEndDate.HasValue && JourneyInstance!.State.TrainingStartDate is DateOnly startDate && startDate >= TrainingEndDate)
        {
            ModelState.AddModelError(nameof(TrainingEndDate), "End date must be after start date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s => s.TrainingEndDate = TrainingEndDate);

        return Redirect(FromCheckAnswers ?
            _linkGenerator.RouteAddCheckAnswers(PersonId, JourneyInstance.InstanceId) :
            _linkGenerator.RouteAddPage(NextPage(AddRoutePage.EndDate) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;
    }
}

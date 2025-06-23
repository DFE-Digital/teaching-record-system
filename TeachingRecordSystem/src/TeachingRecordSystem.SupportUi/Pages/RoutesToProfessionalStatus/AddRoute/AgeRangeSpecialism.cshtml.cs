using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class AgeRangeSpecialismModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    protected override RoutePage CurrentPage => RoutePage.AgeRangeSpecialism;

    public string BackLink => PreviousPage;

    [BindProperty]
    [Display(Name = "Add age range specialism")]
    public AgeRange TrainingAgeSpecialism { get; set; } = new();

    public void OnGet()
    {
        TrainingAgeSpecialism = new AgeRange
        {
            AgeRangeFrom = JourneyInstance!.State.TrainingAgeSpecialismRangeFrom,
            AgeRangeTo = JourneyInstance!.State.TrainingAgeSpecialismRangeTo,
            AgeRangeType = JourneyInstance!.State.TrainingAgeSpecialismType
        };
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

        return Redirect(NextPage);
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;
    }
}

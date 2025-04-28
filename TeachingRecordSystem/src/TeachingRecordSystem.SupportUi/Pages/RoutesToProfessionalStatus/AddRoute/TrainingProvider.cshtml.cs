<<<<<<< HEAD
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
=======
using Microsoft.AspNetCore.Mvc;
>>>>>>> 173fe6bb0332cd59af21f7d5d76b245dfdc1ac04

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class TrainingProviderModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string BackLink => FromCheckAnswers ?
        _linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        _linkGenerator.RouteAddPage(PreviousPage(AddRoutePage.TrainingProvider) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public TrainingProvider[] TrainingProviders { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Select a training provider")]
    [Display(Name = "Enter the training provider for this route")]
    public Guid? TrainingProviderId { get; set; }

    public void OnGet()
    {
        TrainingProviderId = JourneyInstance!.State.TrainingProviderId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s => s.TrainingProviderId = TrainingProviderId);

        return Redirect(FromCheckAnswers ?
            _linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
            _linkGenerator.RouteAddPage(NextPage(AddRoutePage.TrainingProvider) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        TrainingProviders = await _referenceDataCache.GetTrainingProvidersAsync();
        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public IActionResult OnPost()
    {
        return Redirect(FromCheckAnswers ?
            _linkGenerator.RouteCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
            _linkGenerator.RouteAddPage(NextPage(AddRoutePage.TrainingProvider) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }

}

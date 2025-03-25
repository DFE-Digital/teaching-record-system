using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class SubjectSpecialismsModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public string? PersonName { get; set; }

    public TrainingSubject[] Subjects { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Enter a subject")]
    [Display(Name = "Enter the subject they specialise in teaching")]
    public Guid? SubjectId1 { get; set; }
    [BindProperty]
    [Display(Name = "Second subject (optional)")]
    public Guid? SubjectId2 { get; set; }
    [BindProperty]
    [Display(Name = "Third subject (optional)")]
    public Guid? SubjectId3 { get; set; }

    public void OnGet()
    {
        SubjectId1 = JourneyInstance!.State.TrainingSubjectIds?.ElementAtOrDefault(0);
        SubjectId2 = JourneyInstance!.State.TrainingSubjectIds?.ElementAtOrDefault(1);
        SubjectId3 = JourneyInstance!.State.TrainingSubjectIds?.ElementAtOrDefault(2);
    }

    public IActionResult OnPostCancel()
    {
        return Redirect(linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        Guid[] subjects = new Guid?[] { SubjectId1, SubjectId2, SubjectId3 }.Where(s => s.HasValue).Select(s => s!.Value).ToArray();

        await JourneyInstance!.UpdateStateAsync(s => s.TrainingSubjectIds = subjects);

        return Redirect(FromCheckAnswers ?
            linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
            linkGenerator.RouteDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        Subjects = await referenceDataCache.GetTrainingSubjectsAsync();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

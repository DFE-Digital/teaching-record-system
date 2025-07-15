using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class SubjectSpecialismsModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : EditRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string PageTitle = "Edit subject specialisms";
    public string PageHeading = "Enter the subject they specialise in teaching";

    public DisplayInfo[] Subjects { get; set; } = [];

    public bool SubjectSpecialismRequired => QuestionDriverHelper.FieldRequired(Route!.TrainingSubjectsRequired, Status.GetSubjectsRequirement())
        == FieldRequirement.Mandatory;

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (SubjectSpecialismRequired && SubjectId1 is null && SubjectId2 is null && SubjectId3 is null)
        {
            ModelState.AddModelError(nameof(SubjectId1), "Enter a subject");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        Guid[] subjects = new Guid?[] { SubjectId1, SubjectId2, SubjectId3 }.Where(s => s.HasValue).Select(s => s!.Value).ToArray();

        await JourneyInstance!.UpdateStateAsync(s => s.TrainingSubjectIds = subjects);

        return Redirect(FromCheckAnswers ?
            LinkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
            LinkGenerator.RouteEditDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        Subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync())
            .Select(s => new DisplayInfo()
            {
                Id = s.TrainingSubjectId,
                DisplayName = $"{s.Reference} - {s.Name}"
            })
            .ToArray();
    }
}

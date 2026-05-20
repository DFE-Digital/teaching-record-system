using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class SubjectSpecialismsModel(
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceController)
    : EditRouteCommonPageModel(linkGenerator, referenceDataCache, evidenceController)
{
    public DisplayInfo[] Subjects { get; set; } = [];

    [BindProperty]
    public Guid? SubjectId1 { get; set; }

    [BindProperty]
    public Guid? SubjectId2 { get; set; }

    [BindProperty]
    public Guid? SubjectId3 { get; set; }

    public bool SubjectSpecialismRequired => QuestionDriverHelper.FieldRequired(RouteType!.TrainingSubjectsRequired, Status.GetSubjectsRequirement())
        == FieldRequirement.Mandatory;

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
            LinkGenerator.RoutesToProfessionalStatus.EditRoute.CheckAnswers(QualificationId, JourneyInstance.InstanceId) :
            LinkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(QualificationId, JourneyInstance.InstanceId));
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

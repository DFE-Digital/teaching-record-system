using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class SubjectSpecialismsModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    protected override RoutePage CurrentPage => RoutePage.SubjectSpecialisms;

    public string BackLink => PreviousPageUrl;

    public DisplayInfo[] Subjects { get; set; } = [];

    public string PageHeading => "Enter the subject they specialise in teaching" + (!SubjectSpecialismRequired ? " (optional)" : "");
    public bool SubjectSpecialismRequired => QuestionDriverHelper.FieldRequired(Route.TrainingSubjectsRequired, Status.GetSubjectsRequirement())
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
        var trainingSubjectIds = JourneyInstance!.State.NewRouteToProfessionalStatusId != null
            ? JourneyInstance!.State.NewTrainingSubjectIds
            : JourneyInstance!.State.TrainingSubjectIds;
        SubjectId1 = trainingSubjectIds.ElementAtOrDefault(0);
        SubjectId2 = trainingSubjectIds.ElementAtOrDefault(1);
        SubjectId3 = trainingSubjectIds.ElementAtOrDefault(2);
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

        await JourneyInstance!.UpdateStateAsync(s =>
        {
            if (JourneyInstance!.State.NewRouteToProfessionalStatusId == null)
            {
                s.Begin();
            }

            s.NewTrainingSubjectIds = subjects;
        });

        return await ContinueAsync();
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        Subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync())
            .Select(s => new DisplayInfo()
            {
                Id = s.TrainingSubjectId,
                DisplayName = $"{s.Reference} - {s.Name}"
            })
            .ToArray();
    }
}

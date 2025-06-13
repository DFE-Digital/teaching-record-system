using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class SubjectSpecialismsModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string BackLink => FromCheckAnswers ?
        LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        LinkGenerator.RouteAddPage(PreviousPage(AddRoutePage.SubjectSpecialisms) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public DisplayInfo[] Subjects { get; set; } = [];

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        Guid[] subjects = new Guid?[] { SubjectId1, SubjectId2, SubjectId3 }.Where(s => s.HasValue).Select(s => s!.Value).ToArray();

        await JourneyInstance!.UpdateStateAsync(s => s.TrainingSubjectIds = subjects);

        return Redirect(FromCheckAnswers ?
            LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
            LinkGenerator.RouteAddPage(NextPage(AddRoutePage.SubjectSpecialisms) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        Subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync(activeOnly: true))
            .Select(s => new DisplayInfo()
            {
                Id = s.TrainingSubjectId,
                DisplayName = $"{s.Reference} - {s.Name}"
            })
            .ToArray();
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

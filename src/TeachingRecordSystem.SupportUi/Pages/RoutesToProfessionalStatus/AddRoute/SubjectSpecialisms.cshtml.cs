using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus)]
public class SubjectSpecialismsModel(AddRouteJourneyCoordinator journey, ReferenceDataCache referenceDataCache) : PageModel
{
    private readonly InlineValidator<SubjectSpecialismsModel> _validator = new()
    {
        v => v.RuleFor(m => m.SubjectId1)
            .Must((m, _) => m.SubjectId1 is not null || m.SubjectId2 is not null || m.SubjectId3 is not null)
                .WithMessage("Enter a subject")
            .When(m => m.SubjectSpecialismRequired)
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    public DisplayInfo[] Subjects { get; set; } = [];

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public Guid? SubjectId1 { get; set; }
    [BindProperty]
    public Guid? SubjectId2 { get; set; }
    [BindProperty]
    public Guid? SubjectId3 { get; set; }

    public bool SubjectSpecialismRequired { get; set; }

    public void OnGet()
    {
        SubjectId1 = journey.State.TrainingSubjectIds.ElementAtOrDefault(0);
        SubjectId2 = journey.State.TrainingSubjectIds.ElementAtOrDefault(1);
        SubjectId3 = journey.State.TrainingSubjectIds.ElementAtOrDefault(2);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        var subjects = new Guid?[] { SubjectId1, SubjectId2, SubjectId3 }.Where(s => s.HasValue).Select(s => s!.Value).ToArray();

        return await journey.AnswerAndAdvanceAsync(AddRoutePage.SubjectSpecialisms, state => state.TrainingSubjectIds = subjects);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        SubjectSpecialismRequired = await journey.QuestionIsMandatoryAsync(AddRoutePage.SubjectSpecialisms);
        Subjects = (await referenceDataCache.GetTrainingSubjectsAsync())
            .Select(s => new DisplayInfo()
            {
                Id = s.TrainingSubjectId,
                DisplayName = $"{s.Reference} - {s.Name}"
            })
            .ToArray();

        BackLink = journey.GetBackLink();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

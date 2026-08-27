using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class QtsDetailsModel(
    SignInJourneyCoordinator coordinator,
    ReferenceDataCache referenceDataCache,
    TimeProvider timeProvider) : PageModel
{
    [BindProperty]
    public string? YearQtsReceived { get; set; }

    [BindProperty]
    public Guid? TrainingProviderId { get; set; }

    [BindProperty]
    public Guid? SubjectId { get; set; }

    [BindProperty]
    public bool Skip { get; set; }

    public TrainingProvider[] TrainingProviders { get; set; } = [];

    public TrainingSubject[] Subjects { get; set; } = [];

    public void OnGet()
    {
        YearQtsReceived = coordinator.State.YearQtsReceived;
        TrainingProviderId = coordinator.State.QtsTrainingProviderId;
        SubjectId = coordinator.State.QtsSubjectId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Skip)
        {
            return coordinator.AdvanceTo(links => links.CheckAnswers());
        }

        await this.ThrowIfInvalidAsync(CreateValidator());


        coordinator.UpdateState(state => state.SetQtsDetails(
            YearQtsReceived,
            TrainingProviderId,
            SubjectId));

        return coordinator.AdvanceTo(links => links.CheckAnswers());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        TrainingProviders = await referenceDataCache.GetTrainingProvidersAsync();
        Subjects = await referenceDataCache.GetTrainingSubjectsAsync();

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    private InlineValidator<QtsDetailsModel> CreateValidator() =>
        new()
        {
            v => v.RuleFor(m => m.YearQtsReceived)
                .Cascade(CascadeMode.Stop)
                .Matches(@"\A\d{4}\z")
                .WithMessage("Year QTS was received must be 4 digits")
                .Must(year => int.TryParse(year, out var qtsYear) && qtsYear <= timeProvider.GetUtcNow().Year)
                .WithMessage("Year QTS was received cannot be in the future")
                .When(m => !string.IsNullOrWhiteSpace(m.YearQtsReceived)),
            v => v.RuleFor(m => m.TrainingProviderId)
                .Must(id => id is null || TrainingProviders.Any(trainingProvider => trainingProvider.TrainingProviderId == id.Value))
                .WithMessage("Select a training provider"),
            v => v.RuleFor(m => m.SubjectId)
                .Must(id => id is null || Subjects.Any(subject => subject.TrainingSubjectId == id.Value))
                .WithMessage("Select a subject")
        };
}

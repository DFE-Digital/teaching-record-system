using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus)]
public class CheckAnswersModel(
    AddRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    RoutesToProfessionalStatusService routesToProfessionalStatusService) : PageModel
{
    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    // The URL a change link brings the user back to once they've answered the question again.
    public string ReturnUrl { get; set; } = null!;

    public RouteToProfessionalStatusType RouteType { get; set; } = null!;

    public RouteToProfessionalStatusStatus Status => journey.Status;

    public DateOnly? TrainingStartDate { get; set; }

    public DateOnly? TrainingEndDate { get; set; }

    public DateOnly? HoldsFrom { get; set; }

    public bool? IsExemptFromInduction { get; set; }

    public bool HasImplicitExemption { get; set; }

    public string? TrainingProvider { get; set; }

    public string? DegreeType { get; set; }

    public string? TrainingCountry { get; set; }

    public TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; set; }

    public int? TrainingAgeSpecialismRangeFrom { get; set; }

    public int? TrainingAgeSpecialismRangeTo { get; set; }

    public string? TrainingAgeSpecialismRange =>
        TrainingAgeSpecialismRangeFrom is not null && TrainingAgeSpecialismRangeTo is not null
            ? $"From {TrainingAgeSpecialismRangeFrom} to {TrainingAgeSpecialismRangeTo}"
            : null;

    public string[]? TrainingSubjects { get; set; }

    public ChangeReasonOption? ChangeReason { get; set; }

    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();

    [BindProperty]
    public bool Cancel { get; set; }

    public FieldRequirement StartDateRequired => QuestionDriverHelper.FieldRequired(RouteType.TrainingStartDateRequired, Status.GetStartDateRequirement());
    public FieldRequirement EndDateRequired => QuestionDriverHelper.FieldRequired(RouteType.TrainingEndDateRequired, Status.GetEndDateRequirement());
    public FieldRequirement HoldsFromRequired => QuestionDriverHelper.FieldRequired(RouteType.HoldsFromRequired, Status.GetHoldsFromDateRequirement());
    public FieldRequirement InductionExemptionRequired => QuestionDriverHelper.FieldRequired(RouteType.InductionExemptionRequired, Status.GetInductionExemptionRequirement());
    public FieldRequirement TrainingProviderRequired => QuestionDriverHelper.FieldRequired(RouteType.TrainingProviderRequired, Status.GetTrainingProviderRequirement());
    public FieldRequirement DegreeTypeRequired => QuestionDriverHelper.FieldRequired(RouteType.DegreeTypeRequired, Status.GetDegreeTypeRequirement());
    public FieldRequirement TrainingCountryRequired => QuestionDriverHelper.FieldRequired(RouteType.TrainingCountryRequired, Status.GetCountryRequirement());
    public FieldRequirement AgeSpecialismRequired => QuestionDriverHelper.FieldRequired(RouteType.TrainingAgeSpecialismTypeRequired, Status.GetAgeSpecialismRequirement());
    public FieldRequirement TrainingSubjectsRequired => QuestionDriverHelper.FieldRequired(RouteType.TrainingSubjectsRequired, Status.GetSubjectsRequirement());

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        var state = journey.State;

        await routesToProfessionalStatusService.CreateRouteToProfessionalStatusAsync(
            new CreateRouteToProfessionalStatusOptions
            {
                PersonId = journey.PersonId,
                RouteToProfessionalStatusTypeId = RouteType.RouteToProfessionalStatusTypeId,
                Status = journey.Status,
                CreatedBy = User.GetUserId(),
                HoldsFrom = state.HoldsFrom,
                TrainingStartDate = state.TrainingStartDate,
                TrainingEndDate = state.TrainingEndDate,
                TrainingSubjectIds = state.TrainingSubjectIds,
                TrainingAgeSpecialismType = state.TrainingAgeSpecialismType,
                TrainingAgeSpecialismRangeFrom = state.TrainingAgeSpecialismRangeFrom,
                TrainingAgeSpecialismRangeTo = state.TrainingAgeSpecialismRangeTo,
                TrainingCountryId = state.TrainingCountryId,
                TrainingProviderId = state.TrainingProviderId,
                DegreeTypeId = state.DegreeTypeId,
                IsExemptFromInduction = state.IsExemptFromInduction,
                ChangeReason = state.ChangeReason?.GetDisplayName(),
                ChangeReasonDetail = state.ChangeReasonDetail.ChangeReasonDetail,
                EvidenceFile = state.ChangeReasonDetail.Evidence.UploadedEvidenceFile?.ToEventModel(),
                AdditionalInformation = state.ChangeReasonDetail.AdditionalInformation
            });

        journey.DeleteInstance();

        TempData.SetFlashNotificationBanner("Route to professional status added");

        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(journey.PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ReturnUrl = linkGenerator.RoutesToProfessionalStatus.AddRoute.CheckAnswers(journey.InstanceId);

        var state = journey.State;

        RouteType = await journey.GetRouteTypeAsync();
        HasImplicitExemption = RouteType.InductionExemptionReason?.RouteImplicitExemption ?? false;
        TrainingStartDate = state.TrainingStartDate;
        TrainingEndDate = state.TrainingEndDate;
        HoldsFrom = state.HoldsFrom;
        IsExemptFromInduction = state.IsExemptFromInduction;
        TrainingAgeSpecialismType = state.TrainingAgeSpecialismType;
        TrainingAgeSpecialismRangeFrom = state.TrainingAgeSpecialismRangeFrom;
        TrainingAgeSpecialismRangeTo = state.TrainingAgeSpecialismRangeTo;
        ChangeReason = state.ChangeReason;
        ChangeReasonDetail = state.ChangeReasonDetail;

        TrainingProvider = state.TrainingProviderId is Guid trainingProviderId
            ? (await referenceDataCache.GetTrainingProviderByIdAsync(trainingProviderId))?.Name
            : null;
        TrainingCountry = state.TrainingCountryId is string trainingCountryId
            ? (await referenceDataCache.GetTrainingCountryByIdAsync(trainingCountryId))?.Name
            : null;
        DegreeType = state.DegreeTypeId is Guid degreeTypeId
            ? (await referenceDataCache.GetDegreeTypeByIdAsync(degreeTypeId))?.Name
            : null;
        TrainingSubjects = await SubjectDisplayHelper.GetFormattedSubjectNamesAsync(state.TrainingSubjectIds, referenceDataCache);

        BackLink = journey.GetBackLink();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

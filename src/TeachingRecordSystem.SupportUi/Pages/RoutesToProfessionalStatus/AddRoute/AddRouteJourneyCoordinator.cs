using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[JourneyCoordinator(JourneyNames.AddRouteToProfessionalStatus, routeValueKeys: ["personId"])]
public class AddRouteJourneyCoordinator(
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceUploadManager) : JourneyCoordinator<AddRouteState>
{
    public Guid PersonId => HttpContext.GetCurrentPersonFeature().PersonId;

    public RouteToProfessionalStatusStatus Status => State.Status!.Value;

    public string PageCaption => $"Add a route - {HttpContext.GetCurrentPersonFeature().Name}";

    // The questions asked about a route of this type and status, in the order they're asked.
    public static AddRoutePage[] GetPagesForRoute(RouteToProfessionalStatusType route, RouteToProfessionalStatusStatus status)
    {
        return Enum.GetValues<AddRoutePage>()
            .OrderBy(p => p)
            .Where(p => PageApplies(p, route, status))
            .ToArray();
    }

    public static FieldRequirement GetFieldRequirementForPage(
        AddRoutePage page,
        RouteToProfessionalStatusType routeType,
        RouteToProfessionalStatusStatus status)
    {
        return page switch
        {
            AddRoutePage.Route => FieldRequirement.Mandatory,
            AddRoutePage.Status => FieldRequirement.Mandatory,
            AddRoutePage.StartAndEndDate => QuestionDriverHelper.FieldRequired(routeType.TrainingEndDateRequired, status.GetEndDateRequirement()),
            AddRoutePage.HoldsFrom => QuestionDriverHelper.FieldRequired(routeType.HoldsFromRequired, status.GetHoldsFromDateRequirement()),
            AddRoutePage.InductionExemption => QuestionDriverHelper.FieldRequired(routeType.InductionExemptionRequired, status.GetInductionExemptionRequirement()),
            AddRoutePage.TrainingProvider => QuestionDriverHelper.FieldRequired(routeType.TrainingProviderRequired, status.GetTrainingProviderRequirement()),
            AddRoutePage.DegreeType => QuestionDriverHelper.FieldRequired(routeType.DegreeTypeRequired, status.GetDegreeTypeRequirement()),
            AddRoutePage.Country => QuestionDriverHelper.FieldRequired(routeType.TrainingCountryRequired, status.GetCountryRequirement()),
            AddRoutePage.AgeRangeSpecialism => QuestionDriverHelper.FieldRequired(routeType.TrainingAgeSpecialismTypeRequired, status.GetAgeSpecialismRequirement()),
            AddRoutePage.SubjectSpecialisms => QuestionDriverHelper.FieldRequired(routeType.TrainingSubjectsRequired, status.GetSubjectsRequirement()),
            AddRoutePage.ChangeReason => FieldRequirement.Mandatory,
            AddRoutePage.CheckAnswers => FieldRequirement.Mandatory,
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
    }

    public async Task<bool> QuestionIsMandatoryAsync(AddRoutePage page) =>
        GetFieldRequirementForPage(page, await GetRouteTypeAsync(), Status) == FieldRequirement.Mandatory;

    public override AddRouteState GetStartingState() => new();

    public Task<RouteToProfessionalStatusType> GetRouteTypeAsync() =>
        referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(State.RouteToProfessionalStatusId!.Value);

    // Records the answer to currentPage and moves on to the next question.
    //
    // Answering the route or its status decides which questions the journey asks, so it can add and
    // remove questions rather than simply moving the user on. When that happens the steps after this
    // one no longer describe the journey, so they're dropped and the user works forward through the
    // questions again — the path only ever holds the questions they've been through. Otherwise the
    // path is left alone, so a change made from check answers takes the user straight back there with
    // its other change links still working.
    public async Task<AdvanceToResult> AnswerAndAdvanceAsync(AddRoutePage currentPage, Action<AddRouteState> updateState)
    {
        var pagesBefore = await GetPagesAsync();
        UpdateState(updateState);
        var pagesAfter = await GetPagesAsync();

        var nextPageUrl = GetPageUrl(pagesAfter.SkipWhile(p => p != currentPage).ElementAt(1));

        return pagesAfter.SequenceEqual(pagesBefore)
            ? AdvanceTo(nextPageUrl)
            : AdvanceTo(nextPageUrl, new PushStepOptions { SetAsLastStep = true });
    }

    // Returns the URL to send the user back to.
    public async Task<string> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(State.ChangeReasonDetail.Evidence.UploadedEvidenceFile);
        DeleteInstance();
        return linkGenerator.Persons.PersonDetail.Qualifications(PersonId);
    }

    private async Task<AddRoutePage[]> GetPagesAsync()
    {
        if (State.RouteToProfessionalStatusId is null)
        {
            return [AddRoutePage.Route];
        }

        if (State.Status is null)
        {
            return [AddRoutePage.Route, AddRoutePage.Status];
        }

        return GetPagesForRoute(await GetRouteTypeAsync(), Status);
    }

    private static bool PageApplies(
        AddRoutePage page,
        RouteToProfessionalStatusType routeType,
        RouteToProfessionalStatusStatus status)
    {
        // A route that comes with an induction exemption of its own doesn't ask whether it provides one.
        if (page == AddRoutePage.InductionExemption && routeType.InductionExemptionReason?.RouteImplicitExemption == true)
        {
            return false;
        }

        return GetFieldRequirementForPage(page, routeType, status) != FieldRequirement.NotApplicable;
    }

    private string GetPageUrl(AddRoutePage page, string? returnUrl = null) =>
        linkGenerator.RoutesToProfessionalStatus.AddRoute.Page(page, InstanceId, returnUrl);
}

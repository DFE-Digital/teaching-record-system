using Microsoft.AspNetCore.Http.Extensions;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[JourneyCoordinator(JourneyNames.EditRouteToProfessionalStatus, routeValueKeys: ["qualificationId"])]
public class EditRouteJourneyCoordinator(
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceUploadManager) : JourneyCoordinator<EditRouteState>
{
    public Guid QualificationId => Guid.Parse(InstanceId.RouteValues["qualificationId"]!.ToString()!);

    public Guid PersonId => HttpContext.GetCurrentPersonFeature().PersonId;

    public string PageCaption => $"Edit route - {HttpContext.GetCurrentPersonFeature().Name}";

    // The status the answers are being collected for: while a route is being completed the new status
    // is buffered until its questions have all been answered.
    public RouteToProfessionalStatusStatus Status => State.EditStatusState?.Status ?? State.Status;

    public bool IsCompletingRoute => State.EditStatusState is not null;

    public override async Task<EditRouteState> GetStartingStateAsync()
    {
        var route = HttpContext.GetCurrentProfessionalStatusFeature().RouteToProfessionalStatus;
        var routeType = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(route.RouteToProfessionalStatusTypeId);

        var state = new EditRouteState
        {
            FromInductions = HttpContext.Request.Query["fromInductions"].ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase),
            QualificationType = route.QualificationType,
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            CurrentStatus = route.Status,
            Status = route.Status,
            HoldsFrom = route.HoldsFrom,
            TrainingStartDate = route.TrainingStartDate,
            TrainingEndDate = route.TrainingEndDate,
            TrainingSubjectIds = route.TrainingSubjectIds,
            TrainingAgeSpecialismType = route.TrainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom = route.TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = route.TrainingAgeSpecialismRangeTo,
            TrainingCountryId = route.TrainingCountryId,
            TrainingProviderId = route.TrainingProviderId,
            IsExemptFromInduction = route.ExemptFromInduction,
            DegreeTypeId = route.DegreeTypeId
        };

        state.AvailablePages = Enum.GetValues<EditRoutePage>()
            .OrderBy(p => p)
            .Where(p => p.AppliesToRoute(routeType, state.Status))
            .ToArray();

        return state;
    }

    // The status decides which questions the route asks, so answering it changes what's reachable.
    public async Task RefreshAvailablePagesAsync()
    {
        var pages = await GetAvailablePagesAsync();
        UpdateState(state => state.AvailablePages = pages);
    }

    public Task<RouteToProfessionalStatusType> GetRouteTypeAsync() =>
        referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(State.RouteToProfessionalStatusId);

    // The detail page offers the questions that apply to the route, so they're all reachable from it
    // rather than being worked through in order — but only those: a question the route doesn't ask
    // isn't a step the user can reach. The path still records where the user has been, which is what
    // gives each question its back link.
    public override bool StepIsValid(JourneyPathStep step) =>
        base.StepIsValid(step) || IsAvailableQuestion(step);

    // The journey filter asks for the current step before validating it, and the base implementation
    // only knows about steps in the path, so a question reached from the detail page has to be
    // recognised here too.
    public override JourneyPathStep? GetCurrentStep()
    {
        if (base.GetCurrentStep() is JourneyPathStep stepInPath)
        {
            return stepInPath;
        }

        var step = CreateStepFromUrl(HttpContext.Request.GetEncodedPathAndQuery());
        return IsAvailableQuestion(step) ? step : null;
    }

    public async Task<bool> QuestionIsMandatoryAsync(EditRoutePage page) =>
        page.GetFieldRequirement(await GetRouteTypeAsync(), Status) == FieldRequirement.Mandatory;

    // The URL of a question that has to be answered before the change can be made, or null once they've
    // all been answered. The detail page lets the user go straight to check answers, so this is what
    // asks for a reason, and for anything the route needs that it doesn't have yet.
    public async Task<string?> GetUnansweredQuestionUrlAsync(string? returnUrl = null)
    {
        var routeType = await GetRouteTypeAsync();

        foreach (var page in GetAvailablePages(routeType))
        {
            if (page is EditRoutePage.Status or EditRoutePage.CheckAnswers)
            {
                continue;
            }

            if (page.GetFieldRequirement(routeType, Status) == FieldRequirement.Mandatory && !QuestionIsAnswered(page))
            {
                return linkGenerator.RoutesToProfessionalStatus.EditRoute.Page(page, InstanceId, returnUrl);
            }
        }

        return null;
    }

    // The questions this route and status ask, in the order the detail page lists them.
    public async Task<EditRoutePage[]> GetAvailablePagesAsync() => GetAvailablePages(await GetRouteTypeAsync());

    // Moving a route to 'holds' asks for the answers that go with it — when it was first held, and
    // whether it carries an induction exemption. They're buffered in EditStatusState so that a journey
    // abandoned part way through doesn't leave the route half changed.
    public async Task<EditRoutePage[]> GetCompletingRoutePagesAsync()
    {
        var routeType = await GetRouteTypeAsync();

        return EditRoutePage.InductionExemption.AppliesToRoute(routeType, Status)
            ? [EditRoutePage.Status, EditRoutePage.HoldsFrom, EditRoutePage.InductionExemption]
            : [EditRoutePage.Status, EditRoutePage.HoldsFrom];
    }

    public async Task<bool> IsLastCompletingRoutePageAsync(EditRoutePage page) =>
        (await GetCompletingRoutePagesAsync())[^1] == page;

    // Applies the buffered answers to the route and ends the completing-a-route sequence.
    public void CompleteRoute(Action<EditRouteState> updateState)
    {
        UpdateState(state =>
        {
            updateState(state);
            state.Status = state.EditStatusState!.Status;
            state.EditStatusState = null;
        });
    }

    // Returns the URL to send the user back to.
    public async Task<string> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(State.ChangeReasonDetail.Evidence.UploadedEvidenceFile);
        DeleteInstance();
        return linkGenerator.Persons.PersonDetail.Qualifications(PersonId);
    }

    // Kept on the state so that step validation, which the journey filter runs synchronously, doesn't
    // have to reach for the route's reference data.
    private bool IsAvailableQuestion(JourneyPathStep step) =>
        AvailableQuestionStepUrls.Contains(step.NormalizedUrl);

    private string[] AvailableQuestionStepUrls =>
        State.AvailablePages
            .Where(p => p is not EditRoutePage.CheckAnswers)
            .Select(p => CreateStepFromUrl(linkGenerator.RoutesToProfessionalStatus.EditRoute.Page(p, InstanceId)).NormalizedUrl)
            .ToArray();

    private EditRoutePage[] GetAvailablePages(RouteToProfessionalStatusType routeType) =>
        Enum.GetValues<EditRoutePage>()
            .OrderBy(p => p)
            .Where(p => p.AppliesToRoute(routeType, Status))
            .ToArray();

    private bool QuestionIsAnswered(EditRoutePage page) =>
        page switch
        {
            EditRoutePage.StartAndEndDate => State.TrainingStartDate is not null && State.TrainingEndDate is not null,
            EditRoutePage.HoldsFrom => State.HoldsFrom is not null,
            EditRoutePage.InductionExemption => State.IsExemptFromInduction is not null,
            EditRoutePage.TrainingProvider => State.TrainingProviderId is not null,
            EditRoutePage.DegreeType => State.DegreeTypeId is not null,
            EditRoutePage.Country => State.TrainingCountryId is not null,
            EditRoutePage.AgeRangeSpecialism => State.TrainingAgeSpecialismType switch
            {
                null => false,
                TrainingAgeSpecialismType.Range =>
                    State.TrainingAgeSpecialismRangeFrom is not null && State.TrainingAgeSpecialismRangeTo is not null,
                _ => true
            },
            EditRoutePage.SubjectSpecialisms => State.TrainingSubjectIds.Length != 0,
            EditRoutePage.ChangeReason => State.ChangeReason is not null && State.ChangeReasonDetail.IsComplete,
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
}

using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public abstract class AddRouteTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    // No further detail and no evidence — enough for the check answers page to treat the reason
    // question as answered.
    protected static ChangeReasonDetailsState CreateChangeReasonDetail() =>
        new()
        {
            ProvideAdditionalInformation = ProvideMoreInformationOption.No,
            Evidence = new() { UploadEvidence = false }
        };

    protected async Task<AddRouteJourneyCoordinator> CreateJourneyInstanceAsync(Guid personId, AddRouteState? state = null)
    {
        state ??= new AddRouteState();

        // Seed the steps of a journey that's been worked through to check answers — which questions it
        // asks depends on the route and the status it's in — so that back links and step validation
        // behave as they do in the real journey.
        var pathUrls = (await GetPagesAsync(state)).Select(p => GetPageUrl(p, personId)).ToArray();

        return await JourneyHelper.CreateInstanceAsync<AddRouteJourneyCoordinator>(
            JourneyNames.AddRouteToProfessionalStatus,
            new RouteValueDictionary { ["personId"] = personId },
            _ => Task.FromResult<object>(state),
            pathUrls: pathUrls,
            coordinatorFactory: CreateJourneyCoordinator<AddRouteJourneyCoordinator>);
    }

    protected AddRouteState? GetJourneyInstanceState(AddRouteJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (AddRouteState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    protected static string GetPageUrl(AddRoutePage page, Guid personId) =>
        $"{GetPagePath(page)}?personId={personId}";

    // The URL a change link on the check answers page brings the user back to once they've answered
    // the question again.
    protected static string GetCheckAnswersReturnUrl(AddRouteJourneyCoordinator journeyInstance, Guid personId) =>
        $"{GetPageUrl(AddRoutePage.CheckAnswers, personId)}&{journeyInstance.GetUniqueIdQueryParameter()}";

    private static string GetPagePath(AddRoutePage page) =>
        page switch
        {
            AddRoutePage.Route => "/routes/add/route",
            AddRoutePage.Status => "/routes/add/status",
            AddRoutePage.StartAndEndDate => "/routes/add/start-and-end-date",
            AddRoutePage.HoldsFrom => "/routes/add/holds-from",
            AddRoutePage.InductionExemption => "/routes/add/induction-exemption",
            AddRoutePage.TrainingProvider => "/routes/add/training-provider",
            AddRoutePage.DegreeType => "/routes/add/degree-type",
            AddRoutePage.Country => "/routes/add/country",
            AddRoutePage.AgeRangeSpecialism => "/routes/add/age-range",
            AddRoutePage.SubjectSpecialisms => "/routes/add/subjects",
            AddRoutePage.ChangeReason => "/routes/add/reason",
            AddRoutePage.CheckAnswers => "/routes/add/check-answers",
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };

    private async Task<AddRoutePage[]> GetPagesAsync(AddRouteState state)
    {
        if (state.RouteToProfessionalStatusId is null)
        {
            return [AddRoutePage.Route];
        }

        if (state.Status is null)
        {
            return [AddRoutePage.Route, AddRoutePage.Status];
        }

        var routeType = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(state.RouteToProfessionalStatusId.Value);
        return AddRouteJourneyCoordinator.GetPagesForRoute(routeType, state.Status.Value);
    }
}

using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public abstract class EditRouteTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    // A complete reason with detail and no evidence — enough for check answers to treat the reason
    // question as answered.
    protected static ChangeReasonDetailsState CreateChangeReasonDetail() =>
        new()
        {
            ProvideAdditionalInformation = ProvideMoreInformationOption.Yes,
            ChangeReasonDetail = "Some free text reason detail",
            AdditionalInformation = "some additional information",
            Evidence = new() { UploadEvidence = false }
        };

    protected async Task<EditRouteJourneyCoordinator> CreateJourneyInstanceAsync(Guid qualificationId, EditRouteState? state = null)
    {
        state ??= new EditRouteState();

        if (state.RouteToProfessionalStatusId == Guid.Empty)
        {
            // The coordinator seeds the journey from the route being edited; a test that doesn't care
            // about the answers gets the same starting point.
            var qualification = await WithDbContextAsync(dbContext => dbContext.RouteToProfessionalStatuses
                .SingleAsync(r => r.QualificationId == qualificationId));

            state = CreateStateFromRoute(qualification, state);
        }

        state.AvailablePages = await GetAvailablePagesAsync(state);

        // The detail page is where the journey starts and where the questions return to; check answers
        // is the step it advances to. The questions themselves are reachable from the detail page rather
        // than being steps in the path.
        return await JourneyHelper.CreateInstanceAsync<EditRouteJourneyCoordinator>(
            JourneyNames.EditRouteToProfessionalStatus,
            new RouteValueDictionary { ["qualificationId"] = qualificationId },
            _ => Task.FromResult<object>(state),
            pathUrls:
            [
                GetPageUrl(qualificationId, "detail"),
                GetPageUrl(qualificationId, "check-answers")
            ],
            coordinatorFactory: CreateJourneyCoordinator<EditRouteJourneyCoordinator>);
    }

    // The answers the journey starts with, as the coordinator seeds them from the route.
    protected static EditRouteState CreateStateFromRoute(RouteToProfessionalStatus qualification, EditRouteState? state = null)
    {
        ArgumentNullException.ThrowIfNull(qualification);

        state ??= new EditRouteState();

        state.QualificationType = qualification.QualificationType;
        state.RouteToProfessionalStatusId = qualification.RouteToProfessionalStatusTypeId;
        state.CurrentStatus = qualification.Status;
        state.Status = qualification.Status;
        state.HoldsFrom = qualification.HoldsFrom;
        state.TrainingStartDate = qualification.TrainingStartDate;
        state.TrainingEndDate = qualification.TrainingEndDate;
        state.TrainingSubjectIds = qualification.TrainingSubjectIds;
        state.TrainingAgeSpecialismType = qualification.TrainingAgeSpecialismType;
        state.TrainingAgeSpecialismRangeFrom = qualification.TrainingAgeSpecialismRangeFrom;
        state.TrainingAgeSpecialismRangeTo = qualification.TrainingAgeSpecialismRangeTo;
        state.TrainingCountryId = qualification.TrainingCountryId;
        state.TrainingProviderId = qualification.TrainingProviderId;
        state.IsExemptFromInduction = qualification.ExemptFromInduction;
        state.DegreeTypeId = qualification.DegreeTypeId;

        return state;
    }

    private async Task<EditRoutePage[]> GetAvailablePagesAsync(EditRouteState state)
    {
        var routeType = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(state.RouteToProfessionalStatusId);
        var status = state.EditStatusState?.Status ?? state.Status;

        return Enum.GetValues<EditRoutePage>()
            .OrderBy(p => p)
            .Where(p => p.AppliesToRoute(routeType, status))
            .ToArray();
    }

    protected EditRouteState? GetJourneyInstanceState(EditRouteJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (EditRouteState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    protected static string GetPageUrl(Guid qualificationId, string page) =>
        $"/routes/{qualificationId}/edit/{page}";

    // The URL a change link on the check answers page brings the user back to once they've answered the
    // question again.
    protected static string GetCheckAnswersReturnUrl(EditRouteJourneyCoordinator journeyInstance, Guid qualificationId) =>
        $"{GetPageUrl(qualificationId, "check-answers")}?{journeyInstance.GetUniqueIdQueryParameter()}";

    protected static string GetPagePath(AddRoutePage page) =>
        page switch
        {
            AddRoutePage.Status => "status",
            AddRoutePage.StartAndEndDate => "start-and-end-date",
            AddRoutePage.HoldsFrom => "holds-from",
            AddRoutePage.InductionExemption => "induction-exemption",
            AddRoutePage.TrainingProvider => "training-provider",
            AddRoutePage.DegreeType => "degree-type",
            AddRoutePage.Country => "country",
            AddRoutePage.AgeRangeSpecialism => "age-range",
            AddRoutePage.SubjectSpecialisms => "subjects",
            AddRoutePage.ChangeReason => "reason",
            AddRoutePage.CheckAnswers => "check-answers",
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
}

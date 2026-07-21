using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.ReopenAlert;

public abstract class ReopenAlertTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<ReopenAlertJourneyCoordinator> CreateEmptyJourneyInstanceAsync(Guid alertId) =>
        CreateJourneyInstanceAsync(alertId, new());

    protected Task<ReopenAlertJourneyCoordinator> CreateJourneyInstanceForAllStepsCompletedAsync(Alert alert, bool populateOptional = true) =>
        CreateJourneyInstanceAsync(alert.AlertId, new ReopenAlertState
        {
            ChangeReason = ReopenAlertReasonOption.ClosedInError,
            HasAdditionalReasonDetail = populateOptional ? true : false,
            ChangeReasonDetail = populateOptional ? "More details" : null,
            Evidence = new()
            {
                UploadEvidence = populateOptional ? true : false,
                UploadedEvidenceFile = populateOptional ? new()
                {
                    FileId = Guid.NewGuid(),
                    FileName = "evidence.jpeg",
                    FileSizeDescription = "5MB"
                } : null
            }
        });

    protected Task<ReopenAlertJourneyCoordinator> CreateJourneyInstanceForCompletedStepAsync(string step, Alert alert) =>
        step switch
        {
            JourneySteps.New =>
                CreateEmptyJourneyInstanceAsync(alert.AlertId),
            JourneySteps.Index or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    protected ReopenAlertState? GetJourneyInstanceState(ReopenAlertJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (ReopenAlertState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    private Task<ReopenAlertJourneyCoordinator> CreateJourneyInstanceAsync(Guid alertId, ReopenAlertState state) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<ReopenAlertJourneyCoordinator>(
            JourneyNames.ReopenAlert,
            new RouteValueDictionary { ["alertId"] = alertId },
            _ => Task.FromResult<object>(state),
            pathUrls:
            [
                $"/alerts/{alertId}/reopen",
                $"/alerts/{alertId}/reopen/check-answers",
            ]);

    public static class JourneySteps
    {
        public const string New = nameof(New);
        public const string Index = nameof(Index);
        public const string CheckAnswers = nameof(CheckAnswers);
    }
}

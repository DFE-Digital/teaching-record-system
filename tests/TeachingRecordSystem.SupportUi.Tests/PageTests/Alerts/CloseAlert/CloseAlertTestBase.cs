using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.CloseAlert;

public abstract class CloseAlertTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<CloseAlertJourneyCoordinator> CreateEmptyJourneyInstanceAsync(Guid alertId) =>
        CreateJourneyInstanceAsync(alertId, new());

    protected Task<CloseAlertJourneyCoordinator> CreateJourneyInstanceForAllStepsCompletedAsync(Alert alert, bool populateOptional = true) =>
        CreateJourneyInstanceAsync(alert.AlertId, new CloseAlertState
        {
            EndDate = alert.StartDate!.Value.AddDays(2),
            ChangeReason = CloseAlertReasonOption.AnotherReason,
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

    protected Task<CloseAlertJourneyCoordinator> CreateJourneyInstanceForCompletedStepAsync(string step, Alert alert) =>
        step switch
        {
            JourneySteps.New =>
                CreateEmptyJourneyInstanceAsync(alert.AlertId),
            JourneySteps.Index =>
                CreateJourneyInstanceAsync(alert.AlertId, new CloseAlertState
                {
                    EndDate = alert.StartDate!.Value.AddDays(2)
                }),
            JourneySteps.Reason or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    protected CloseAlertState? GetJourneyInstanceState(CloseAlertJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (CloseAlertState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    private Task<CloseAlertJourneyCoordinator> CreateJourneyInstanceAsync(Guid alertId, CloseAlertState state) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<CloseAlertJourneyCoordinator>(
            JourneyNames.CloseAlert,
            new RouteValueDictionary { ["alertId"] = alertId },
            _ => Task.FromResult<object>(state),
            pathUrls:
            [
                $"/alerts/{alertId}/close",
                $"/alerts/{alertId}/close/reason",
                $"/alerts/{alertId}/close/check-answers",
            ]);

    public static class JourneySteps
    {
        public const string New = nameof(New);
        public const string Index = nameof(Index);
        public const string Reason = nameof(Reason);
        public const string CheckAnswers = nameof(CheckAnswers);
    }
}

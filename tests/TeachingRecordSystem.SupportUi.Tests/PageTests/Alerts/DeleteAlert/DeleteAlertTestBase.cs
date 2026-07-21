using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.DeleteAlert;

public class DeleteAlertTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<DeleteAlertJourneyCoordinator> CreateEmptyJourneyInstanceAsync(Guid alertId) =>
        CreateJourneyInstanceAsync(alertId, new());

    protected Task<DeleteAlertJourneyCoordinator> CreateJourneyInstanceForAllStepsCompletedAsync(Alert alert, bool populateOptional = true, DeleteAlertReasonOption deleteReason = DeleteAlertReasonOption.AnotherReason, bool provideAdditionalInformation = false) =>
        CreateJourneyInstanceAsync(alert.AlertId, new DeleteAlertState
        {
            DeleteReason = deleteReason,
            DeleteReasonDetail = deleteReason == DeleteAlertReasonOption.AnotherReason ? "More details" : null,
            Evidence = new()
            {
                UploadEvidence = populateOptional ? true : false,
                UploadedEvidenceFile = populateOptional ? new()
                {
                    FileId = Guid.NewGuid(),
                    FileName = "evidence.jpeg",
                    FileSizeDescription = "5MB"
                } : null
            },
            ProvideAdditionalInformation = provideAdditionalInformation,
            AdditionalInformation = provideAdditionalInformation == true ? "Some additional information" : null
        });

    protected Task<DeleteAlertJourneyCoordinator> CreateJourneyInstanceForCompletedStepAsync(string step, Alert alert) =>
        step switch
        {
            JourneySteps.New =>
                CreateEmptyJourneyInstanceAsync(alert.AlertId),
            JourneySteps.Index or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true, provideAdditionalInformation: true),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    protected DeleteAlertState? GetJourneyInstanceState(DeleteAlertJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (DeleteAlertState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    private Task<DeleteAlertJourneyCoordinator> CreateJourneyInstanceAsync(Guid alertId, DeleteAlertState state) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<DeleteAlertJourneyCoordinator>(
            JourneyNames.DeleteAlert,
            new RouteValueDictionary { ["alertId"] = alertId },
            _ => Task.FromResult<object>(state),
            pathUrls:
            [
                $"/alerts/{alertId}/delete",
                $"/alerts/{alertId}/delete/check-answers",
            ]);

    public static class JourneySteps
    {
        public const string New = nameof(New);
        public const string Index = nameof(Index);
        public const string CheckAnswers = nameof(CheckAnswers);
    }
}

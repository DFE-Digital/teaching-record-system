using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.StartDate;

public abstract class StartDateTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<EditAlertStartDateJourneyCoordinator> CreateEmptyJourneyInstanceAsync(Guid alertId) =>
        CreateJourneyInstanceAsync(alertId, new());

    protected Task<EditAlertStartDateJourneyCoordinator> CreateJourneyInstanceForAllStepsCompletedAsync(Alert alert, bool populateOptional = true) =>
        CreateJourneyInstanceAsync(alert.AlertId, new EditAlertStartDateState
        {
            CurrentStartDate = alert.StartDate,
            StartDate = alert.StartDate!.Value.AddDays(1),
            ChangeReason = AlertChangeStartDateReasonOption.AnotherReason,
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

    protected Task<EditAlertStartDateJourneyCoordinator> CreateJourneyInstanceForCompletedStepAsync(string step, Alert alert) =>
        step switch
        {
            JourneySteps.New =>
                CreateJourneyInstanceAsync(alert.AlertId, new EditAlertStartDateState
                {
                    CurrentStartDate = alert.StartDate
                }),
            JourneySteps.Index =>
                CreateJourneyInstanceAsync(alert.AlertId, new EditAlertStartDateState
                {
                    CurrentStartDate = alert.StartDate,
                    StartDate = alert.StartDate!.Value.AddDays(1)
                }),
            JourneySteps.Reason or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    protected EditAlertStartDateState? GetJourneyInstanceState(EditAlertStartDateJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (EditAlertStartDateState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    private Task<EditAlertStartDateJourneyCoordinator> CreateJourneyInstanceAsync(Guid alertId, EditAlertStartDateState state) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<EditAlertStartDateJourneyCoordinator>(
            JourneyNames.EditAlertStartDate,
            new RouteValueDictionary { ["alertId"] = alertId },
            _ => Task.FromResult<object>(state),
            pathUrls:
            [
                $"/alerts/{alertId}/start-date",
                $"/alerts/{alertId}/start-date/reason",
                $"/alerts/{alertId}/start-date/check-answers",
            ]);

    public static class JourneySteps
    {
        public const string New = nameof(New);
        public const string Index = nameof(Index);
        public const string Reason = nameof(Reason);
        public const string CheckAnswers = nameof(CheckAnswers);
    }
}

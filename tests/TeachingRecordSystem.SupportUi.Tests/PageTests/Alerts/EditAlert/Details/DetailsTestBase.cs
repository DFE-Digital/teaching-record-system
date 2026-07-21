using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Details;

public abstract class DetailsTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<EditAlertDetailsJourneyCoordinator> CreateEmptyJourneyInstanceAsync(Guid alertId) =>
        CreateJourneyInstanceAsync(alertId, new());

    protected Task<EditAlertDetailsJourneyCoordinator> CreateJourneyInstanceForAllStepsCompletedAsync(Alert alert, bool populateOptional = true, bool provideAdditionalInformation = false, AlertChangeDetailsReasonOption changeReasonOption = AlertChangeDetailsReasonOption.AnotherReason) =>
        CreateJourneyInstanceAsync(alert.AlertId, new EditAlertDetailsState
        {
            CurrentDetails = alert.Details,
            Details = "New details",
            ChangeReason = changeReasonOption,
            ProvideAdditionalInformation = provideAdditionalInformation,
            AdditionalInformation = provideAdditionalInformation ? "some additional information" : null,
            ChangeReasonDetail = changeReasonOption == AlertChangeDetailsReasonOption.AnotherReason ? "More details" : null,
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

    protected Task<EditAlertDetailsJourneyCoordinator> CreateJourneyInstanceForCompletedStepAsync(string step, Alert alert) =>
        step switch
        {
            JourneySteps.New =>
                CreateEmptyJourneyInstanceAsync(alert.AlertId),
            JourneySteps.Index =>
                CreateJourneyInstanceAsync(alert.AlertId, new EditAlertDetailsState
                {
                    CurrentDetails = alert.Details,
                    Details = "New details"
                }),
            JourneySteps.Reason or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    protected EditAlertDetailsState? GetJourneyInstanceState(EditAlertDetailsJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (EditAlertDetailsState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    private Task<EditAlertDetailsJourneyCoordinator> CreateJourneyInstanceAsync(Guid alertId, EditAlertDetailsState state) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<EditAlertDetailsJourneyCoordinator>(
            JourneyNames.EditAlertDetails,
            new RouteValueDictionary { ["alertId"] = alertId },
            _ => Task.FromResult<object>(state),
            pathUrls:
            [
                $"/alerts/{alertId}/details",
                $"/alerts/{alertId}/details/reason",
                $"/alerts/{alertId}/details/check-answers",
            ]);

    public static class JourneySteps
    {
        public const string New = nameof(New);
        public const string Index = nameof(Index);
        public const string Reason = nameof(Reason);
        public const string CheckAnswers = nameof(CheckAnswers);
    }
}

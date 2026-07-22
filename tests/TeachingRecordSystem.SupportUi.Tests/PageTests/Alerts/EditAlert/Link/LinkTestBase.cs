using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Link;

public abstract class LinkTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<EditAlertLinkJourneyCoordinator> CreateEmptyJourneyInstanceAsync(Guid alertId) =>
        CreateJourneyInstanceAsync(alertId, new());

    protected Task<EditAlertLinkJourneyCoordinator> CreateJourneyInstanceForAllStepsCompletedAsync(Alert alert, bool populateOptional = true) =>
        CreateJourneyInstanceAsync(alert.AlertId, new EditAlertLinkState
        {
            CurrentLink = populateOptional ? null : alert.ExternalLink,
            AddLink = populateOptional,
            Link = populateOptional ? "https://www.example.com" : null,
            ChangeReason = AlertChangeLinkReasonOption.AnotherReason,
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

    protected Task<EditAlertLinkJourneyCoordinator> CreateJourneyInstanceForCompletedStepAsync(string step, Alert alert, bool populateOptional = true) =>
        step switch
        {
            JourneySteps.New =>
                CreateEmptyJourneyInstanceAsync(alert.AlertId),
            JourneySteps.Index =>
                CreateJourneyInstanceAsync(alert.AlertId, new EditAlertLinkState
                {
                    CurrentLink = populateOptional ? null : alert.ExternalLink,
                    AddLink = populateOptional,
                    Link = populateOptional ? "https://www.example.com" : null
                }),
            JourneySteps.Reason or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: populateOptional),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    protected EditAlertLinkState? GetJourneyInstanceState(EditAlertLinkJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (EditAlertLinkState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    private Task<EditAlertLinkJourneyCoordinator> CreateJourneyInstanceAsync(Guid alertId, EditAlertLinkState state) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<EditAlertLinkJourneyCoordinator>(
            JourneyNames.EditAlertLink,
            new RouteValueDictionary { ["alertId"] = alertId },
            _ => Task.FromResult<object>(state),
            pathUrls:
            [
                $"/alerts/{alertId}/link",
                $"/alerts/{alertId}/link/reason",
                $"/alerts/{alertId}/link/check-answers",
            ]);

    public static class JourneySteps
    {
        public const string New = nameof(New);
        public const string Index = nameof(Index);
        public const string Reason = nameof(Reason);
        public const string CheckAnswers = nameof(CheckAnswers);
    }
}

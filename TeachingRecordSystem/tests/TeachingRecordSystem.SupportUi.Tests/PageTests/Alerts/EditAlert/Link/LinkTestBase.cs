using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Link;

public abstract class LinkTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<JourneyInstance<EditAlertLinkState>> CreateEmptyJourneyInstanceAsync(Guid alertId) =>
        CreateJourneyInstanceAsync(alertId, new());

    protected Task<JourneyInstance<EditAlertLinkState>> CreateJourneyInstanceForAllStepsCompletedAsync(Alert alert, bool populateOptional = true) =>
        CreateJourneyInstanceAsync(alert.AlertId, new EditAlertLinkState()
        {
            Initialized = true,
            CurrentLink = populateOptional ? null : alert.ExternalLink,
            AddLink = populateOptional,
            Link = populateOptional ? "https://www.example.com" : null,
            ChangeReason = AlertChangeLinkReasonOption.AnotherReason,
            HasAdditionalReasonDetail = populateOptional ? true : false,
            ChangeReasonDetail = populateOptional ? "More details" : null,
            UploadEvidence = populateOptional ? true : false,
            EvidenceFileId = populateOptional ? Guid.NewGuid() : null,
            EvidenceFileName = populateOptional ? "evidence.jpeg" : null,
            EvidenceFileSizeDescription = populateOptional ? "5MB" : null
        });

    protected Task<JourneyInstance<EditAlertLinkState>> CreateJourneyInstanceForCompletedStepAsync(string step, Alert alert, bool populateOptional = true) =>
        step switch
        {
            JourneySteps.New =>
                CreateEmptyJourneyInstanceAsync(alert.AlertId),
            JourneySteps.Index =>
                CreateJourneyInstanceAsync(alert.AlertId, new EditAlertLinkState()
                {
                    Initialized = true,
                    CurrentLink = populateOptional ? null : alert.ExternalLink,
                    AddLink = populateOptional,
                    Link = populateOptional ? "https://www.example.com" : null
                }),
            JourneySteps.Reason or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: populateOptional),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    private Task<JourneyInstance<EditAlertLinkState>> CreateJourneyInstanceAsync(Guid alertId, EditAlertLinkState state) =>
        CreateJourneyInstance(
            JourneyNames.EditAlertLink,
            state,
            new KeyValuePair<string, object>("alertId", alertId));

    public static class JourneySteps
    {
        public const string New = nameof(New);
        public const string Index = nameof(Index);
        public const string Reason = nameof(Reason);
        public const string CheckAnswers = nameof(CheckAnswers);
    }
}

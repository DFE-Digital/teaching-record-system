using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.ReopenAlert;

public abstract class ReopenAlertTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<JourneyInstance<ReopenAlertState>> CreateEmptyJourneyInstanceAsync(Guid alertId) =>
        CreateJourneyInstanceAsync(alertId, new());

    protected Task<JourneyInstance<ReopenAlertState>> CreateJourneyInstanceForAllStepsCompletedAsync(Alert alert, bool populateOptional = true) =>
        CreateJourneyInstanceAsync(alert.AlertId, new ReopenAlertState
        {
            ChangeReason = ReopenAlertReasonOption.ClosedInError,
            HasAdditionalReasonDetail = populateOptional ? true : false,
            ChangeReasonDetail = populateOptional ? "More details" : null,
            UploadEvidence = populateOptional ? true : false,
            EvidenceFileId = populateOptional ? Guid.NewGuid() : null,
            EvidenceFileName = populateOptional ? "evidence.jpeg" : null,
            EvidenceFileSizeDescription = populateOptional ? "5MB" : null
        });

    protected Task<JourneyInstance<ReopenAlertState>> CreateJourneyInstanceForCompletedStepAsync(string step, Alert alert) =>
        step switch
        {
            JourneySteps.New =>
                CreateEmptyJourneyInstanceAsync(alert.AlertId),
            JourneySteps.Index or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    private Task<JourneyInstance<ReopenAlertState>> CreateJourneyInstanceAsync(Guid alertId, ReopenAlertState state) =>
        CreateJourneyInstance(
            JourneyNames.ReopenAlert,
            state,
            new KeyValuePair<string, object>("alertId", alertId));

    public static class JourneySteps
    {
        public const string New = nameof(New);
        public const string Index = nameof(Index);
        public const string CheckAnswers = nameof(CheckAnswers);
    }
}

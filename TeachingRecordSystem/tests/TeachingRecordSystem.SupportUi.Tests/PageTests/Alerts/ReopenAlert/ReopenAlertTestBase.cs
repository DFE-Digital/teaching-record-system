using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.ReopenAlert;

public abstract class ReopenAlertTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<JourneyInstance<ReopenAlertState>> CreateEmptyJourneyInstance(Guid alertId) =>
        CreateJourneyInstance(alertId, new());

    protected Task<JourneyInstance<ReopenAlertState>> CreateJourneyInstanceForAllStepsCompleted(Alert alert, bool populateOptional = true) =>
        CreateJourneyInstance(alert.AlertId, new ReopenAlertState()
        {
            ChangeReason = ReopenAlertReasonOption.ClosedInError,
            HasAdditionalReasonDetail = populateOptional ? true : false,
            ChangeReasonDetail = populateOptional ? "More details" : null,
            UploadEvidence = populateOptional ? true : false,
            EvidenceFileId = populateOptional ? Guid.NewGuid() : null,
            EvidenceFileName = populateOptional ? "evidence.jpeg" : null,
            EvidenceFileSizeDescription = populateOptional ? "5MB" : null
        });

    protected Task<JourneyInstance<ReopenAlertState>> CreateJourneyInstanceForCompletedStep(string step, Alert alert) =>
        step switch
        {
            JourneySteps.New =>
                CreateEmptyJourneyInstance(alert.AlertId),
            JourneySteps.Index or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompleted(alert, populateOptional: true),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    private Task<JourneyInstance<ReopenAlertState>> CreateJourneyInstance(Guid alertId, ReopenAlertState state) =>
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

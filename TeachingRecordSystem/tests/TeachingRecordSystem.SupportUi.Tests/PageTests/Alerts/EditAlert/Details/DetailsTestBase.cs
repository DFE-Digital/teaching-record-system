using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Details;

public abstract class DetailsTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<JourneyInstance<EditAlertDetailsState>> CreateEmptyJourneyInstanceAsync(Guid alertId) =>
        CreateJourneyInstanceAsync(alertId, new());

    protected Task<JourneyInstance<EditAlertDetailsState>> CreateJourneyInstanceForAllStepsCompletedAsync(Alert alert, bool populateOptional = true) =>
        CreateJourneyInstanceAsync(alert.AlertId, new EditAlertDetailsState
        {
            Initialized = true,
            CurrentDetails = alert.Details,
            Details = "New details",
            ChangeReason = AlertChangeDetailsReasonOption.AnotherReason,
            HasAdditionalReasonDetail = populateOptional ? true : false,
            ChangeReasonDetail = populateOptional ? "More details" : null,
            UploadEvidence = populateOptional ? true : false,
            EvidenceFileId = populateOptional ? Guid.NewGuid() : null,
            EvidenceFileName = populateOptional ? "evidence.jpeg" : null,
            EvidenceFileSizeDescription = populateOptional ? "5MB" : null
        });

    protected Task<JourneyInstance<EditAlertDetailsState>> CreateJourneyInstanceForCompletedStepAsync(string step, Alert alert) =>
        step switch
        {
            JourneySteps.New =>
                CreateEmptyJourneyInstanceAsync(alert.AlertId),
            JourneySteps.Index =>
                CreateJourneyInstanceAsync(alert.AlertId, new EditAlertDetailsState
                {
                    Initialized = true,
                    CurrentDetails = alert.Details,
                    Details = "New details"
                }),
            JourneySteps.Reason or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    private Task<JourneyInstance<EditAlertDetailsState>> CreateJourneyInstanceAsync(Guid alertId, EditAlertDetailsState state) =>
        CreateJourneyInstance(
            JourneyNames.EditAlertDetails,
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

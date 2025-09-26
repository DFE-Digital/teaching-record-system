using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.StartDate;

public abstract class StartDateTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<JourneyInstance<EditAlertStartDateState>> CreateEmptyJourneyInstanceAsync(Guid alertId) =>
        CreateJourneyInstanceAsync(alertId, new());

    protected Task<JourneyInstance<EditAlertStartDateState>> CreateJourneyInstanceForAllStepsCompletedAsync(Alert alert, bool populateOptional = true) =>
        CreateJourneyInstanceAsync(alert.AlertId, new EditAlertStartDateState
        {
            Initialized = true,
            CurrentStartDate = alert.StartDate,
            StartDate = alert.StartDate!.Value.AddDays(1),
            ChangeReason = AlertChangeStartDateReasonOption.AnotherReason,
            HasAdditionalReasonDetail = populateOptional ? true : false,
            ChangeReasonDetail = populateOptional ? "More details" : null,
            UploadEvidence = populateOptional ? true : false,
            EvidenceFileId = populateOptional ? Guid.NewGuid() : null,
            EvidenceFileName = populateOptional ? "evidence.jpeg" : null,
            EvidenceFileSizeDescription = populateOptional ? "5MB" : null
        });

    protected Task<JourneyInstance<EditAlertStartDateState>> CreateJourneyInstanceForCompletedStepAsync(string step, Alert alert) =>
        step switch
        {
            JourneySteps.New =>
                CreateJourneyInstanceAsync(alert.AlertId, new EditAlertStartDateState
                {
                    Initialized = true,
                    CurrentStartDate = alert.StartDate
                }),
            JourneySteps.Index =>
                CreateJourneyInstanceAsync(alert.AlertId, new EditAlertStartDateState
                {
                    Initialized = true,
                    CurrentStartDate = alert.StartDate,
                    StartDate = alert.StartDate!.Value.AddDays(1)
                }),
            JourneySteps.Reason or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    private Task<JourneyInstance<EditAlertStartDateState>> CreateJourneyInstanceAsync(Guid alertId, EditAlertStartDateState state) =>
        CreateJourneyInstance(
            JourneyNames.EditAlertStartDate,
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

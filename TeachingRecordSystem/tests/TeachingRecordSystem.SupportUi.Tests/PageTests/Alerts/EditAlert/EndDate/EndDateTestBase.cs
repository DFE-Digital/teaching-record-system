using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.EndDate;

public abstract class EndDateTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<JourneyInstance<EditAlertEndDateState>> CreateEmptyJourneyInstance(Guid alertId) =>
        CreateJourneyInstance(alertId, new());

    protected Task<JourneyInstance<EditAlertEndDateState>> CreateJourneyInstanceForAllStepsCompleted(Alert alert, bool populateOptional = true) =>
        CreateJourneyInstance(alert.AlertId, new EditAlertEndDateState()
        {
            Initialized = true,
            CurrentEndDate = alert.EndDate,
            EndDate = alert.EndDate!.Value.AddDays(-5),
            ChangeReason = AlertChangeEndDateReasonOption.AnotherReason,
            HasAdditionalReasonDetail = populateOptional ? true : false,
            ChangeReasonDetail = populateOptional ? "More details" : null,
            UploadEvidence = populateOptional ? true : false,
            EvidenceFileId = populateOptional ? Guid.NewGuid() : null,
            EvidenceFileName = populateOptional ? "evidence.jpeg" : null,
            EvidenceFileSizeDescription = populateOptional ? "5MB" : null
        });

    protected Task<JourneyInstance<EditAlertEndDateState>> CreateJourneyInstanceForCompletedStep(string step, Alert alert) =>
        step switch
        {
            JourneySteps.New =>
                CreateJourneyInstance(alert.AlertId, new EditAlertEndDateState()
                {
                    Initialized = true,
                    CurrentEndDate = alert.EndDate
                }),
            JourneySteps.Index =>
                CreateJourneyInstance(alert.AlertId, new EditAlertEndDateState()
                {
                    Initialized = true,
                    CurrentEndDate = alert.EndDate,
                    EndDate = alert.EndDate!.Value.AddDays(-5)
                }),
            JourneySteps.Reason or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompleted(alert, populateOptional: true),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    private Task<JourneyInstance<EditAlertEndDateState>> CreateJourneyInstance(Guid alertId, EditAlertEndDateState state) =>
        CreateJourneyInstance(
            JourneyNames.EditAlertEndDate,
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

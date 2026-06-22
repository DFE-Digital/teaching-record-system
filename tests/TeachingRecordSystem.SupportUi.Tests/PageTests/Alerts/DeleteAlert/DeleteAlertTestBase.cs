using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.DeleteAlert;

public class DeleteAlertTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<JourneyInstance<DeleteAlertState>> CreateEmptyJourneyInstanceAsync(Guid alertId) =>
        CreateJourneyInstanceAsync(alertId, new());

    protected Task<JourneyInstance<DeleteAlertState>> CreateJourneyInstanceForAllStepsCompletedAsync(Alert alert, bool populateOptional = true, DeleteAlertReasonOption deleteReason = DeleteAlertReasonOption.AnotherReason, bool provideAdditionalInformation = false) =>
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

    protected Task<JourneyInstance<DeleteAlertState>> CreateJourneyInstanceForCompletedStepAsync(string step, Alert alert) =>
        step switch
        {
            JourneySteps.New =>
                CreateEmptyJourneyInstanceAsync(alert.AlertId),
            JourneySteps.Index or JourneySteps.CheckAnswers =>
                CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true, provideAdditionalInformation: true),
            _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
        };

    private Task<JourneyInstance<DeleteAlertState>> CreateJourneyInstanceAsync(Guid alertId, DeleteAlertState state) =>
        CreateJourneyInstance(
            JourneyNames.DeleteAlert,
            state,
            new KeyValuePair<string, object>("alertId", alertId));

    public static class JourneySteps
    {
        public const string New = nameof(New);
        public const string Index = nameof(Index);
        public const string CheckAnswers = nameof(CheckAnswers);
    }
}

using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public abstract class AddAlertTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<JourneyInstance<AddAlertState>> CreateEmptyJourneyInstanceAsync(Guid personId) =>
        CreateJourneyInstanceAsync(personId, new());

    protected async Task<JourneyInstance<AddAlertState>> CreateJourneyInstanceForAllStepsCompletedAsync(Guid personId, bool populateOptional = true)
    {
        var alertType = await GetKnownAlertTypeAsync();

        return await CreateJourneyInstanceAsync(personId, new AddAlertState
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Alert Details",
            AddLink = populateOptional,
            Link = populateOptional ? "https://www.example.com" : null,
            StartDate = Clock.Today.AddDays(-30),
            AddReason = AddAlertReasonOption.AnotherReason,
            HasAdditionalReasonDetail = populateOptional ? true : false,
            AddReasonDetail = populateOptional ? "More details" : null,
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
    }

    protected async Task<JourneyInstance<AddAlertState>> CreateJourneyInstanceForCompletedStepAsync(string step, Guid personId, bool populateOptional = true)
    {
        var alertType = await GetKnownAlertTypeAsync();

        return await CreateJourneyInstanceForCompletedStepAsync(step, personId, alertType, populateOptional);
    }

    protected async Task<JourneyInstance<AddAlertState>> CreateJourneyInstanceForCompletedStepAsync(string step, Guid personId, Guid alertTypeId, bool populateOptional = true)
    {
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(alertTypeId);

        return await CreateJourneyInstanceForCompletedStepAsync(step, personId, alertType, populateOptional);
    }

    protected Task<JourneyInstance<AddAlertState>> CreateJourneyInstanceForCompletedStepAsync(string step, Guid personId, AlertType alertType, bool populateOptional = true)
    {
        return
            (step switch
            {
                JourneySteps.New or JourneySteps.Index =>
                    CreateEmptyJourneyInstanceAsync(personId),
                JourneySteps.AlertType =>
                    CreateJourneyInstanceAsync(personId, new AddAlertState
                    {
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name
                    }),
                JourneySteps.Details =>
                    CreateJourneyInstanceAsync(personId, new AddAlertState
                    {
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name,
                        Details = "Alert Details"
                    }),
                JourneySteps.Link =>
                    CreateJourneyInstanceAsync(personId, new AddAlertState
                    {
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name,
                        Details = "Alert Details",
                        AddLink = populateOptional,
                        Link = populateOptional ? "https://www.example.com" : null
                    }),
                JourneySteps.StartDate =>
                    CreateJourneyInstanceAsync(personId, new AddAlertState
                    {
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name,
                        Details = "Alert Details",
                        AddLink = populateOptional,
                        Link = populateOptional ? "https://www.example.com" : null,
                        StartDate = Clock.Today.AddDays(-30)
                    }),
                JourneySteps.Reason or JourneySteps.CheckAnswers =>
                    CreateJourneyInstanceForAllStepsCompletedAsync(personId, populateOptional: true),
                _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
            });
    }

    protected Task<AlertType> GetKnownAlertTypeAsync(bool isDbsAlertType = false) =>
        isDbsAlertType ? TestData.ReferenceDataCache.GetAlertTypeByDqtSanctionCodeAsync("") : TestData.ReferenceDataCache.GetAlertTypeByDqtSanctionCodeAsync("T4");

    private Task<JourneyInstance<AddAlertState>> CreateJourneyInstanceAsync(Guid personId, AddAlertState state) =>
        CreateJourneyInstance(
            JourneyNames.AddAlert,
            state ?? new AddAlertState(),
            new KeyValuePair<string, object>("personId", personId));

    public static class JourneySteps
    {
        public const string New = nameof(New);
        public const string Index = nameof(Index);
        public const string AlertType = nameof(AlertType);
        public const string Details = nameof(Details);
        public const string Link = nameof(Link);
        public const string StartDate = nameof(StartDate);
        public const string Reason = nameof(Reason);
        public const string CheckAnswers = nameof(CheckAnswers);
    }
}

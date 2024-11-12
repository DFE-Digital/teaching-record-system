using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public abstract class AddAlertTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<JourneyInstance<AddAlertState>> CreateEmptyJourneyInstance(Guid personId) =>
        CreateJourneyInstance(personId, new());

    protected async Task<JourneyInstance<AddAlertState>> CreateJourneyInstanceForAllStepsCompleted(Guid personId, bool populateOptional = true)
    {
        var alertType = await GetKnownAlertType();

        return await CreateJourneyInstance(personId, new AddAlertState()
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
            UploadEvidence = populateOptional ? true : false,
            EvidenceFileId = populateOptional ? Guid.NewGuid() : null,
            EvidenceFileName = populateOptional ? "evidence.jpeg" : null,
            EvidenceFileSizeDescription = populateOptional ? "5MB" : null
        });
    }

    protected async Task<JourneyInstance<AddAlertState>> CreateJourneyInstanceForCompletedStep(string step, Guid personId, bool populateOptional = true)
    {
        var alertType = await GetKnownAlertType();

        return await
            (step switch
            {
                JourneySteps.New or JourneySteps.Index =>
                    CreateEmptyJourneyInstance(personId),
                JourneySteps.AlertType =>
                    CreateJourneyInstance(personId, new AddAlertState()
                    {
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name
                    }),
                JourneySteps.Details =>
                    CreateJourneyInstance(personId, new AddAlertState()
                    {
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name,
                        Details = "Alert Details"
                    }),
                JourneySteps.Link =>
                    CreateJourneyInstance(personId, new AddAlertState()
                    {
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name,
                        Details = "Alert Details",
                        AddLink = populateOptional,
                        Link = populateOptional ? "https://www.example.com" : null
                    }),
                JourneySteps.StartDate =>
                    CreateJourneyInstance(personId, new AddAlertState()
                    {
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name,
                        Details = "Alert Details",
                        AddLink = populateOptional,
                        Link = populateOptional ? "https://www.example.com" : null,
                        StartDate = Clock.Today.AddDays(-30)
                    }),
                JourneySteps.Reason or JourneySteps.CheckAnswers =>
                    CreateJourneyInstanceForAllStepsCompleted(personId, populateOptional: true),
                _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
            });
    }

    protected Task<AlertType> GetKnownAlertType(bool isDbsAlertType = false) =>
        isDbsAlertType ? TestData.ReferenceDataCache.GetAlertTypeByDqtSanctionCode("") : TestData.ReferenceDataCache.GetAlertTypeByDqtSanctionCode("T1");

    private Task<JourneyInstance<AddAlertState>> CreateJourneyInstance(Guid personId, AddAlertState state) =>
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

using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public abstract class AddAlertTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<JourneyInstance<AddAlertState>> CreateJourneyInstanceForNoStepsCompletedAsync(Guid personId)
    {
        return CreateJourneyInstanceForCompletedStepAsync(JourneyStepNames.Index, personId, alertType: null);
    }

    protected Task<JourneyInstance<AddAlertState>> CreateJourneyInstanceForIncompleteStepAsync(
        string stepName,
        Guid personId,
        bool populateOptional = true)
    {
        var previousStep = stepName switch
        {
            JourneyStepNames.Index or JourneyStepNames.AlertType => JourneyStepNames.Index,
            JourneyStepNames.Details => JourneyStepNames.AlertType,
            JourneyStepNames.Link => JourneyStepNames.Details,
            JourneyStepNames.StartDate => JourneyStepNames.Link,
            JourneyStepNames.Reason => JourneyStepNames.StartDate,
            JourneyStepNames.CheckAnswers => JourneyStepNames.Reason,
            _ => throw new ArgumentException($"Unknown {nameof(stepName)}: '{stepName}'", nameof(stepName))
        };

        return CreateJourneyInstanceForCompletedStepAsync(previousStep, personId, populateOptional: populateOptional);
    }

    protected async Task<JourneyInstance<AddAlertState>> CreateJourneyInstanceForCompletedStepAsync(
        string stepName,
        Guid personId,
        AlertType? alertType = null,
        bool populateOptional = true)
    {
        alertType ??= await GetKnownAlertTypeAsync();

        return await
            (stepName switch
            {
                JourneyStepNames.Index =>
                    CreateJourneyInstanceAsync(personId, instanceId => new AddAlertState()
                    {
                        Steps = new JourneySteps(
                            GetStepUrl(JourneyStepNames.Index, personId, instanceId),
                            GetStepUrl(JourneyStepNames.AlertType, personId, instanceId))
                    }),
                JourneyStepNames.AlertType =>
                    CreateJourneyInstanceAsync(personId, instanceId => new AddAlertState
                    {
                        Steps = new JourneySteps(
                            GetStepUrl(JourneyStepNames.Index, personId, instanceId),
                            GetStepUrl(JourneyStepNames.AlertType, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Details, personId, instanceId)),
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name
                    }),
                JourneyStepNames.Details =>
                    CreateJourneyInstanceAsync(personId, instanceId => new AddAlertState
                    {
                        Steps = new JourneySteps(
                            GetStepUrl(JourneyStepNames.Index, personId, instanceId),
                            GetStepUrl(JourneyStepNames.AlertType, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Details, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Link, personId, instanceId)),
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name,
                        Details = "Alert Details"
                    }),
                JourneyStepNames.Link =>
                    CreateJourneyInstanceAsync(personId, instanceId => new AddAlertState
                    {
                        Steps = new JourneySteps(
                            GetStepUrl(JourneyStepNames.Index, personId, instanceId),
                            GetStepUrl(JourneyStepNames.AlertType, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Details, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Link, personId, instanceId),
                            GetStepUrl(JourneyStepNames.StartDate, personId, instanceId)),
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name,
                        Details = "Alert Details",
                        AddLink = populateOptional,
                        Link = populateOptional ? "https://www.example.com" : null
                    }),
                JourneyStepNames.StartDate =>
                    CreateJourneyInstanceAsync(personId, instanceId => new AddAlertState
                    {
                        Steps = new JourneySteps(
                            GetStepUrl(JourneyStepNames.Index, personId, instanceId),
                            GetStepUrl(JourneyStepNames.AlertType, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Details, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Link, personId, instanceId),
                            GetStepUrl(JourneyStepNames.StartDate, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Reason, personId, instanceId)),
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name,
                        Details = "Alert Details",
                        AddLink = populateOptional,
                        Link = populateOptional ? "https://www.example.com" : null,
                        StartDate = Clock.Today.AddDays(-30)
                    }),
                JourneyStepNames.Reason =>
                    CreateJourneyInstanceAsync(personId, instanceId => new AddAlertState
                    {
                        Steps = new JourneySteps(
                            GetStepUrl(JourneyStepNames.Index, personId, instanceId),
                            GetStepUrl(JourneyStepNames.AlertType, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Details, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Link, personId, instanceId),
                            GetStepUrl(JourneyStepNames.StartDate, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Reason, personId, instanceId),
                            GetStepUrl(JourneyStepNames.CheckAnswers, personId, instanceId)),
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name,
                        Details = "Alert Details",
                        AddLink = populateOptional,
                        Link = populateOptional ? "https://www.example.com" : null,
                        StartDate = Clock.Today.AddDays(-30),
                        AddReason = AddAlertReasonOption.AnotherReason,
                        HasAdditionalReasonDetail = populateOptional,
                        AddReasonDetail = populateOptional ? "More details" : null,
                        Evidence = new()
                        {
                            UploadEvidence = populateOptional,
                            UploadedEvidenceFile = populateOptional ? new()
                            {
                                FileId = Guid.NewGuid(),
                                FileName = "evidence.jpeg",
                                FileSizeDescription = "5MB"
                            } : null
                        }
                    }),
                JourneyStepNames.CheckAnswers =>
                    CreateJourneyInstanceAsync(personId, instanceId => new AddAlertState
                    {
                        Steps = new JourneySteps(
                            GetStepUrl(JourneyStepNames.Index, personId, instanceId),
                            GetStepUrl(JourneyStepNames.AlertType, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Details, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Link, personId, instanceId),
                            GetStepUrl(JourneyStepNames.StartDate, personId, instanceId),
                            GetStepUrl(JourneyStepNames.Reason, personId, instanceId),
                            GetStepUrl(JourneyStepNames.CheckAnswers, personId, instanceId)),
                        AlertTypeId = alertType.AlertTypeId,
                        AlertTypeName = alertType.Name,
                        Details = "Alert Details",
                        AddLink = populateOptional,
                        Link = populateOptional ? "https://www.example.com" : null,
                        StartDate = Clock.Today.AddDays(-30),
                        AddReason = AddAlertReasonOption.AnotherReason,
                        HasAdditionalReasonDetail = populateOptional,
                        AddReasonDetail = populateOptional ? "More details" : null,
                        Evidence = new()
                        {
                            UploadEvidence = populateOptional,
                            UploadedEvidenceFile = populateOptional ? new()
                            {
                                FileId = Guid.NewGuid(),
                                FileName = "evidence.jpeg",
                                FileSizeDescription = "5MB"
                            } : null
                        }
                    }),
                _ => throw new ArgumentException($"Unknown {nameof(stepName)}: '{stepName}'.", nameof(stepName))
            });
    }

    protected Task<AlertType> GetKnownAlertTypeAsync(bool isDbsAlertType = false) =>
        isDbsAlertType ?
            TestData.ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.DbsAlertTypeId) :
            TestData.ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.ProhibitionBySoSMisconduct);

    protected static string GetStepUrl(string stepName, Guid personId, JourneyInstanceId instanceId) =>
        stepName switch
        {
            JourneyStepNames.Index => $"/alerts/add?personId={personId}&{instanceId.GetUniqueIdQueryParameter()}",
            JourneyStepNames.AlertType => $"/alerts/add/type?personId={personId}&{instanceId.GetUniqueIdQueryParameter()}",
            JourneyStepNames.Details => $"/alerts/add/details?personId={personId}&{instanceId.GetUniqueIdQueryParameter()}",
            JourneyStepNames.Link => $"/alerts/add/link?personId={personId}&{instanceId.GetUniqueIdQueryParameter()}",
            JourneyStepNames.StartDate => $"/alerts/add/start-date?personId={personId}&{instanceId.GetUniqueIdQueryParameter()}",
            JourneyStepNames.Reason => $"/alerts/add/reason?personId={personId}&{instanceId.GetUniqueIdQueryParameter()}",
            JourneyStepNames.CheckAnswers => $"/alerts/add/check-answers?personId={personId}&{instanceId.GetUniqueIdQueryParameter()}",
            _ => throw new ArgumentException($"Unknown {nameof(stepName)}: '{stepName}'.", nameof(stepName))
        };

    private Task<JourneyInstance<AddAlertState>> CreateJourneyInstanceAsync(Guid personId, Func<JourneyInstanceId, AddAlertState> createState) =>
        CreateJourneyInstance(
            JourneyNames.AddAlert,
            createState,
            new KeyValuePair<string, object>("personId", personId));

    protected static class JourneyStepNames
    {
        public const string Index = nameof(Index);
        public const string AlertType = nameof(AlertType);
        public const string Details = nameof(Details);
        public const string Link = nameof(Link);
        public const string StartDate = nameof(StartDate);
        public const string Reason = nameof(Reason);
        public const string CheckAnswers = nameof(CheckAnswers);
    }
}

using GovUk.Questions.AspNetCore;
using GovUk.Questions.AspNetCore.State;
using GovUk.Questions.AspNetCore.Testing;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public abstract class AddAlertTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected Task<AddAlertJourneyCoordinator> CreateEmptyJourneyInstanceAsync(Guid personId) =>
        CreateJourneyInstanceAsync(personId, new());

    protected async Task<AddAlertJourneyCoordinator> CreateJourneyInstanceForAllStepsCompletedAsync(Guid personId, bool populateOptional = true, bool provideAdditionalInformation = false, AddAlertReasonOption addReasonOption = AddAlertReasonOption.AnotherReason)
    {
        var alertType = await GetKnownAlertTypeAsync();

        return await CreateJourneyInstanceAsync(personId, new AddAlertState
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Alert Details",
            AddLink = populateOptional,
            Link = populateOptional ? "https://www.example.com" : null,
            StartDate = TimeProvider.Today.AddDays(-30),
            AddReason = addReasonOption,
            ProvideAdditionalInformation = provideAdditionalInformation ? true : false,
            AddReasonDetail = addReasonOption == AddAlertReasonOption.AnotherReason ? "More details" : null,
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
            AdditionalInformation = provideAdditionalInformation == true ? "Some additional information" : null,
        });
    }

    protected async Task<AddAlertJourneyCoordinator> CreateJourneyInstanceForCompletedStepAsync(string step, Guid personId, bool populateOptional = true)
    {
        var alertType = await GetKnownAlertTypeAsync();

        return await CreateJourneyInstanceForCompletedStepAsync(step, personId, alertType, populateOptional);
    }

    protected async Task<AddAlertJourneyCoordinator> CreateJourneyInstanceForCompletedStepAsync(string step, Guid personId, Guid alertTypeId, bool populateOptional = true)
    {
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(alertTypeId);

        return await CreateJourneyInstanceForCompletedStepAsync(step, personId, alertType, populateOptional);
    }

    protected Task<AddAlertJourneyCoordinator> CreateJourneyInstanceForCompletedStepAsync(string step, Guid personId, AlertType alertType, bool populateOptional = true)
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
                        StartDate = TimeProvider.Today.AddDays(-30)
                    }),
                JourneySteps.Reason or JourneySteps.CheckAnswers =>
                    CreateJourneyInstanceForAllStepsCompletedAsync(personId, populateOptional: true),
                _ => throw new ArgumentException($"Unknown {nameof(step)}: '{step}'.", nameof(step))
            });
    }

    protected Task<AlertType> GetKnownAlertTypeAsync(bool isDbsAlertType = false) =>
        isDbsAlertType ? TestData.ReferenceDataCache.GetAlertTypeByDqtSanctionCodeAsync("") : TestData.ReferenceDataCache.GetAlertTypeByDqtSanctionCodeAsync("T4");

    protected AddAlertState? GetJourneyInstanceState(AddAlertJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (AddAlertState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    private async Task<AddAlertJourneyCoordinator> CreateJourneyInstanceAsync(Guid personId, AddAlertState state)
    {
        var journeyHelper = HostFixture.Services.GetRequiredService<JourneyHelper>();

        var coordinator = await journeyHelper.CreateInstanceAsync<AddAlertJourneyCoordinator>(
            JourneyNames.AddAlert,
            new RouteValueDictionary { ["personId"] = personId },
            _ => Task.FromResult<object>(state),
            pathUrls: []);

        // Seed the whole journey path so that any page under test is reachable; the pages' own guards
        // handle redirecting when prerequisite state is missing (mirroring the previous FormFlow behaviour).
        foreach (var url in new[]
        {
            $"/alerts/add/type?personId={personId}",
            $"/alerts/add/details?personId={personId}",
            $"/alerts/add/link?personId={personId}",
            $"/alerts/add/start-date?personId={personId}",
            $"/alerts/add/reason?personId={personId}",
            $"/alerts/add/check-answers?personId={personId}",
        })
        {
            AddUrlToPath(coordinator, url);
        }

        return coordinator;
    }

    private static void AddUrlToPath(JourneyCoordinator coordinator, string url)
    {
        var newStep = coordinator.CreateStepFromUrl(url);
        var newPath = new JourneyPath(coordinator.Path.Steps.Append(newStep));
        coordinator.UnsafeSetPath(newPath);
    }

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

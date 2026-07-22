using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TeacherPensions.Resolve;

public abstract class ResolveTeacherPensionsPotentialDuplicateTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task<ResolveTeacherPensionsPotentialDuplicateState> CreateStateAsync(SupportTask supportTask)
    {
        // Resolve TrnRequestService from its own scope so that it doesn't share a DbContext with the
        // test's other operations.
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var trnRequestService = scope.ServiceProvider.GetRequiredService<TrnRequestService>();

        return await ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator.CreateStateAsync(trnRequestService, supportTask);
    }

    protected Task<ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator> CreateJourneyInstanceAsync(
        string supportTaskReference,
        ResolveTeacherPensionsPotentialDuplicateState state)
    {
        var basePath = $"/support-tasks/teacher-pensions/{supportTaskReference}/resolve";

        // Seed the path the real journey would have built up by this point, so that every page is
        // reachable and back links match production. The journey forks at Matches: keeping the records
        // separate never visits Merge or CheckAnswers, and merging never visits the keep-separate pages.
        string[] pathUrls = state.PersonId == ResolveTeacherPensionsPotentialDuplicateState.KeepRecordSeparatePersonIdSentinel
            ? [$"{basePath}/matches", $"{basePath}/keep-record-separate", $"{basePath}/confirm-keep-record-separate"]
            : [$"{basePath}/matches", $"{basePath}/merge", $"{basePath}/check-answers"];

        return JourneyHelper.CreateInstanceAsync<ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator>(
            JourneyNames.ResolveTpsPotentialDuplicate,
            new RouteValueDictionary { ["supportTaskReference"] = supportTaskReference },
            _ => Task.FromResult<object>(state),
            pathUrls: pathUrls,
            // JourneyHelper activates coordinators with Activator.CreateInstance, which can't supply
            // this one's constructor dependencies.
            coordinatorFactory: () => ActivatorUtilities.CreateInstance<ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator>(HostFixture.Services));
    }

    protected ResolveTeacherPensionsPotentialDuplicateState? GetJourneyInstanceState(
        ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (ResolveTeacherPensionsPotentialDuplicateState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}

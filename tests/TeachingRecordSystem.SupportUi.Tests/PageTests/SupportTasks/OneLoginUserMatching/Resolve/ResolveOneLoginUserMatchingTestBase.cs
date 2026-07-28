using System.Diagnostics;
using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserMatching.Resolve;

public abstract class ResolveOneLoginUserMatchingTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task<ResolveOneLoginUserMatchingJourneyCoordinator> CreateJourneyInstanceAsync(
        SupportTask supportTask,
        Action<ResolveOneLoginUserMatchingState>? configureState = null)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.OneLoginUserIdVerification or SupportTaskType.OneLoginUserRecordMatching);

        // Resolve OneLoginService from its own scope so that it doesn't share a DbContext with the
        // test's other operations.
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var oneLoginService = scope.ServiceProvider.GetRequiredService<OneLoginService>();

        var state = await ResolveOneLoginUserMatchingJourneyCoordinator.CreateStateAsync(
            oneLoginService,
            supportTask,
            GetDefaultCompletionUrl(supportTask));

        configureState?.Invoke(state);

        return await CreateJourneyInstanceAsync(supportTask.SupportTaskReference, state);
    }

    /// <summary>
    /// Creates a journey instance with the given matches, instead of the ones the matching service finds.
    /// </summary>
    protected Task<ResolveOneLoginUserMatchingJourneyCoordinator> CreateJourneyInstanceWithMatchedPersonsAsync(
        SupportTask supportTask,
        Action<ResolveOneLoginUserMatchingState>? configureState = null,
        params MatchPersonResult[] matchedPersons)
    {
        var state = new ResolveOneLoginUserMatchingState
        {
            MatchedPersons = matchedPersons,
            CompletionUrl = GetDefaultCompletionUrl(supportTask)
        };

        configureState?.Invoke(state);

        return CreateJourneyInstanceAsync(supportTask.SupportTaskReference, state);
    }

    /// <summary>
    /// The URL the journey completes to when it's started without a <c>returnUrl</c>.
    /// </summary>
    protected string GetDefaultCompletionUrl(SupportTask supportTask)
    {
        var linkGenerator = HostFixture.Services.GetRequiredService<SupportUiLinkGenerator>();

        return supportTask.SupportTaskType is SupportTaskType.OneLoginUserIdVerification ?
            linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification() :
            linkGenerator.SupportTasks.OneLoginUserMatching.RecordMatching();
    }

    protected ResolveOneLoginUserMatchingState? GetJourneyInstanceState(ResolveOneLoginUserMatchingJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (ResolveOneLoginUserMatchingState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    private Task<ResolveOneLoginUserMatchingJourneyCoordinator> CreateJourneyInstanceAsync(
        string supportTaskReference,
        ResolveOneLoginUserMatchingState state)
    {
        var basePath = $"/support-tasks/one-login-user-matching/{supportTaskReference}/resolve";

        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps). The steps are ordered so that every
        // transition the journey can make moves forwards through this list.
        return JourneyHelper.CreateInstanceAsync<ResolveOneLoginUserMatchingJourneyCoordinator>(
            JourneyNames.ResolveOneLoginUserMatching,
            new RouteValueDictionary { ["supportTaskReference"] = supportTaskReference },
            _ => Task.FromResult<object>(state),
            pathUrls:
            [
                $"{basePath}/verify",
                $"{basePath}/matches",
                $"{basePath}/no-matches",
                $"{basePath}/reject",
                $"{basePath}/confirm-reject",
                $"{basePath}/not-connecting",
                $"{basePath}/confirm-not-connecting",
                $"{basePath}/confirm-connect"
            ],
            coordinatorFactory: CreateJourneyCoordinator<ResolveOneLoginUserMatchingJourneyCoordinator>);
    }
}

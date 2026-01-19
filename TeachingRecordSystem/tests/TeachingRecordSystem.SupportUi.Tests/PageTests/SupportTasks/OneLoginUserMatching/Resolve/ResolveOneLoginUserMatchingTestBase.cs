using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserMatching.Resolve;

public abstract class ResolveOneLoginUserMatchingTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task<JourneyInstance<ResolveOneLoginUserMatchingState>> CreateJourneyInstanceAsync(
        SupportTask supportTask,
        Action<ResolveOneLoginUserMatchingState>? configureState = null)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.OneLoginUserIdVerification or SupportTaskType.OneLoginUserRecordMatching);

        var state = await CreateJourneyStateWithFactory<ResolveOneLoginUserMatchingStateFactory, ResolveOneLoginUserMatchingState>(
            f => f.CreateAsync(supportTask));

        configureState?.Invoke(state);

        return await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserMatching,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));
    }

    protected async Task<JourneyInstance<ResolveOneLoginUserMatchingState>> CreateJourneyInstanceAsync(
        string supportTaskReference,
        Action<ResolveOneLoginUserMatchingState>? configureState = null,
        params MatchPersonResult[] matchedPersons)
    {
        var state = new ResolveOneLoginUserMatchingState { MatchedPersons = matchedPersons };

        configureState?.Invoke(state);

        return await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserMatching,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));
    }
}

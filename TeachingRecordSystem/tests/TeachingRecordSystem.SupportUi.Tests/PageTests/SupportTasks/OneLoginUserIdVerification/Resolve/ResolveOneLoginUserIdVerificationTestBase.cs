using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification.Resolve;

public abstract class ResolveOneLoginUserIdVerificationTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task<JourneyInstance<ResolveOneLoginUserIdVerificationState>> CreateJourneyInstanceAsync(
        SupportTask supportTask,
        Action<ResolveOneLoginUserIdVerificationState>? configureState = null)
    {
        Debug.Assert(supportTask.SupportTaskType is SupportTaskType.OneLoginUserIdVerification);

        var state = await base.CreateJourneyStateWithFactory<ResolveOneLoginUserIdVerificationStateFactory, ResolveOneLoginUserIdVerificationState>(
            f => f.CreateAsync(supportTask));

        configureState?.Invoke(state);

        return await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));
    }

    protected async Task<JourneyInstance<ResolveOneLoginUserIdVerificationState>> CreateJourneyInstanceAsync(
        string supportTaskReference,
        Action<ResolveOneLoginUserIdVerificationState>? configureState = null,
        params ResolveOneLoginUserIdVerificationStateMatch[] matchedPersons)
    {
        var state = new ResolveOneLoginUserIdVerificationState { MatchedPersons = matchedPersons };

        configureState?.Invoke(state);

        return await CreateJourneyInstance(
            JourneyNames.ResolveOneLoginUserIdVerification,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));
    }
}

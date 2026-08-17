using System.Reflection;
using System.Transactions;
using Xunit.v3;

namespace TeachingRecordSystem.TestCommon;

public class ClearDbBeforeTestAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        if (!test.TestCase.TestCollection.DisableParallelization)
        {
            throw new InvalidOperationException("Tests must be inside a collection with DisableParallelization set to true.");
        }

        using var sc = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);

        // xUnit has no asynchronous equivalent of this hook, so blocking is the only option. Task.Run keeps the
        // continuations off the test's synchronization context, which the blocked thread is holding.
#pragma warning disable VSTHRD002
        Task.Run(() => DbHelper.Instance.ClearDataAsync()).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }
}

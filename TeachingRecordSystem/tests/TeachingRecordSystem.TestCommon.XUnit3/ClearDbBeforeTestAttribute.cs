using System.Reflection;
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

#pragma warning disable VSTHRD002
        DbHelper.Instance.ClearDataAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }
}

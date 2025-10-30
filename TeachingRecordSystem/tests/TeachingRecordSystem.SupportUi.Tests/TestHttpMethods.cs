using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace TeachingRecordSystem.SupportUi.Tests;

[Flags]
public enum TestHttpMethods
{
    None = 0,
    Get = 1 << 1,
    Post = 1 << 2,
    GetAndPost = Get | Post
}

public class HttpMethodsAttribute(TestHttpMethods methods) : DataAttribute
{
    public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker) =>
        new(methods.SplitTestMethods().Select(m => new TheoryDataRow(m)).ToArray());

    public override bool SupportsDiscoveryEnumeration() => true;
}

public static class Extensions
{
    public static IEnumerable<HttpMethod> SplitTestMethods(this TestHttpMethods methods)
    {
        if (methods.HasFlag(TestHttpMethods.Get))
        {
            yield return HttpMethod.Get;
        }

        if (methods.HasFlag(TestHttpMethods.Post))
        {
            yield return HttpMethod.Post;
        }
    }
}

namespace TeachingRecordSystem.SupportUi.Tests;

[Flags]
public enum TestHttpMethods
{
    None = 0,
    Get = 1 << 1,
    Post = 1 << 2,
    GetAndPost = Get | Post
}

public class HttpMethodsAttribute(TestHttpMethods methods) : DataSourceGeneratorAttribute<HttpMethod>
{
    protected override IEnumerable<Func<HttpMethod>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        foreach (var method in methods.SplitTestMethods())
        {
            yield return () => method;
        }
    }
}

public class MatrixHttpMethodsAttribute(TestHttpMethods methods) : MatrixAttribute<HttpMethod>
{
    public override object?[] GetObjects(DataGeneratorMetadata dataGeneratorMetadata)
    {
        return methods.SplitTestMethods().Cast<object?>().ToArray();
    }
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

namespace TeachingRecordSystem.Api.Tests.V3.VNext;

public abstract class TestBase : Tests.TestBase
{
    protected TestBase(HostFixture hostFixture) : base(hostFixture)
    {
    }

    public HttpClient GetHttpClientWithApiKey() => GetHttpClientWithApiKey(VersionRegistry.V3MinorVersions.VNext);
}

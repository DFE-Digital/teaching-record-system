namespace TeachingRecordSystem.Api.Tests.V3.V20240606;

public abstract class TestBase : Tests.TestBase
{
    protected TestBase(HostFixture hostFixture) : base(hostFixture)
    {
    }

    public HttpClient GetHttpClientWithApiKey() => GetHttpClientWithApiKey(VersionRegistry.V3MinorVersions.V20240606);
}

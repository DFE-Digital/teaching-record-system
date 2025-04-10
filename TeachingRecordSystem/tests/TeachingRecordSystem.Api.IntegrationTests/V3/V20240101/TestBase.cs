namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240101;

public abstract class TestBase : IntegrationTests.TestBase
{
    protected TestBase(HostFixture hostFixture) : base(hostFixture)
    {
    }

    public HttpClient GetHttpClientWithApiKey() => GetHttpClientWithApiKey(VersionRegistry.V3MinorVersions.V20240101);
}

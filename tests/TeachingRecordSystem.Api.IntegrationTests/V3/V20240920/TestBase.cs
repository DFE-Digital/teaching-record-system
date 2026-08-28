namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240920;

public abstract class TestBase : IntegrationTests.TestBase
{
    public const string Version = VersionRegistry.V3MinorVersions.V20240920;

    protected TestBase(HostFixture hostFixture) : base(hostFixture)
    {
    }

    public HttpClient GetHttpClientWithApiKey() =>
        GetHttpClientWithApiKey(Version);
}

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240412;

public abstract class TestBase : IntegrationTests.TestBase
{
    public const string Version = VersionRegistry.V3MinorVersions.V20240412;

    protected TestBase(HostFixture hostFixture) : base(hostFixture)
    {
    }

    public HttpClient GetHttpClientWithApiKey() => GetHttpClientWithApiKey(Version);
}

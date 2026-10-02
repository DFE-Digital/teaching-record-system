namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240412;

public abstract class TestBase(HostFixture hostFixture) : IntegrationTests.TestBase(hostFixture)
{
    public const string Version = VersionRegistry.V3MinorVersions.V20240412;


    public HttpClient GetHttpClientWithApiKey() => GetHttpClientWithApiKey(Version);
}

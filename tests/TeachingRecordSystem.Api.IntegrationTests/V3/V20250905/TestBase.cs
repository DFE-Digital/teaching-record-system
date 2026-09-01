namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250905;

public abstract class TestBase(HostFixture hostFixture) : IntegrationTests.TestBase(hostFixture)
{
    public const string Version = VersionRegistry.V3MinorVersions.V20250905;

    public ReferenceDataCache ReferenceCache => HostFixture.Services.GetRequiredService<ReferenceDataCache>();


    public HttpClient GetHttpClientWithApiKey() =>
        GetHttpClientWithApiKey(Version);
}

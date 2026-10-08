namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250804;

public abstract class TestBase(HostFixture hostFixture) : IntegrationTests.TestBase(hostFixture)
{
    public const string Version = VersionRegistry.V3MinorVersions.V20250804;

    public ReferenceDataCache ReferenceCache => HostFixture.Services.GetRequiredService<ReferenceDataCache>();


    public HttpClient GetHttpClientWithApiKey() =>
        GetHttpClientWithApiKey(Version);
}

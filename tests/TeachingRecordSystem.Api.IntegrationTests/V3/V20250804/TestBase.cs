namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250804;

public abstract class TestBase : IntegrationTests.TestBase
{
    public const string Version = VersionRegistry.V3MinorVersions.V20250804;

    public ReferenceDataCache ReferenceCache { get; }

    protected TestBase(HostFixture hostFixture) : base(hostFixture)
    {
        ReferenceCache = hostFixture.Services.GetRequiredService<ReferenceDataCache>();
    }

    public HttpClient GetHttpClientWithApiKey() =>
        GetHttpClientWithApiKey(Version);
}

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20260120;

public abstract class TestBase : IntegrationTests.TestBase
{
    public const string Version = VersionRegistry.V3MinorVersions.V20260120;

    public ReferenceDataCache ReferenceCache { get; }

    protected TestBase(HostFixture hostFixture) : base(hostFixture)
    {
        ReferenceCache = hostFixture.Services.GetRequiredService<ReferenceDataCache>();
    }

    public HttpClient GetHttpClientWithApiKey() =>
        GetHttpClientWithApiKey(Version);
}

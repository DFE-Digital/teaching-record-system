namespace TeachingRecordSystem.Api.IntegrationTests.V3.VNext;

public abstract class TestBase : IntegrationTests.TestBase
{
    public const string Version = VersionRegistry.V3MinorVersions.VNext;

    public ReferenceDataCache ReferenceCache { get; }

    protected TestBase(HostFixture hostFixture) : base(hostFixture)
    {
        ReferenceCache = hostFixture.Services.GetRequiredService<ReferenceDataCache>();
    }

    public HttpClient GetHttpClientWithApiKey() =>
        GetHttpClientWithApiKey(Version);

    public HttpClient GetHttpClientWithIdentityAccessToken(string trn, string scope = "dqt:read") =>
        GetHttpClientWithIdentityAccessToken(trn, scope, Version);
}

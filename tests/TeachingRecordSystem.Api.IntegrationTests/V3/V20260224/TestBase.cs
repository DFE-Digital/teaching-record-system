namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20260224;

public abstract class TestBase(HostFixture hostFixture) : IntegrationTests.TestBase(hostFixture)
{
    public const string Version = VersionRegistry.V3MinorVersions.V20260224;

    public ReferenceDataCache ReferenceCache => HostFixture.Services.GetRequiredService<ReferenceDataCache>();


    public HttpClient GetHttpClientWithApiKey() =>
        GetHttpClientWithApiKey(Version);

    public HttpClient GetHttpClientWithIdentityAccessToken(string trn, string scope = "dqt:read") =>
        GetHttpClientWithIdentityAccessToken(trn, scope, Version);
}

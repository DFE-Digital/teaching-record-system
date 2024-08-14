namespace TeachingRecordSystem.Api.Tests.V3.V20240814;

public abstract class TestBase(HostFixture hostFixture) : Tests.TestBase(hostFixture)
{
    public const string Version = VersionRegistry.V3MinorVersions.V20240814;

    public HttpClient GetHttpClient() => GetHttpClient(Version);

    public HttpClient GetHttpClientWithApiKey() => GetHttpClientWithApiKey(Version);

    public HttpClient GetHttpClientWithIdentityAccessToken(string trn) => GetHttpClientWithIdentityAccessToken(trn, version: Version);
}

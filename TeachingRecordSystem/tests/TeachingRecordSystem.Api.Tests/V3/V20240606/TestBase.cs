namespace TeachingRecordSystem.Api.Tests.V3.V20240606;

public abstract class TestBase(HostFixture hostFixture) : Tests.TestBase(hostFixture)
{
    public const string Version = VersionRegistry.V3MinorVersions.V20240606;

    public HttpClient GetHttpClient() => GetHttpClient(Version);

    public HttpClient GetHttpClientWithApiKey() => GetHttpClientWithApiKey(Version);

    public HttpClient GetHttpClientWithIdentityAccessToken(string trn) => GetHttpClientWithIdentityAccessToken(trn, version: Version);
}

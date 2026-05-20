namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240412;

public abstract class TestBase : IntegrationTests.TestBase
{
    protected TestBase(HostFixture hostFixture) : base(hostFixture)
    {
    }

    public HttpClient GetHttpClientWithApiKey() => GetHttpClientWithApiKey(VersionRegistry.V3MinorVersions.V20240412);

    public HttpClient GetHttpClientWithIdentityAccessToken(string trn, string scope = "dqt:read") =>
        GetHttpClientWithIdentityAccessToken(trn, scope, VersionRegistry.V3MinorVersions.V20240412);
}

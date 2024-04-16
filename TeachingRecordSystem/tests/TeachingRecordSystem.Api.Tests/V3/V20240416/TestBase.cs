namespace TeachingRecordSystem.Api.Tests.V3.V20240416;

public abstract class TestBase : Tests.TestBase
{
    protected TestBase(HostFixture hostFixture) : base(hostFixture)
    {
    }

    public HttpClient GetHttpClientWithApiKey() => GetHttpClientWithApiKey(VersionRegistry.V3MinorVersions.V20240416);

    public HttpClient GetHttpClientWithIdentityAccessToken(string trn, string scope = "dqt:read") =>
        GetHttpClientWithIdentityAccessToken(trn, scope, VersionRegistry.V3MinorVersions.V20240416);
}

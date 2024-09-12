namespace TeachingRecordSystem.Api.Tests.V3.V20240912;

public abstract class TestBase : Tests.TestBase
{
    public const string Version = VersionRegistry.V3MinorVersions.V20240912;

    protected TestBase(HostFixture hostFixture) : base(hostFixture)
    {
    }

    public HttpClient GetHttpClientWithApiKey() =>
        GetHttpClientWithApiKey(Version);

    public HttpClient GetHttpClientWithIdentityAccessToken(string trn, string scope = "dqt:read") =>
        GetHttpClientWithIdentityAccessToken(trn, scope, Version);
}

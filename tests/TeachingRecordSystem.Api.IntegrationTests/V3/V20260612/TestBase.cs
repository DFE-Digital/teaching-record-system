namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20260612;

public abstract class TestBase(HostFixture hostFixture) : IntegrationTests.TestBase(hostFixture)
{
    public const string Version = VersionRegistry.V3MinorVersions.V20260612;

    public ReferenceDataCache ReferenceCache => HostFixture.Services.GetRequiredService<ReferenceDataCache>();


    public HttpClient GetHttpClientWithApiKey() =>
        GetHttpClientWithApiKey(Version);

    public HttpClient GetHttpClientWithAuthorizeAccessToken(string trn) =>
        GetHttpClientWithAuthorizeAccessToken(trn, Version);

    public HttpClient GetHttpClientWithAuthorizeAccessTokenForTrnRequest(Guid applicationUserId, string trnRequestId) =>
        GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUserId, trnRequestId, Version);
}

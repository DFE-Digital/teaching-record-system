using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.Api.IntegrationTests.V2.Operations;

[Collection(nameof(DisableParallelization))]  // To keep the set of training providers consistent
public class GetIttProvidersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    // TODO: investigate!
    [Fact(Skip = "Breaks certain API integration tests for unknown reason after XUnit upgrade")]
    public async Task Given_request_returns_list_of_itt_providers()
    {
        // Arrange
        await DbHelper.ClearDataAsync();
        await WithDbContextAsync(SeedLookupData.ResetTrainingProvidersAsync);

        var request = new HttpRequestMessage(HttpMethod.Get, "/v2/itt-providers");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                ittProviders = new[]
                {
                    new
                    {
                        providerName = "TestProviderName",
                        ukprn = "11111111"
                    },
                    new
                    {
                        providerName = "TestProviderNameInactive",
                        ukprn = "23456789"
                    }
                }
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }
}

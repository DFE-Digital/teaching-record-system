using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Moq;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.TestCommon;
using Xunit;

namespace QualifiedTeachersApi.Tests.V2.Operations;

public class GetIttProvidersTests : ApiTestBase
{
    public GetIttProvidersTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Fact]
    public async Task Given_request_returns_list_of_itt_providers()
    {
        // Arrange
        var provider1 = new Account()
        {
            Name = "Provider 1",
            dfeta_UKPRN = "1234567"
        };

        var provider2 = new Account()
        {
            Name = "Provider 2",
            dfeta_UKPRN = "2345678"
        };

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.GetIttProviders(false))
            .ReturnsAsync(new[] { provider1, provider2 });

        var request = new HttpRequestMessage(HttpMethod.Get, "/v2/itt-providers");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                ittProviders = new[]
                {
                    new
                    {
                        providerName = provider1.Name,
                        ukprn = provider1.dfeta_UKPRN
                    },
                    new
                    {
                        providerName = provider2.Name,
                        ukprn = provider2.dfeta_UKPRN
                    }
                }
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }
}

using DqtApi.DataStore.Crm;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class GetIttProvidersTests
    {
        private readonly DataverseAdapter _dataverseAdapter;

        public GetIttProvidersTests(CrmClientFixture crmClientFixture)
        {
            _dataverseAdapter = crmClientFixture.CreateDataverseAdapter();
        }

        [Fact]
        public async Task Returns_providers()
        {
            // Arrange

            // Act
            var result = await _dataverseAdapter.GetIttProviders();

            // Assert
            Assert.NotEmpty(result);
        }
    }
}

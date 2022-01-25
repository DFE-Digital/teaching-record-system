using DqtApi.DataStore.Crm;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    [Collection(nameof(DataverseTestCollection))]
    public class GetCountryTests
    {
        private readonly DataverseAdapter _dataverseAdapter;

        public GetCountryTests(CrmClientFixture crmClientFixture)
        {
            _dataverseAdapter = crmClientFixture.CreateDataverseAdapter();
        }

        [Fact]
        public async Task Given_valid_country_code_returns_country()
        {
            // Arrange
            var countryCode = "XK";

            // Act
            var result = await _dataverseAdapter.GetCountry(countryCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(countryCode, result.dfeta_Value);
        }

        [Fact]
        public async Task Given_invalid_country_code_returns_null()
        {
            // Arrange
            var countryCode = "XXXX";

            // Act
            var result = await _dataverseAdapter.GetCountry(countryCode);

            // Assert
            Assert.Null(result);
        }
    }
}

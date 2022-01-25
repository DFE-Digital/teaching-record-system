using DqtApi.DataStore.Crm;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    [Collection(nameof(DataverseTestCollection))]
    public class GetEarlyYearsStatusTests
    {
        private readonly DataverseAdapter _dataverseAdapter;

        public GetEarlyYearsStatusTests(CrmClientFixture crmClientFixture)
        {
            _dataverseAdapter = crmClientFixture.CreateDataverseAdapter();
        }

        [Fact]
        public async Task Given_valid_value_returns_entity()
        {
            // Arrange
            var value = "220";

            // Act
            var result = await _dataverseAdapter.GetEarlyYearsStatus(value);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(value, result.dfeta_Value);
        }

        [Fact]
        public async Task Given_invalid_value_returns_null()
        {
            // Arrange
            var countryCode = "XXXX";

            // Act
            var result = await _dataverseAdapter.GetEarlyYearsStatus(countryCode);

            // Assert
            Assert.Null(result);
        }
    }
}

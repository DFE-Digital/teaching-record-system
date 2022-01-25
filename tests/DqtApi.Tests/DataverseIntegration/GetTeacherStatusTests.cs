using DqtApi.DataStore.Crm;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class GetTeacherStatusTests
    {
        private readonly DataverseAdapter _dataverseAdapter;

        public GetTeacherStatusTests(CrmClientFixture crmClientFixture)
        {
            _dataverseAdapter = crmClientFixture.CreateDataverseAdapter();
        }

        [Fact]
        public async Task Given_valid_value_returns_entity()
        {
            // Arrange
            var value = "211";

            // Act
            var result = await _dataverseAdapter.GetTeacherStatus(value, qtsDateRequired: false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(value, result.dfeta_Value);
        }

        [Fact]
        public async Task Given_invalid_value_returns_null()
        {
            // Arrange
            var value = "XXXX";

            // Act
            var result = await _dataverseAdapter.GetTeacherStatus(value, qtsDateRequired: false);

            // Assert
            Assert.Null(result);
        }
    }
}

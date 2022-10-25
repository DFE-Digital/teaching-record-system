using DqtApi.DataStore.Crm;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class GetHeQualificationByCodeTests : IAsyncLifetime
    {
        private readonly CrmClientFixture.TestDataScope _dataScope;
        private readonly DataverseAdapter _dataverseAdapter;

        public GetHeQualificationByCodeTests(CrmClientFixture crmClientFixture)
        {
            _dataScope = crmClientFixture.CreateTestDataScope();
            _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _dataScope.DisposeAsync();

        [Fact]
        public async Task Given_valid_qualification_name_returns_country()
        {
            // Arrange
            var qualificationCode = "400";  // First Degree

            // Act
            var result = await _dataverseAdapter.GetHeQualificationByCode(qualificationCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(qualificationCode, result.dfeta_Value);
        }

        [Fact]
        public async Task Given_invalid_qualification_name_returns_null()
        {
            // Arrange
            var qualificationCode = "XXXX";

            // Act
            var result = await _dataverseAdapter.GetHeQualificationByCode(qualificationCode);

            // Assert
            Assert.Null(result);
        }
    }
}

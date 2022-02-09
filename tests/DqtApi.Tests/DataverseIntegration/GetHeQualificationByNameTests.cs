using DqtApi.DataStore.Crm;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class GetHeQualificationByNameTests : IAsyncLifetime
    {
        private readonly CrmClientFixture.TestDataScope _dataScope;
        private readonly DataverseAdapter _dataverseAdapter;

        public GetHeQualificationByNameTests(CrmClientFixture crmClientFixture)
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
            var qualificationName = "First Degree";

            // Act
            var result = await _dataverseAdapter.GetHeQualificationByName(qualificationName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(qualificationName, result.dfeta_name);
        }

        [Fact]
        public async Task Given_invalid_qualification_name_returns_null()
        {
            // Arrange
            var QualificationName = "XXXX";

            // Act
            var result = await _dataverseAdapter.GetHeQualificationByName(QualificationName);

            // Assert
            Assert.Null(result);
        }
    }
}

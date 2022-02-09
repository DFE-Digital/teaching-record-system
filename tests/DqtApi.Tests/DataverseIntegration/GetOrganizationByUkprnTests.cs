using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class GetOrganizationByUkprnTests : IAsyncLifetime
    {
        private readonly CrmClientFixture.TestDataScope _dataScope;
        private readonly DataverseAdapter _dataverseAdapter;

        public GetOrganizationByUkprnTests(CrmClientFixture crmClientFixture)
        {
            _dataScope = crmClientFixture.CreateTestDataScope();
            _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _dataScope.DisposeAsync();

        [Fact]
        public async Task Given_valid_ukprn_returns_account()
        {
            // Arrange
            var ukprn = "10044534";

            // Act
            var result = await _dataverseAdapter.GetOrganizationByUkprn(ukprn, columnNames: Account.Fields.dfeta_UKPRN);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ukprn, result.dfeta_UKPRN);
        }

        [Fact]
        public async Task Given_invalid_ukprn_returns_null()
        {
            // Arrange
            var ukprn = "xxx";

            // Act
            var result = await _dataverseAdapter.GetOrganizationByUkprn(ukprn, columnNames: Account.Fields.dfeta_UKPRN);

            // Assert
            Assert.Null(result);
        }
    }
}

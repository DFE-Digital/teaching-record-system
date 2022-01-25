using DqtApi.DataStore.Crm;
using Microsoft.PowerPlatform.Dataverse.Client;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    [Collection(nameof(DataverseTestCollection))]
    public class GetHeQualificationByNameTests
    {
        private readonly DataverseAdapter _dataverseAdapter;
        private readonly ServiceClient _serviceClient;

        public GetHeQualificationByNameTests(CrmClientFixture crmClientFixture)
        {
            _dataverseAdapter = crmClientFixture.CreateDataverseAdapter();
            _serviceClient = crmClientFixture.ServiceClient;
        }

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

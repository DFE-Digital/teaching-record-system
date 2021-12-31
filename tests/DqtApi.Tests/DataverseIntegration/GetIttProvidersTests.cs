using System.Threading.Tasks;
using DqtApi.DAL;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    [Collection(nameof(DataverseTestCollection))]
    public class GetIttProvidersTests : IClassFixture<CrmClientFixture>
    {
        private readonly DataverseAdaptor _dataverseAdaptor;

        public GetIttProvidersTests(CrmClientFixture crmClientFixture)
        {
            _dataverseAdaptor = crmClientFixture.CreateDataverseAdaptor();
        }

        [Fact]
        public async Task Returns_providers()
        {
            // Arrange

            // Act
            var result = await _dataverseAdaptor.GetIttProviders();

            // Assert
            Assert.NotEmpty(result);
        }
    }
}

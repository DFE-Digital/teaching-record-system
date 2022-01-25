using DqtApi.DataStore.Crm;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    [Collection(nameof(DataverseTestCollection))]
    public class GetIttSubjectByNameTests
    {
        private readonly DataverseAdapter _dataverseAdapter;

        public GetIttSubjectByNameTests(CrmClientFixture crmClientFixture)
        {
            _dataverseAdapter = crmClientFixture.CreateDataverseAdapter();
        }

        [Fact]
        public async Task Given_valid_subject_name_returns_country()
        {
            // Arrange
            var subjectName = "computer science";

            // Act
            var result = await _dataverseAdapter.GetIttSubjectByName(subjectName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(subjectName, result.dfeta_name);
        }

        [Fact]
        public async Task Given_invalid_subject_name_returns_null()
        {
            // Arrange
            var subjectName = "XXXX";

            // Act
            var result = await _dataverseAdapter.GetIttSubjectByName(subjectName);

            // Assert
            Assert.Null(result);
        }
    }
}

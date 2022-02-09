using DqtApi.DataStore.Crm;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class GetIttSubjectByNameTests : IAsyncLifetime
    {
        private readonly CrmClientFixture.TestDataScope _dataScope;
        private readonly DataverseAdapter _dataverseAdapter;

        public GetIttSubjectByNameTests(CrmClientFixture crmClientFixture)
        {
            _dataScope = crmClientFixture.CreateTestDataScope();
            _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _dataScope.DisposeAsync();

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

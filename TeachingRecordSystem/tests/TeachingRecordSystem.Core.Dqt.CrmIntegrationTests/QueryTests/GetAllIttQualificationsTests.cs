namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetAllIttQualificationsTests
{
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetAllIttQualificationsTests(CrmClientFixture crmClientFixture)
    {
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var query = new GetAllIttQualificationsQuery();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        Assert.NotEmpty(result);
    }
}

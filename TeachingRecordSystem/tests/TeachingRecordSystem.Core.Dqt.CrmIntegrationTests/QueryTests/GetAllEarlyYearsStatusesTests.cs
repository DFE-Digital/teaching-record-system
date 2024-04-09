namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetAllEarlyYearsStatusesTests
{
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetAllEarlyYearsStatusesTests(CrmClientFixture crmClientFixture)
    {
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var query = new GetAllActiveEarlyYearsStatusesQuery();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        Assert.NotEmpty(result);
    }
}

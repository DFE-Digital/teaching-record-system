namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

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
        var query = new GetAllEarlyYearsStatusesQuery();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        Assert.NotEmpty(result);
    }
}

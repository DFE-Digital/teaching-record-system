namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetAllActiveITTSubjectsTests
{
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetAllActiveITTSubjectsTests(CrmClientFixture crmClientFixture)
    {
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var query = new GetAllActiveITTSubjectsQuery();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        Assert.NotEmpty(result);
    }
}

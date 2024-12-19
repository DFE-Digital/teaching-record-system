namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetAllActiveIttSubjectsTests
{
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetAllActiveIttSubjectsTests(CrmClientFixture crmClientFixture)
    {
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var query = new GetAllActiveIttSubjectsQuery();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        Assert.NotEmpty(result);
    }
}

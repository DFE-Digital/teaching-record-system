namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class GetAllSanctionCodesTests
{
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetAllSanctionCodesTests(CrmClientFixture crmClientFixture)
    {
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var query = new GetAllSanctionCodesQuery();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        Assert.NotEmpty(result);
    }
}

namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class GetAllMqEstablishmentsTests
{
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetAllMqEstablishmentsTests(CrmClientFixture crmClientFixture)
    {
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var query = new GetAllMqEstablishmentsQuery();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        Assert.NotEmpty(result);
    }
}

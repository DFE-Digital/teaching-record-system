using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class GetSanctionsByContactIdsTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetSanctionsByContactIdsTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = _dataScope.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task ReturnsSanctionsForEachContactIdSpecified()
    {
        // Arrange
        // Create a single sanction for contact 1, two for contact 2 and none for contact 3
        var person1Sanction1Code = "G1";
        var person1 = await _dataScope.TestData.CreatePerson(x => x.WithSanction(person1Sanction1Code));
        var person2Sanction1Code = "G1";
        var person2Sanction2Code = "A1";
        var person2 = await _dataScope.TestData.CreatePerson(x => x.WithSanction(person2Sanction1Code).WithSanction(person2Sanction2Code));
        var person3 = await _dataScope.TestData.CreatePerson();

        // Act
        var sanctionCodesByContactId = await _crmQueryDispatcher.ExecuteQuery(
            new GetSanctionsByContactIdsQuery(new[] { person1.ContactId, person2.ContactId, person3.ContactId }, ActiveOnly: false, new ColumnSet()));

        // Assert
        Assert.Equal(3, sanctionCodesByContactId.Count);
        Assert.Collection(sanctionCodesByContactId[person1.ContactId], c => Assert.Equal(person1Sanction1Code, c.SanctionCode));
        Assert.Collection(sanctionCodesByContactId[person2.ContactId], c => Assert.Equal(person2Sanction2Code, c.SanctionCode), c => Assert.Equal(person2Sanction1Code, c.SanctionCode));
        Assert.Empty(sanctionCodesByContactId[person3.ContactId]);
    }
}

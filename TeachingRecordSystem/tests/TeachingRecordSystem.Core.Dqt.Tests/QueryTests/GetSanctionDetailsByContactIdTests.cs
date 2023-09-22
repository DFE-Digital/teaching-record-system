using static TeachingRecordSystem.TestCommon.CrmTestData;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class GetSanctionDetailsByContactIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetSanctionDetailsByContactIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_ForContactWithoutSanctions_ReturnsEmptyArray()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePerson();

        // Act
        var sanctions = await _crmQueryDispatcher.ExecuteQuery(new GetSanctionDetailsByContactIdQuery(person.ContactId));

        // Assert
        Assert.Empty(sanctions);
    }

    [Fact]
    public async Task WhenCalled_ForContactWithSanctions_ReturnsSanctionsAsExpected()
    {
        // Arrange
        var sanction1Code = "G1";
        var sanction1CodeName = (await _dataScope.TestData.ReferenceDataCache.GetSanctionCodeByValue(sanction1Code)).dfeta_name;
        var sanction2Code = "A1";
        var sanction2CodeName = (await _dataScope.TestData.ReferenceDataCache.GetSanctionCodeByValue(sanction2Code)).dfeta_name;
        var person = await _dataScope.TestData.CreatePerson(x => x.WithSanction(sanction1Code).WithSanction(sanction2Code));

        // Act
        var sanctions = await _crmQueryDispatcher.ExecuteQuery(new GetSanctionDetailsByContactIdQuery(person.ContactId));

        // Assert
        Assert.Equal(2, sanctions.Length);
        Assert.Collection(
            sanctions,
            s => Assert.Equal(sanction1CodeName, s.Description),
            s => Assert.Equal(sanction2CodeName, s.Description));
    }
}

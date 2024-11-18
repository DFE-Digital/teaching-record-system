using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.PluginTests;

public class UpdateQtlsDateSetTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;
    private DbFixture DbFixture;

    public UpdateQtlsDateSetTests(CrmClientFixture crmClientFixture, DbFixture fixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
        DbFixture = fixture;
    }

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    public Task InitializeAsync() => DbFixture.DbHelper.EnsureSchemaAsync();


    [Fact]
    public async Task WhenCreatingTeacherWithQtlsDate_QtlsHasBeenSetIsTrue()
    {
        // Arrange

        // Act
        var createPersonResult = await _dataScope.TestData.CreatePersonAsync(x =>
        {
            x.WithQtlsDate(new DateOnly(2021, 01, 1));
        });
        var person = await _crmQueryDispatcher.ExecuteQueryAsync(new GetActiveContactDetailByIdQuery(createPersonResult.PersonId, ColumnSet: new(
            Contact.Fields.dfeta_QtlsDateHasBeenSet)));

        // Assert
        Assert.True(person!.Contact.dfeta_QtlsDateHasBeenSet);
    }

    [Fact]
    public async Task WhenCreatingTeacherWithoutQtlsDate_QtlsHasBeenSetIsFalse()
    {
        // Arrange

        // Act
        var createPersonResult = await _dataScope.TestData.CreatePersonAsync();
        var person = await _crmQueryDispatcher.ExecuteQueryAsync(new GetActiveContactDetailByIdQuery(createPersonResult.PersonId, ColumnSet: new(
            Contact.Fields.dfeta_QtlsDateHasBeenSet)));

        // Assert
        Assert.False(person!.Contact.dfeta_QtlsDateHasBeenSet);
    }
}

using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.PluginTests;

public class QTSRegistrationDeleteTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;
    private DbFixture DbFixture;

    public QTSRegistrationDeleteTests(CrmClientFixture crmClientFixture, DbFixture fixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
        DbFixture = fixture;
    }

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    public Task InitializeAsync() => DbFixture.DbHelper.EnsureSchemaAsync();

    [Fact]
    public async Task WhenCreatingTeacherWithQtlsDate_QtsDateIsSet()
    {
        // Arrange
        var qtlsDate = new DateOnly(2021, 01, 1);

        // Act
        var createPersonResult = await _dataScope.TestData.CreatePersonAsync(x =>
        {
            x.WithQtlsDateInDqt(new DateOnly(2021, 01, 1));
        });
        var person = await _crmQueryDispatcher.ExecuteQueryAsync(new GetActiveContactDetailByIdQuery(createPersonResult.PersonId, ColumnSet: new(
            Contact.Fields.dfeta_QTSDate)));

        // Assert
        Assert.Equal(qtlsDate, person!.Contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
    }

    [Fact]
    public async Task WhenCreatingTeacherWithQts_QtsDateIsSet()
    {
        // Arrange
        var qtsDate = new DateOnly(2019, 01, 1);

        // Act
        var createPersonResult = await _dataScope.TestData.CreatePersonAsync(x =>
        {
            x.WithQts(qtsDate);
        });
        var person = await _crmQueryDispatcher.ExecuteQueryAsync(new GetActiveContactDetailByIdQuery(createPersonResult.PersonId, ColumnSet: new(
            Contact.Fields.dfeta_QTSDate)));

        // Assert
        Assert.Equal(qtsDate, person!.Contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
    }

    [Fact]
    public async Task WhenCreatingTeacherWithQtsAndQtls_QtsDateIsChosen()
    {
        // Arrange
        var qtsDate = new DateOnly(2019, 01, 1);
        var qtlsDate = new DateOnly(2022, 01, 1);

        // Act
        var createPersonResult = await _dataScope.TestData.CreatePersonAsync(x =>
        {
            x.WithQts(qtsDate);
            x.WithQtlsDateInDqt(qtlsDate);
        });
        var person = await _crmQueryDispatcher.ExecuteQueryAsync(new GetActiveContactDetailByIdQuery(createPersonResult.PersonId, ColumnSet: new(
            Contact.Fields.dfeta_QTSDate)));

        // Assert
        Assert.Equal(qtsDate, person!.Contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
    }

    [Fact]
    public async Task WhenCreatingTeacherWithQtsAndQtls_QtlsDateIsChosen()
    {
        // Arrange
        var qtsDate = new DateOnly(2019, 01, 1);
        var qtlsDate = new DateOnly(2012, 01, 1);

        // Act
        var createPersonResult = await _dataScope.TestData.CreatePersonAsync(x =>
        {
            x.WithQts(qtsDate);
            x.WithQtlsDateInDqt(qtlsDate);
        });
        var person = await _crmQueryDispatcher.ExecuteQueryAsync(new GetActiveContactDetailByIdQuery(createPersonResult.PersonId, ColumnSet: new(
            Contact.Fields.dfeta_QTSDate)));

        // Assert
        Assert.Equal(qtlsDate, person!.Contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true));
    }
}

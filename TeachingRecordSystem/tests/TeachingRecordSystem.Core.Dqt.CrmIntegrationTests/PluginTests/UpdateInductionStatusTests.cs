using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.PluginTests;

public class UpdateInductionStatusTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;
    private DbFixture DbFixture;

    public UpdateInductionStatusTests(CrmClientFixture crmClientFixture, DbFixture fixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
        DbFixture = fixture;
    }

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    public Task InitializeAsync() => DbFixture.DbHelper.EnsureSchema();

    [Theory]
    [InlineData(dfeta_InductionStatus.Exempt, dfeta_InductionExemptionReason.Exempt, "01/04/2018", dfeta_InductionStatus.Exempt)]
    [InlineData(dfeta_InductionStatus.Fail, null, "01/04/2018", dfeta_InductionStatus.Fail)]
    [InlineData(dfeta_InductionStatus.InProgress, null, "01/04/2018", dfeta_InductionStatus.Exempt)]
    [InlineData(dfeta_InductionStatus.InductionExtended, null, "01/04/2018", dfeta_InductionStatus.Exempt)]
    [InlineData(dfeta_InductionStatus.NotYetCompleted, null, "01/04/2018", dfeta_InductionStatus.Exempt)]
    [InlineData(dfeta_InductionStatus.Pass, null, "01/04/2018", dfeta_InductionStatus.Pass)]
    [InlineData(dfeta_InductionStatus.PassedinWales, null, "01/04/2018", dfeta_InductionStatus.PassedinWales)]
    [InlineData(dfeta_InductionStatus.RequiredtoComplete, null, "01/04/2018", dfeta_InductionStatus.Exempt)]
    [InlineData(dfeta_InductionStatus.FailedinWales, null, "01/04/2018", dfeta_InductionStatus.Exempt)]
    public async Task WhenCalled_WithTRAInductionStatusAndQTLSDate_ReturnsExpectedResults(dfeta_InductionStatus inductionStatus, dfeta_InductionExemptionReason? exemptionReason, string qtls, dfeta_InductionStatus expectedInductionStatus)
    {
        // Arrange
        var qtlsDate = DateOnly.Parse(qtls);
        var postcode = Faker.Address.UkPostCode();
        var establishment1 = await _dataScope.TestData.CreateAccount(x =>
        {
            x.WithName("SomeAccountName");
        });

        var createPersonResult = await _dataScope.TestData.CreatePerson(x =>
        {
            x.WithQts(new DateOnly(2021, 01, 1));
            x.WithQtlsDate(qtlsDate);
            x.WithInduction(inductionStatus: inductionStatus, inductionExemptionReason: exemptionReason, startDate: new DateOnly(2021, 01, 01), endDate: new DateOnly(2022, 01, 01), appropriateBodyOrgId: establishment1.AccountId);
        });

        // Act
        var person = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactDetailByIdQuery(createPersonResult.PersonId, ColumnSet: new(
                    Contact.Fields.dfeta_InductionStatus,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_qtlsdate)));

        // Assert
        Assert.Equal(expectedInductionStatus, person!.Contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task WhenCalled_WithoutTRAInductionStatusStatusAndQTLSDate_ReturnsExpectedResults()
    {
        // Arrange
        var qtlsDate = DateOnly.Parse("01/01/2021");
        var postcode = Faker.Address.UkPostCode();
        var establishment1 = await _dataScope.TestData.CreateAccount(x =>
        {
            x.WithName("SomeAccountName");
        });

        var createPersonResult = await _dataScope.TestData.CreatePerson(x =>
        {

            x.WithQtlsDate(qtlsDate);
            x.WithQts(new DateOnly(2021, 01, 1));
        });

        // Act
        var person = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactDetailByIdQuery(createPersonResult.PersonId, ColumnSet: new(
                    Contact.Fields.dfeta_InductionStatus,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_qtlsdate)));

        // Assert
        Assert.Equal(dfeta_InductionStatus.Exempt, person!.Contact.dfeta_InductionStatus);
    }

    [Theory]
    [InlineData(dfeta_InductionStatus.Exempt, dfeta_InductionExemptionReason.Exempt, dfeta_InductionStatus.Exempt)]
    [InlineData(dfeta_InductionStatus.Fail, null, dfeta_InductionStatus.Fail)]
    [InlineData(dfeta_InductionStatus.FailedinWales, null, dfeta_InductionStatus.FailedinWales)]
    [InlineData(dfeta_InductionStatus.InProgress, null, dfeta_InductionStatus.InProgress)]
    [InlineData(dfeta_InductionStatus.InductionExtended, null, dfeta_InductionStatus.InductionExtended)]
    [InlineData(dfeta_InductionStatus.NotYetCompleted, null, dfeta_InductionStatus.NotYetCompleted)]
    [InlineData(dfeta_InductionStatus.Pass, null, dfeta_InductionStatus.Pass)]
    [InlineData(dfeta_InductionStatus.PassedinWales, null, dfeta_InductionStatus.PassedinWales)]
    [InlineData(dfeta_InductionStatus.RequiredtoComplete, null, dfeta_InductionStatus.RequiredtoComplete)]
    public async Task WhenCalled_WithoutTRAQTLSDate_ReturnsExpectedResults(dfeta_InductionStatus inductionStatus, dfeta_InductionExemptionReason? exemptionReason, dfeta_InductionStatus expectedInductionStatus)
    {
        // Arrange
        var qtlsDate = DateOnly.Parse("01/01/2021");
        var postcode = Faker.Address.UkPostCode();
        var establishment1 = await _dataScope.TestData.CreateAccount(x =>
        {
            x.WithName("SomeAccountName");
        });

        var createPersonResult = await _dataScope.TestData.CreatePerson(x =>
        {
            x.WithQts(qtlsDate);
            x.WithInduction(inductionStatus, exemptionReason, null, null, null);
        });

        // Act
        var person = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactDetailByIdQuery(createPersonResult.PersonId, ColumnSet: new(
                    Contact.Fields.dfeta_InductionStatus,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_qtlsdate)));

        // Assert
        Assert.Equal(expectedInductionStatus, person!.Contact.dfeta_InductionStatus);
    }

    [Fact]
    public async Task WhenCalled_WithoutTRAInductionStatusAndQTLSDate_ReturnsExpectedResults()
    {
        // Arrange
        var qtlsDate = DateOnly.Parse("01/01/2021");
        var postcode = Faker.Address.UkPostCode();
        var establishment1 = await _dataScope.TestData.CreateAccount(x =>
        {
            x.WithName("SomeAccountName");
        });

        var createPersonResult = await _dataScope.TestData.CreatePerson();

        // Act
        var person = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactDetailByIdQuery(createPersonResult.PersonId, ColumnSet: new(
                    Contact.Fields.dfeta_InductionStatus,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_qtlsdate)));

        // Assert
        Assert.Null(person!.Contact.dfeta_InductionStatus);
        Assert.Null(person!.Contact.dfeta_QTSDate);
        Assert.Null(person!.Contact.dfeta_qtlsdate);
    }

    [Fact]
    public async Task WhenCalled_WithQTLSDate_ReturnsExpectedResults()
    {
        // Arrange
        var qtlsDate = DateOnly.Parse("01/01/2021");
        var postcode = Faker.Address.UkPostCode();
        var establishment1 = await _dataScope.TestData.CreateAccount(x =>
        {
            x.WithName("SomeAccountName");
        });

        var createPersonResult = await _dataScope.TestData.CreatePerson(x =>
        {
            x.WithQtlsDate(qtlsDate);
        });

        // Act
        var person = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactDetailByIdQuery(createPersonResult.PersonId, ColumnSet: new(
                    Contact.Fields.dfeta_InductionStatus,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_qtlsdate)));

        // Assert
        Assert.Equal(dfeta_InductionStatus.Exempt, person!.Contact.dfeta_InductionStatus);
        Assert.Equal(qtlsDate, person!.Contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(qtlsDate, person!.Contact.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }
}

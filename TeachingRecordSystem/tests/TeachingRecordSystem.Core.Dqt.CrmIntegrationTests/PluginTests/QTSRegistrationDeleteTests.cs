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

    public Task InitializeAsync() => DbFixture.DbHelper.EnsureSchema();

    //trigger from qtls contact updates
    //trigger from qtls contact create
    //trigger from qtsregistration create
    //trigger from qtsregistration update
}

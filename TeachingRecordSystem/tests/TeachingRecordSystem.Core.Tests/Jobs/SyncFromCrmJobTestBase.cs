using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public abstract class SyncFromCrmJobTestBase : IClassFixture<SyncFromCrmJobFixture>
{
    public SyncFromCrmJobTestBase(SyncFromCrmJobFixture jobFixture)
    {
        JobFixture = jobFixture;
    }

    public SyncFromCrmJobFixture JobFixture { get; }

    protected TestableClock Clock => JobFixture.Clock;

    protected ILoggerFactory LoggerFactory => JobFixture.LoggerFactory;

    protected DbFixture DbFixture => JobFixture.DbFixture;

    protected TrsDataSyncHelper Helper => JobFixture.Helper;

    protected TestData TestData => JobFixture.TestData;

    public ICrmServiceClientProvider CrmServiceClientProvider => JobFixture.CrmServiceClientProvider;
}

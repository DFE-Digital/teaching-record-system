using System.Diagnostics;
using System.Net.Http.Headers;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests;

public sealed class CrmClientFixture : IDisposable
{
    private readonly ServiceClient _baseServiceClient;
    private readonly CancellationTokenSource _completedCts;
    private readonly EnvironmentLockManager _lockManager;
    private readonly IMemoryCache _memoryCache;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ITrnGenerator _trnGenerationApiClient;
    private readonly ReferenceDataCache _referenceDataCache;

    public CrmClientFixture(ServiceClient serviceClient, DbFixture dbFixture, IConfiguration configuration, IMemoryCache memoryCache, ILoggerFactory loggerFactory)
    {
        Clock = new Clock();
        Configuration = configuration;
        _baseServiceClient = serviceClient;
        DbFixture = dbFixture;
        _completedCts = new CancellationTokenSource();
        _lockManager = new EnvironmentLockManager(Configuration);
        _lockManager.AcquireLock(_completedCts.Token);
        _memoryCache = memoryCache;
        _loggerFactory = loggerFactory;
        _trnGenerationApiClient = GetTrnGenerationApiClient();
        _referenceDataCache = new ReferenceDataCache(
            new CrmQueryDispatcher(CreateQueryServiceProvider(_baseServiceClient, referenceDataCache: null), serviceClientName: null),
            dbFixture.GetDbContextFactory());
    }

    public IClock Clock { get; }

    public IConfiguration Configuration { get; }

    public DbFixture DbFixture { get; }

    public CrmQueryDispatcher CreateQueryDispatcher() =>
        new CrmQueryDispatcher(CreateQueryServiceProvider(_baseServiceClient, _referenceDataCache), serviceClientName: null);

    /// <summary>
    /// Creates a scope that owns an implementation of <see cref="IOrganizationServiceAsync2"/> that tracks the entities created through it.
    /// When <see cref="IAsyncDisposable.DisposeAsync"/> is called the created entities will be deleted from CRM.
    /// </summary>
    public TestDataScope CreateTestDataScope(bool withSync = false)
    {
        var dbContext = DbFixture.GetDbContextFactory().CreateDbContext();
        var onAsyncDispose = () => dbContext.DisposeAsync();

        return new(
            _baseServiceClient,
            orgService => new DataverseAdapter(orgService, Clock, _memoryCache, _trnGenerationApiClient, dbContext),
            orgService => new CrmQueryDispatcher(CreateQueryServiceProvider(orgService, _referenceDataCache), serviceClientName: null),
            orgService => TestData.CreateWithCustomTrnGeneration(
                DbFixture.GetDbContextFactory(),
                orgService,
                _referenceDataCache,
                Clock,
                () => _trnGenerationApiClient.GenerateTrnAsync(),
                withSync ? TestDataSyncConfiguration.Sync(new(DbFixture.GetDataSource(), orgService, _referenceDataCache, Clock, new TestableAuditRepository(), _loggerFactory.CreateLogger<TrsDataSyncHelper>())) : TestDataSyncConfiguration.NoSync()),
            _memoryCache,
            onAsyncDispose);
    }

    public void Dispose()
    {
        _baseServiceClient.Dispose();
        _completedCts.Cancel();
    }

    private static IServiceProvider CreateQueryServiceProvider(IOrganizationServiceAsync organizationService, ReferenceDataCache? referenceDataCache)
    {
        var services = new ServiceCollection();
        services.AddCrmQueries();
        services.AddSingleton(organizationService);

        if (referenceDataCache is not null)
        {
            services.AddSingleton(referenceDataCache);
        }

        return services.BuildServiceProvider();
    }

    private ITrnGenerator GetTrnGenerationApiClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(Configuration.GetRequiredValue("TrnGenerationApi:BaseAddress"))
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Configuration["TrnGenerationApi:ApiKey"]);
        return new ApiTrnGenerator(httpClient);
    }

    public sealed class TestDataScope : IAsyncDisposable
    {
        private readonly Func<IOrganizationServiceAsync2, DataverseAdapter> _createDataverseAdapter;
        private readonly Func<IOrganizationServiceAsync2, CrmQueryDispatcher> _createCrmQueryDispatcher;
        private readonly IMemoryCache _memoryCache;
        private readonly Func<ValueTask> _onAsyncDispose;

        internal TestDataScope(
            ServiceClient serviceClient,
            Func<IOrganizationServiceAsync2, DataverseAdapter> createDataverseAdapter,
            Func<IOrganizationServiceAsync2, CrmQueryDispatcher> createCrmQueryDispatcher,
            Func<IOrganizationServiceAsync2, TestData> createTestData,
            IMemoryCache memoryCache,
            Func<ValueTask> onAsyncDispose)
        {
            _createDataverseAdapter = createDataverseAdapter;
            _createCrmQueryDispatcher = createCrmQueryDispatcher;
            _memoryCache = memoryCache;
            _onAsyncDispose = onAsyncDispose;

            OrganizationService = EntityTrackingOrganizationService.CreateProxy(serviceClient);
            TestData = createTestData(OrganizationService);
        }

        public ITrackedEntityOrganizationService OrganizationService { get; }

        public TestData TestData { get; }

        public CrmQueryDispatcher CreateQueryDispatcher() => _createCrmQueryDispatcher(OrganizationService);

        public DataverseAdapter CreateDataverseAdapter() => _createDataverseAdapter(OrganizationService);

        public TestDataHelper CreateTestDataHelper() => new TestDataHelper(this, _memoryCache);

        public async ValueTask DisposeAsync()
        {
            await OrganizationService.DisposeAsync();
            await _onAsyncDispose();
        }
    }

    private class EnvironmentLockManager
    {
        private static readonly TimeSpan _lockAcquireWaitTime = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan _lockAcquireRetryDelay = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan _lockRenewalInterval = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan _lockRenewalBuffer = TimeSpan.FromSeconds(10);

        private readonly BlobClient _blobClient;

        public EnvironmentLockManager(IConfiguration configuration)
        {
            var blobUri = new Uri(configuration.GetRequiredValue("BuildEnvLockBlobUri"));
            var sasToken = configuration.GetRequiredValue("BuildEnvLockBlobSasToken");

            _blobClient = new BlobClient(blobUri, new AzureSasCredential(sasToken));
        }

        public void AcquireLock(CancellationToken cancellationToken)
        {
            Debug.Assert(_lockRenewalBuffer < _lockRenewalInterval);

            var leaseClient = _blobClient.GetBlobLeaseClient();

            var acquireSw = Stopwatch.StartNew();

            while (true)
            {
                try
                {
                    leaseClient.Acquire(_lockRenewalInterval, cancellationToken: cancellationToken);
                    break;
                }
                catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseAlreadyPresent" && acquireSw.Elapsed < _lockAcquireWaitTime)
                {
                    Thread.Sleep(_lockAcquireRetryDelay);
                }
            }

            cancellationToken.Register(() => leaseClient.Release());

            _ = Task.Run(async () =>
            {
                await Task.Delay(_lockRenewalInterval - _lockRenewalBuffer, cancellationToken);
                await RenewLock();

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(_lockRenewalInterval, cancellationToken);
                    await RenewLock();
                }

                Task RenewLock() => leaseClient.RenewAsync();
            }, cancellationToken);
        }
    }
}

using System.Diagnostics;
using System.Net.Http.Headers;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Services.TrnGenerationApi;

namespace TeachingRecordSystem.Core.Dqt.Tests;

public sealed class CrmClientFixture : IDisposable
{
    private readonly ServiceClient _baseServiceClient;
    private readonly CancellationTokenSource _completedCts;
    private readonly EnvironmentLockManager _lockManager;
    private readonly IMemoryCache _memoryCache;
    private readonly ITrnGenerationApiClient _trnGenerationApiClient;
    private readonly ReferenceDataCache _referenceDataCache;

    public CrmClientFixture(ServiceClient serviceClient, IConfiguration configuration, IMemoryCache memoryCache)
    {
        Clock = new();
        Configuration = configuration;
        _baseServiceClient = serviceClient;
        _completedCts = new CancellationTokenSource();
        _lockManager = new EnvironmentLockManager(Configuration);
        _lockManager.AcquireLock(_completedCts.Token);
        _memoryCache = memoryCache;
        _trnGenerationApiClient = GetTrnGenerationApiClient();
        _referenceDataCache = new ReferenceDataCache(new CrmQueryDispatcher(CreateQueryServiceProvider(_baseServiceClient, referenceDataCache: null)));
    }

    public TestableClock Clock { get; }

    public IConfiguration Configuration { get; }

    public CrmQueryDispatcher CreateQueryDispatcher() =>
        new CrmQueryDispatcher(CreateQueryServiceProvider(_baseServiceClient, _referenceDataCache));

    /// <summary>
    /// Creates a scope that owns an implementation of <see cref="IOrganizationServiceAsync2"/> that tracks the entities created through it.
    /// When <see cref="IAsyncDisposable.DisposeAsync"/> is called the created entities will be deleted from CRM.
    /// </summary>
    public TestDataScope CreateTestDataScope() => new(
        _baseServiceClient,
        orgService => new DataverseAdapter(orgService, Clock, _memoryCache, _trnGenerationApiClient),
        orgService => new CrmQueryDispatcher(CreateQueryServiceProvider(orgService, _referenceDataCache)),
        _memoryCache);

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

    private ITrnGenerationApiClient GetTrnGenerationApiClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(Configuration.GetRequiredValue("TrnGenerationApi:BaseAddress"))
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Configuration["TrnGenerationApi:ApiKey"]);
        return new TrnGenerationApiClient(httpClient);
    }

    public sealed class TestDataScope : IAsyncDisposable
    {
        private readonly Func<IOrganizationServiceAsync2, DataverseAdapter> _createDataverseAdapter;
        private readonly Func<IOrganizationServiceAsync2, CrmQueryDispatcher> _createCrmQueryDispatcher;
        private readonly IMemoryCache _memoryCache;

        internal TestDataScope(
            ServiceClient serviceClient,
            Func<IOrganizationServiceAsync2, DataverseAdapter> createDataverseAdapter,
            Func<IOrganizationServiceAsync2, CrmQueryDispatcher> createCrmQueryDispatcher,
            IMemoryCache memoryCache)
        {
            _createDataverseAdapter = createDataverseAdapter;
            _createCrmQueryDispatcher = createCrmQueryDispatcher;
            _memoryCache = memoryCache;

            OrganizationService = EntityTrackingOrganizationService.CreateProxy(serviceClient);
            TestData = new CrmTestData(OrganizationService);
        }

        public ITrackedEntityOrganizationService OrganizationService { get; }

        public CrmTestData TestData { get; }

        public CrmQueryDispatcher CreateQueryDispatcher() => _createCrmQueryDispatcher(OrganizationService);

        public DataverseAdapter CreateDataverseAdapter() => _createDataverseAdapter(OrganizationService);

        public TestDataHelper CreateTestDataHelper() => new TestDataHelper(this, _memoryCache);

        public ValueTask DisposeAsync() => OrganizationService.DisposeAsync();
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

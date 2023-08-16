#nullable disable
using System.Diagnostics;
using System.Net.Http.Headers;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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
    }

    public TestableClock Clock { get; }

    public IConfiguration Configuration { get; }

    /// <summary>
    /// Creates a scope that owns an implementation of <see cref="IOrganizationServiceAsync2"/> that tracks the entities created through it.
    /// When <see cref="IAsyncDisposable.DisposeAsync"/> is called the created entities will be deleted from CRM.
    /// </summary>
    public TestDataScope CreateTestDataScope() => new(
        _baseServiceClient,
        orgService => new DataverseAdapter(orgService, Clock, _memoryCache, _trnGenerationApiClient),
        _memoryCache);

    public void Dispose()
    {
        _baseServiceClient.Dispose();
        _completedCts.Cancel();
    }

    private ITrnGenerationApiClient GetTrnGenerationApiClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(Configuration["TrnGenerationApi:BaseAddress"])
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Configuration["TrnGenerationApi:ApiKey"]);
        return new TrnGenerationApiClient(httpClient);
    }

    public sealed class TestDataScope : IAsyncDisposable
    {
        private readonly Func<IOrganizationServiceAsync2, DataverseAdapter> _createDataverseAdapter;
        private readonly IMemoryCache _memoryCache;

        internal TestDataScope(
            ServiceClient serviceClient,
            Func<IOrganizationServiceAsync2, DataverseAdapter> createDataverseAdapter,
            IMemoryCache memoryCache)
        {
            OrganizationService = EntityTrackingOrganizationService.CreateProxy(serviceClient);

            _createDataverseAdapter = createDataverseAdapter;
            _memoryCache = memoryCache;
        }

        public ITrackedEntityOrganizationService OrganizationService { get; }

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
            var blobUri = new Uri(configuration["BuildEnvLockBlobUri"]);
            var sasToken = configuration["BuildEnvLockBlobSasToken"];

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

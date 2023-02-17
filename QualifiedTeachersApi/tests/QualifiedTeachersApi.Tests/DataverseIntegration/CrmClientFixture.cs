using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using Microsoft.PowerPlatform.Dataverse.Client;
using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.Services.TrnGenerationApi;

namespace QualifiedTeachersApi.Tests.DataverseIntegration
{
    public sealed class CrmClientFixture : IDisposable
    {
        private readonly ServiceClient _baseServiceClient;
        private readonly CancellationTokenSource _completedCts;
        private readonly EnvironmentLockManager _lockManager;
        private readonly IMemoryCache _memoryCache;
        private readonly IFeatureManager _featureIsEnabledFeatureManager;
        private readonly IFeatureManager _featureIsNotEnabledFeatureManager;
        private readonly ITrnGenerationApiClient _realTrnGenerationApiClient;
        private readonly ITrnGenerationApiClient _noopTrnGenerationApiClient;

        public CrmClientFixture(IMemoryCache memoryCache)
        {
            Clock = new();
            Configuration = GetConfiguration();
            _baseServiceClient = GetCrmServiceClient();

            _featureIsEnabledFeatureManager = GetFeatureManager(true);
            _featureIsNotEnabledFeatureManager = GetFeatureManager(false);
            _realTrnGenerationApiClient = GetTrnGenerationApiClient(true);
            _noopTrnGenerationApiClient = GetTrnGenerationApiClient(false);

            _completedCts = new CancellationTokenSource();
            _lockManager = new EnvironmentLockManager(Configuration);
            _lockManager.AcquireLock(_completedCts.Token);
            _memoryCache = memoryCache;
        }

        public TestableClock Clock { get; }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// Creates a scope that owns an implementation of <see cref="IOrganizationServiceAsync2"/> that tracks the entities created through it.
        /// When <see cref="IAsyncDisposable.DisposeAsync"/> is called the created entities will be deleted from CRM.
        /// </summary>
        public TestDataScope CreateTestDataScope(bool useTrnGenerationApi = false) => new(
            _baseServiceClient,
            orgService => new DataverseAdapter(
                orgService,
                Clock,
                _memoryCache,
                useTrnGenerationApi ? _featureIsEnabledFeatureManager : _featureIsNotEnabledFeatureManager,
                useTrnGenerationApi ? _realTrnGenerationApiClient : _noopTrnGenerationApiClient),
            _memoryCache);

        public void Dispose()
        {
            _baseServiceClient.Dispose();
            _completedCts.Cancel();
        }

        private static IConfiguration GetConfiguration() =>
            new ConfigurationBuilder()
                .AddUserSecrets<CrmClientFixture>(optional: true)
                .AddEnvironmentVariables("IntegrationTests_")
                .Build();

        // This is wrapped up in Task.Run because the ServiceClient constructor can deadlock in some environments (e.g. CI).
        // InitServiceAsync().Result within Microsoft.PowerPlatform.Dataverse.Client.ConnectionService.GetCachedService() looks to be the culprit
        private ServiceClient GetCrmServiceClient() => Task.Run(() =>
            new ServiceClient(
                new Uri(Configuration["CrmUrl"]),
                Configuration["CrmClientId"],
                Configuration["CrmClientSecret"],
                useUniqueInstance: true)).Result;

        private IFeatureManager GetFeatureManager(bool isUseTrnGenerationApiEnabled)
        {
            var featureManager = Mock.Of<IFeatureManager>();
            Mock.Get(featureManager)
                .Setup(f => f.IsEnabledAsync(FeatureFlags.UseTrnGenerationApi))
                .ReturnsAsync(isUseTrnGenerationApiEnabled);
            return featureManager;
        }

        private ITrnGenerationApiClient GetTrnGenerationApiClient(bool isUseTrnGenerationApiEnabled)
        {
            ITrnGenerationApiClient trnGenerationApiClient;
            if (isUseTrnGenerationApiEnabled)
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(Configuration["TrnGenerationApi:BaseAddress"]);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Configuration["TrnGenerationApi:ApiKey"]);
                trnGenerationApiClient = new TrnGenerationApiClient(httpClient);
            }
            else
            {
                trnGenerationApiClient = new NoopTrnGenerationApiClient();
            }

            return trnGenerationApiClient;
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
}

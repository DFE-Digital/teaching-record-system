using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using DqtApi.DataStore.Crm;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace DqtApi.Tests.DataverseIntegration
{
    public sealed class CrmClientFixture : IAsyncDisposable
    {
        private readonly EntityCleanupHelper _createdEntityTracker;
        private readonly CancellationTokenSource _completedCts;
        private readonly EnvironmentLockManager _lockManager;

        public CrmClientFixture()
        {
            Clock = new();
            Configuration = GetConfiguration();
            ServiceClient = GetCrmServiceClient();
            _createdEntityTracker = CreateEntityCleanupHelper();

            _completedCts = new CancellationTokenSource();
            _lockManager = new EnvironmentLockManager(Configuration);
            _lockManager.AcquireLock(_completedCts.Token);
        }

        public TestableClock Clock { get; }

        public IConfiguration Configuration { get; }

        public ServiceClient ServiceClient { get; }

        public DataverseAdapter CreateDataverseAdapter() => new(ServiceClient, Clock, new MemoryCache(Options.Create<MemoryCacheOptions>(new())));

        public EntityCleanupHelper CreateEntityCleanupHelper() => new(ServiceClient);

        public async ValueTask DisposeAsync()
        {
            await _createdEntityTracker.CleanupEntities();
            ServiceClient.Dispose();
            _completedCts.Cancel();
        }

        public void RegisterForCleanup(Entity entity) =>
            _createdEntityTracker.RegisterForCleanup(entity);

        public void RegisterForCleanup(string entityName, Guid entityId) =>
            _createdEntityTracker.RegisterForCleanup(entityName, entityId);

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

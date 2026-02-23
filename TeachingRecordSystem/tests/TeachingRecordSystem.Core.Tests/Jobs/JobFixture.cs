using Azure.Storage.Files.DataLake;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Tests.Jobs;

[assembly: AssemblyFixture(typeof(JobFixture))]

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class JobFixture : ServiceProviderFixture
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddLogging()
            .AddMemoryCache()
            .AddSingleton<TestData>()
            .AddSingleton<ReferenceDataCache>()
            .AddEventPublisher()
            .AddWebhookMessageFactory()
            .AddPersonService()
            .AddSupportTaskService()
            .AddTrnRequestService(configuration)
            .AddSingleton<IGetAnIdentityApiClient>(_ => Mock.Of<IGetAnIdentityApiClient>())
            .AddSingleton(Options.Create(new AccessYourTeachingQualificationsOptions { BaseAddress = "https://aytq.example.com/" }))
            .AddSingleton(Options.Create(new TrnRequestOptions()))
            .AddSingleton(Options.Create(new CapitaTpsUserOption { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId }))
            .AddKeyedSingleton<DataLakeServiceClient>("sftpstorage", (_, _) => Mock.Of<DataLakeServiceClient>());

        TestScopedServices.ConfigureServices(services);
    }
}

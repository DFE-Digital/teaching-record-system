using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Tests.Services;
using TeachingRecordSystem.Core.Tests.Services.Webhooks;

[assembly: AssemblyFixture(typeof(ServiceFixture))]

namespace TeachingRecordSystem.Core.Tests.Services;

public class ServiceFixture : ServiceProviderFixture
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdDbContext>(options => options.UseInMemoryDatabase("TeacherAuthId"), contextLifetime: ServiceLifetime.Transient);

        services
            .AddMemoryCache()
            .AddSingleton<TestData>()
            .AddSingleton<ReferenceDataCache>()
            .AddSingleton<PersonInfoCache>()
            .AddWebhookMessageFactory()
            .AddEventPublisher()
            .AddOneLoginService()
            .AddSupportTaskService()
            .AddPersonService()
            .AddTransient<TrnRequestService>()
            .AddSingleton<IGetAnIdentityApiClient>(new NoopGetAnIdentityApiClient())
            .AddSingleton<IOptions<AccessYourTeachingQualificationsOptions>>(Options.Create(new AccessYourTeachingQualificationsOptions { BaseAddress = "http://localhost" }))
            .AddSingleton<IOptions<TrnRequestOptions>>(Options.Create(new TrnRequestOptions()))
            .AddSingleton<INotificationSender, NoopNotificationSender>()
            .AddTestTrnGeneration()
            .AddSingleton<WebhookReceiver>();

        TestScopedServices.ConfigureServices(services);
    }

    private class NoopGetAnIdentityApiClient : IGetAnIdentityApiClient
    {
        public Task<TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models.User?> GetUserByIdAsync(Guid userId) =>
            Task.FromResult<TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models.User?>(null);

        public Task<CreateTrnTokenResponse> CreateTrnTokenAsync(CreateTrnTokenRequest request) =>
            Task.FromResult(new CreateTrnTokenResponse { TrnToken = Guid.NewGuid().ToString(), Email = request.Email, Trn = request.Trn, ExpiresUtc = DateTime.UtcNow.AddHours(1) });

        public Task SetTeacherTrnAsync(Guid userId, string trn) =>
            Task.CompletedTask;
    }
}

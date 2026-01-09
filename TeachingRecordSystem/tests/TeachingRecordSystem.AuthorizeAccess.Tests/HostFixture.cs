using System.Security.Cryptography;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using TeachingRecordSystem.AuthorizeAccess.Tests;
using TeachingRecordSystem.AuthorizeAccess.Tests.Infrastructure.Security;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.TestCommon.Infrastructure;
using TeachingRecordSystem.UiTestCommon.Infrastructure.FormFlow;
using TeachingRecordSystem.WebCommon.FormFlow.State;

[assembly: AssemblyFixture(typeof(HostFixture))]

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public class HostFixture : InitializeDbFixture
{
    private readonly AuthorizeAccessWebApplicationFactory _webApplicationFactory;

    public HostFixture()
    {
        _webApplicationFactory = new();
    }

    public IServiceProvider Services => _webApplicationFactory.Services;

    public HttpClient CreateClient() => _webApplicationFactory.CreateClient();

    public HttpClient CreateClient(WebApplicationFactoryClientOptions options) => _webApplicationFactory.CreateClient(options);

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        _ = Services;  // Start the server
    }

    private class AuthorizeAccessWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Tests");

            // N.B. Don't use builder.ConfigureAppConfiguration here since it runs *after* the entry point
            // i.e. Program.cs and that has a dependency on IConfiguration
            var configuration = TestConfiguration.GetConfiguration();
            builder.UseConfiguration(configuration);

            builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<IdDbContext>(options => options.UseInMemoryDatabase("TeacherAuthId"), contextLifetime: ServiceLifetime.Transient);

                services
                    .Configure<AuthenticationOptions>(options =>
                    {
                        options.AddScheme(OneLoginDefaults.AuthenticationScheme, b => b.HandlerType = typeof(DummyOneLoginHandler));
                    })
                    .AddSingleton<DummyOneLoginHandler>();

                // Remove the built-in antiforgery filters
                // (we want to be able to POST directly from a test without having to set antiforgery cookies etc.)
                services.AddSingleton<IPageApplicationModelProvider, RemoveAutoValidateAntiforgeryPageApplicationModelProvider>();

                // Publish events synchronously
                PublishEventsDbCommandInterceptor.ConfigureServices(services);

                services.Configure<AuthorizeAccessOptions>(options =>
                {
                    using var rsa = RSA.Create(keySizeInBits: 2048);
                    var privateKeyPem = rsa.ExportRSAPrivateKeyPem();

                    options.OneLoginSigningKeys =
                    [
                        new AuthorizeAccessOptionsOneLoginSigningKey
                        {
                            KeyId = "test-key-1",
                            PrivateKeyPem = privateKeyPem
                        }
                    ];
                });

                services
                    .AddSingleton(DbHelper.Instance)
                    .AddSingleton<TestData>()
                    .AddSingleton<IUserInstanceStateProvider, InMemoryInstanceStateProvider>()
                    .AddSingleton(GetMockFileService())
                    .AddSingleton<IStartupFilter, ExecuteScheduledJobsStartupFilter>();

                TestScopedServices.ConfigureServices(services);

                IFileService GetMockFileService()
                {
                    var fileService = new Mock<IFileService>();
                    fileService
                        .Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string?>(), null))
                        .ReturnsAsync(Guid.NewGuid());
                    fileService
                        .Setup(s => s.GetFileUrlAsync(It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
                        .ReturnsAsync("https://fake.blob.core.windows.net/fake");
                    fileService
                        .Setup(s => s.OpenReadStreamAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(() => new MemoryStream(TestData.JpegImage));
                    return fileService.Object;
                }
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Ensure we can flow AsyncLocals from tests to the server
            builder.ConfigureServices(services => services.Configure<TestServerOptions>(o => o.PreserveExecutionContext = true));

            return base.CreateHost(builder);
        }
    }

    private class RemoveAutoValidateAntiforgeryPageApplicationModelProvider : IPageApplicationModelProvider
    {
        public int Order => int.MaxValue;

        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
        }

        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {
            var pageApplicationModel = context.PageApplicationModel;

            var autoValidateAttribute = pageApplicationModel.Filters.OfType<AutoValidateAntiforgeryTokenAttribute>().SingleOrDefault();
            if (autoValidateAttribute is not null)
            {
                pageApplicationModel.Filters.Remove(autoValidateAttribute);
            }
        }
    }

    private class ExecuteScheduledJobsStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                app.Use(async (_, next) =>
                {
                    await next();

                    await TestScopedServices.GetCurrent().BackgroundJobScheduler.ExecuteDeferredJobsAsync();
                });

                next(app);
            };
    }
}

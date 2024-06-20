using Microsoft.AspNetCore.TestHost;
using TeachingRecordSystem.UiCommon.FormFlow;
using TeachingRecordSystem.UiCommon.FormFlow.State;
using TeachingRecordSystem.UiCommon.FormFlow.Tests;

namespace TeachingRecordSystem.UiCommon.Tests.FormFlow.Infrastructure;

public sealed class MvcTestFixture : IDisposable
{
    private readonly IHost _host;

    public MvcTestFixture()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices((ctx, services) =>
                    {
                        services.AddMvc();

                        services.AddFormFlow(options =>
                        {
                            options.JourneyRegistry.RegisterJourney(new JourneyDescriptor(
                                journeyName: "MissingInstanceActionFilterTests",
                                stateType: typeof(MissingInstanceActionFilterTestsState),
                                requestDataKeys: new[] { "id" },
                                appendUniqueKey: false));

                            options.JourneyRegistry.RegisterJourney(new JourneyDescriptor(
                                journeyName: "E2ETests",
                                stateType: typeof(E2ETestsState),
                                requestDataKeys: new[] { "id", "subid" },
                                appendUniqueKey: true));
                        });

                        services.AddSingleton<IUserInstanceStateProvider, InMemoryInstanceStateProvider>();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
            })
            .StartAsync().GetAwaiter().GetResult();

        Services = _host.Services;
        HttpClient = _host.GetTestClient();
    }

    public IServiceProvider Services { get; }

    public HttpClient HttpClient { get; }

    public void Dispose()
    {
        HttpClient.Dispose();
        _host.Dispose();
    }
}

[CollectionDefinition("Mvc")]
public class MvcTestCollection : ICollectionFixture<MvcTestFixture> { }

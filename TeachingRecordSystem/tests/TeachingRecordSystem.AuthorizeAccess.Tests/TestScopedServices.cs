using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices()
    {
        Clock = new();
        EventObserver = new();
        GetAnIdentityApiClient = new();
        BlobStorageFileService = new();
    }

    public static void ConfigureServices(IServiceCollection services) =>
        services
            .AddTestScoped<IClock>(tss => tss.Clock)
            .AddTestScoped(tss => tss.GetAnIdentityApiClient.Object);

    public static TestScopedServices GetCurrent() =>
        _current.Value ?? throw new InvalidOperationException("No current instance has been set.");

    public static TestScopedServices Reset()
    {
        if (_current.Value is not null)
        {
            throw new InvalidOperationException("Current instance has already been set.");
        }

        return _current.Value = new();
    }

    public TestableClock Clock { get; }

    public CaptureEventObserver EventObserver { get; }

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClient { get; }

    public Mock<IFileService> BlobStorageFileService { get; }
}

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestScoped<T>(this IServiceCollection services, Func<TestScopedServices, T> resolveService)
        where T : class
    {
        return services.AddTransient(_ => resolveService(TestScopedServices.GetCurrent()));
    }
}

using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure;

namespace TeachingRecordSystem.SupportUi.Tests;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices(IServiceProvider serviceProvider)
    {
        Clock = new();
        DataverseAdapterMock = new();
        AzureActiveDirectoryUserServiceMock = new();
        EventObserver = new();
        FeatureProvider = ActivatorUtilities.CreateInstance<TestableFeatureProvider>(serviceProvider);
        BlobStorageFileServiceMock = new();
    }

    public static TestScopedServices GetCurrent() =>
        _current.Value ?? throw new InvalidOperationException("No current instance has been set.");

    public static TestScopedServices Reset(IServiceProvider serviceProvider)
    {
        if (_current.Value is not null)
        {
            throw new InvalidOperationException("Current instance has already been set.");
        }

        return _current.Value = new(serviceProvider);
    }

    public TestableClock Clock { get; }

    public Mock<IDataverseAdapter> DataverseAdapterMock { get; }

    public Mock<IAadUserService> AzureActiveDirectoryUserServiceMock { get; }

    public CaptureEventObserver EventObserver { get; }

    public TestableFeatureProvider FeatureProvider { get; }

    public Mock<IFileService> BlobStorageFileServiceMock { get; }
}

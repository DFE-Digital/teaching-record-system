using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure;

namespace TeachingRecordSystem.SupportUi.Tests;

public class TestScopedServices
{
    public const string FakeBlobStorageFileUrlBase = "https://fake.blob.core.windows.net/";

    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices(IServiceProvider serviceProvider)
    {
        Clock = new();
        DataverseAdapterMock = new();
        AzureActiveDirectoryUserServiceMock = new();
        EventObserver = new();
        FeatureProvider = ActivatorUtilities.CreateInstance<TestableFeatureProvider>(serviceProvider);
        BlobStorageFileServiceMock = new();
        BlobStorageFileServiceMock
            .Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string?>(), null))
            .ReturnsAsync(Guid.NewGuid());
        BlobStorageFileServiceMock
            .Setup(s => s.GetFileUrlAsync(It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync((Guid id, TimeSpan time) => $"{FakeBlobStorageFileUrlBase}{id}");
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

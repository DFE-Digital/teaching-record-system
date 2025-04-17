using TeachingRecordSystem.Core.Services.DqtNoteAttachments;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using Xunit.Sdk;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices()
    {
        Clock = new();
        EventObserver = new();
        GetAnIdentityApiClient = new();
        DqtNoteFileAttachmentStorageMock = new();
    }

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

    public Mock<IDqtNoteAttachmentStorage> DqtNoteFileAttachmentStorageMock { get; }
}

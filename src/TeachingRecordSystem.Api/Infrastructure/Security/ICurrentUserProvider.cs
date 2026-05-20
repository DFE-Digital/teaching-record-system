using System.Diagnostics.CodeAnalysis;

namespace TeachingRecordSystem.Api.Infrastructure.Security;

public interface ICurrentUserProvider
{
    Guid GetCurrentApplicationUserId();
    bool TryGetTrnRequestId([NotNullWhen(true)] out string? trnRequestId);
    Task<string?> GetTrnAsync();
}

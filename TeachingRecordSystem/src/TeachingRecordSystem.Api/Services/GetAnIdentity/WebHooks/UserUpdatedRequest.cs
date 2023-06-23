using MediatR;
using Optional;

namespace TeachingRecordSystem.Api.Services.GetAnIdentity.WebHooks;

public class UserUpdatedRequest : IRequest
{
    public required Guid UserId { get; init; }
    public required string? Trn { get; init; }
    public required string EmailAddress { get; init; }
    public required string? MobileNumber { get; init; }
    public required DateTime UpdateTimeUtc { get; init; }
    public required Option<string?> ChangedTrn { get; init; }
}

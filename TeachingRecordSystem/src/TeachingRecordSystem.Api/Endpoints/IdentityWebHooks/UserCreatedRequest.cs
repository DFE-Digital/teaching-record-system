using MediatR;

namespace TeachingRecordSystem.Api.Endpoints.IdentityWebHooks;

public class UserCreatedRequest : IRequest
{
    public required Guid UserId { get; set; }
    public required string? Trn { get; init; }
    public required string EmailAddress { get; set; }
    public required string? MobileNumber { get; set; }
    public required DateTime UpdateTimeUtc { get; set; }
}

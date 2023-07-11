namespace TeachingRecordSystem.Api.Endpoints.IdentityWebHooks.Messages;

public record User
{
    public required Guid UserId { get; init; }
    public required string EmailAddress { get; init; }
    public required string? Trn { get; init; }
    public required string? MobileNumber { get; init; }
}

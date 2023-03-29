using System;

namespace QualifiedTeachersApi.Services.GetAnIdentity.WebHooks;

public record User
{
    public required Guid UserId { get; init; }
    public required string EmailAddress { get; init; }
    public string Trn { get; init; }
    public string MobileNumber { get; init; }
}

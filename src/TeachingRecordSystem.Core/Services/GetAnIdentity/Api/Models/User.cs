namespace TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;

public class User
{
    public required Guid UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}

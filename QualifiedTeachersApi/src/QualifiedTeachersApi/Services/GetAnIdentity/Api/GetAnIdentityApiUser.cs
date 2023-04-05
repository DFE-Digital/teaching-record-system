#nullable disable
using System;

namespace QualifiedTeachersApi.Services.GetAnIdentityApi;

public class GetAnIdentityApiUser
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
}

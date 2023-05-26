namespace QualifiedTeachersApi.Services.GetAnIdentity.Api.Models;

public record CreateTrnTokenRequest
{
    public required string Trn { get; init; }

    public required string Email { get; init; }
}

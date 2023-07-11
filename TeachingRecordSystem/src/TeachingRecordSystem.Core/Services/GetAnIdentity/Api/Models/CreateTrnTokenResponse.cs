namespace TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;

public record CreateTrnTokenResponse
{
    public required string Trn { get; init; }
    public required string Email { get; set; }
    public required string TrnToken { get; set; }
    public required DateTime ExpiresUtc { get; set; }
}

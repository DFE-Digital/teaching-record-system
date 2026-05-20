namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class AuthzRegistrationToken
{
    public required string Token { get; set; }

    public required string Trn { get; set; }

    public required string EmailAddress { get; set; }

    public required DateTime CreatedUtc { get; set; }

    public required DateTime ExpiresUtc { get; set; }

    public bool IsActive { get; set; } = true;
}

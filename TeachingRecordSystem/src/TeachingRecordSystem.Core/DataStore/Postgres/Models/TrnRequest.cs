using System.Text.RegularExpressions;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TrnRequest
{
    public const int RequestIdMaxLength = 100;
    public const int TrnTokenMaxLength = 128;

    public static Regex ValidRequestIdPattern { get; } = new Regex(@"^([0-9]|[a-z]|\-|_)+$", RegexOptions.IgnoreCase);

    public long TrnRequestId { get; set; }
    public required string ClientId { get; set; }
    public required string RequestId { get; set; }
    public required Guid TeacherId { get; set; }
    public Guid? IdentityUserId { get; set; }
    public bool LinkedToIdentity { get; set; }
    public string? TrnToken { get; set; }
}

#nullable disable
using System;
using System.Text.RegularExpressions;

namespace QualifiedTeachersApi.DataStore.Sql.Models;

public class TrnRequest
{
    public const int RequestIdMaxLength = 100;

    public static Regex ValidRequestIdPattern { get; } = new Regex(@"^([0-9]|[a-z]|\-|_)+$", RegexOptions.IgnoreCase);

    public long TrnRequestId { get; set; }
    public string ClientId { get; set; }
    public string RequestId { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid? IdentityUserId { get; set; }
    public bool LinkedToIdentity { get; set; }
}

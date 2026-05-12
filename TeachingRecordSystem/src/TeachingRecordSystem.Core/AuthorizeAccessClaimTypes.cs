namespace TeachingRecordSystem.Core;

public static class AuthorizeAccessClaimTypes
{
    public const string Email = "email";
    public const string Subject = "sub";
    public const string Trn = "trn";
    public const string VerifiedName = "verified_name";
    public const string VerifiedDateOfBirth = "verified_date_of_birth";

    // Internal-only claims
    public const string OneLoginIdToken = "_ta_olidt";
    public const string TrsApplicationUserId = "_ta_auid";
    public const string TrnRequestId = "_ta_trid";
}

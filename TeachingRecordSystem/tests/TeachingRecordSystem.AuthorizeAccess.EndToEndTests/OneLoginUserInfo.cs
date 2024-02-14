using System.Text.Json;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

#pragma warning disable IDE1006 // Naming Styles
public record OneLoginUserInfo(string sub, string email, string vot, string sid, string? CoreIdentityVc)
#pragma warning restore IDE1006 // Naming Styles
{
    public static OneLoginUserInfo Create(string sub, string email) =>
        Create(sub, email, (string?)null);

    public static OneLoginUserInfo Create(string sub, string email, JsonDocument? coreIdentityVc)
    {
        string? coretIdentityVcStr = coreIdentityVc is null ? null : JsonSerializer.Serialize(coreIdentityVc);
        return Create(sub, email, coretIdentityVcStr);
    }

    public static OneLoginUserInfo Create(string sub, string email, string? coreIdentityVc) =>
        new(sub, email, vot: @"[""Cl.Cm""]", sid: Guid.NewGuid().ToString(), coreIdentityVc);
}

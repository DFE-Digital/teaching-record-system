namespace TeachingRecordSystem.Core;

public static class ApiRoles
{
    public const string GetPerson = "GetPerson";
    public const string UpdatePerson = "UpdatePerson";
    public const string UpdateNpq = "UpdateNpq";
    public const string UnlockPerson = "UnlockPerson";
    public const string CreateTrn = "CreateTrn";

    public static IReadOnlyCollection<string> All { get; } = new[]
    {
        GetPerson,
        UpdatePerson,
        UpdateNpq,
        UnlockPerson,
        CreateTrn
    };
}

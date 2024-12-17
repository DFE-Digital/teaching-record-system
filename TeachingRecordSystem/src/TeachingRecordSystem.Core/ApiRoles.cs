namespace TeachingRecordSystem.Core;

public static class ApiRoles
{
    public const string GetPerson = "GetPerson";
    public const string UpdatePerson = "UpdatePerson";
    public const string UpdateNpq = "UpdateNpq";
    public const string UnlockPerson = "UnlockPerson";
    public const string CreateTrn = "CreateTrn";
    public const string AssignQtls = "AssignQtls";
    public const string AppropriateBody = "AppropriateBody";
    public const string UpdateRole = "UpdateRole";
    public const string SetCpdInduction = "SetCpdInduction";
    public const string SetWelshInduction = "SetWelshInduction";

    public static IReadOnlyCollection<string> All { get; } =
    [
        GetPerson,
        UpdatePerson,
        UpdateNpq,
        UnlockPerson,
        CreateTrn,
        AssignQtls,
        AppropriateBody,
        UpdateRole,
        SetCpdInduction,
        SetWelshInduction
    ];
}

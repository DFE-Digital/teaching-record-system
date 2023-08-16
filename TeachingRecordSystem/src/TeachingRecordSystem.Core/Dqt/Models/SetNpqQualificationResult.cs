#nullable disable


namespace TeachingRecordSystem.Core.Dqt.Models;

public sealed class SetNpqQualificationResult
{
    public bool Succeeded { get; private set; }
    public SetNpqQualificationFailedReasons FailedReasons { get; private set; }

    public static SetNpqQualificationResult Success() => new()
    {
        Succeeded = true,
    };

    public static SetNpqQualificationResult Failed(SetNpqQualificationFailedReasons reasons) => new()
    {
        Succeeded = false,
        FailedReasons = reasons
    };
}

[Flags]
public enum SetNpqQualificationFailedReasons
{
    None = 0,
    MultipleNpqQualificationsWithQualificationType = 1,
    NpqQualificationNotCreatedByApi = 2
}

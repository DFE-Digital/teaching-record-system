#nullable disable


namespace TeachingRecordSystem.Core.Dqt.Models;

public sealed class SetIttResultForTeacherResult
{
    public bool Succeeded { get; private set; }
    public SetIttResultForTeacherFailedReason FailedReason { get; private set; }
    public DateOnly? QtsDate { get; private set; }

    public static SetIttResultForTeacherResult Success(DateOnly? qtsDate) => new()
    {
        Succeeded = true,
        QtsDate = qtsDate
    };

    public static SetIttResultForTeacherResult Failed(SetIttResultForTeacherFailedReason reason) => new()
    {
        Succeeded = false,
        FailedReason = reason
    };
}

public enum SetIttResultForTeacherFailedReason
{
    QtsDateMismatch,
    EytsDateMismatch,
    NoMatchingIttRecord,
    MultipleIttRecords,
    NoMatchingQtsRecord,
    MultipleQtsRecords,
}

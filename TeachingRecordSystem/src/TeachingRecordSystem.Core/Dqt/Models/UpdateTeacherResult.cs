#nullable disable


namespace TeachingRecordSystem.Core.Dqt.Models;

public sealed class UpdateTeacherResult
{
    public bool Succeeded { get; private set; }
    public Guid TeacherId { get; private set; }
    public UpdateTeacherFailedReasons FailedReasons { get; private set; }

    public static UpdateTeacherResult Success(Guid teacherId, string trn) => new()
    {
        Succeeded = true,
        TeacherId = teacherId,
    };

    public static UpdateTeacherResult Failed(UpdateTeacherFailedReasons reasons) => new()
    {
        Succeeded = false,
        FailedReasons = reasons
    };
}

[Flags]
public enum UpdateTeacherFailedReasons
{
    None = 0,
    AlreadyHaveQtsDate = 1,
    NoMatchingIttRecord = 2,
    AlreadyHaveEytsDate = 4,
    IttProviderNotFound = 8,
    MultipleInTrainingIttRecords = 16,
    Subject1NotFound = 32,
    Subject2NotFound = 64,
    QualificationCountryNotFound = 128,
    QualificationSubjectNotFound = 256,
    QualificationProviderNotFound = 512,
    QualificationNotFound = 1024,
    MultipleQualificationRecords = 2048,
    CannotChangeProgrammeType = 4096,
    Subject3NotFound = 8192,
    IttQualificationNotFound = 16384,
    QualificationSubject2NotFound = 32768,
    QualificationSubject3NotFound = 65536,
    DuplicateHusId = 131072,
    TrainingCountryNotFound = 262144,
    InTrainingResultNotPermittedForProgrammeType = 524288,
    UnderAssessmentOnlyPermittedForProgrammeType = 1048576,
    NoMatchingQtsRecord = 2097152,
    MultipleQtsRecords = 4194304,
    UnableToUnwithdrawToDeferredStatus = 8388608,
    UnableToChangeFailedResult = 16777216
}

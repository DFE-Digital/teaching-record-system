namespace TeachingRecordSystem.Core.Dqt.Models;

public sealed class SetProfessionalStatusResult
{
    public bool Succeeded { get; private set; }
    public SetProfessionalStatusFailedReason FailedReason { get; private set; }

    public static SetProfessionalStatusResult Success() => new()
    {
        Succeeded = true
    };

    public static SetProfessionalStatusResult Failed(SetProfessionalStatusFailedReason reason) => new()
    {
        Succeeded = false,
        FailedReason = reason
    };
}

public enum SetProfessionalStatusFailedReason
{
    MultipleIttRecords,
    NoMatchingQtsRecord,
    MultipleQtsRecords,
    NoMatchingTeacherStatus,
    NoMatchingIttQualification,
    NoMatchingSubject1,
    NoMatchingSubject2,
    NoMatchingSubject3,
    NoMatchingCountry,
    NoMatchingProvider,
    CannotChangeProgrammeType,
    NoMatchingIttProvider,
    NoMatchingTrainingCountry,
    InTrainingResultNotPermittedForProgrammeType,
    UnderAssessmentOnlyPermittedForProgrammeType,
    UnableToUnwithdrawToDeferredStatus,
    UnableToChangeFailedResult
}

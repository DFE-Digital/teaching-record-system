using System;

namespace DqtApi.DataStore.Crm
{
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
        AlreadyHaveEytsDate = 4,
        NoMatchingIttRecord = 8,
        MultipleInTrainingIttRecords = 16,
        Subject1NotFound = 32,
        Subject2NotFound = 64,
        QualificationCountryNotFound = 128,
        QualificationSubjectNotFound = 256,
        QualificationProviderNotFound = 512,
        QualificationNotFound = 1024,
        MultipleQualificationRecords = 2048,
        CannotChangeProgrammeType = 4096
    }
}

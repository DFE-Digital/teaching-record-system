using System;

namespace DqtApi.DataStore.Crm
{
    public sealed class CreateTeacherResult
    {
        public bool Succeeded { get; private set; }
        public Guid TeacherId { get; private set; }
        public string Trn { get; private set; }
        public CreateTeacherFailedReasons FailedReasons { get; private set; }

        public static CreateTeacherResult Success(Guid teacherId, string trn) => new()
        {
            Succeeded = true,
            TeacherId = teacherId,
            Trn = trn
        };

        public static CreateTeacherResult Failed(CreateTeacherFailedReasons reasons) => new()
        {
            Succeeded = false,
            FailedReasons = reasons
        };
    }

    [Flags]
    public enum CreateTeacherFailedReasons
    {
        None = 0,
        IttProviderNotFound = 1,
        Subject1NotFound = 2,
        Subject2NotFound = 4,
        QualificationCountryNotFound = 8,
        QualificationSubjectNotFound = 16,
        QualificationProviderNotFound = 32,
    }
}

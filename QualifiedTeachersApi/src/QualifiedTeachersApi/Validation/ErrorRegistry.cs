using System.Collections.Generic;
using System.Linq;

namespace QualifiedTeachersApi.Validation
{
    public static class ErrorRegistry
    {
        private static readonly Dictionary<int, ErrorDescriptor> _all = new ErrorDescriptor[]
        {
            ErrorDescriptor.Create(10001),  // Teacher with specified TRN not found
            ErrorDescriptor.Create(10002),  // Multiple teachers found with specified TRN
            ErrorDescriptor.Create(10003),  // Teacher already has QTS/EYTS date
            ErrorDescriptor.Create(10004),  // Teacher has multiple incomplete ITT records
            ErrorDescriptor.Create(10005),  // Teacher has no incomplete ITT record
            ErrorDescriptor.Create(10006),  // Teacher has no QTS record
            ErrorDescriptor.Create(10007),  // Teacher has multiple QTS records
            ErrorDescriptor.Create(10008),  // Organisation not found
            ErrorDescriptor.Create(10009),  // Subject not found
            ErrorDescriptor.Create(10010),  // County not found,
            ErrorDescriptor.Create(10011),  // Cannot change Programme Type
            ErrorDescriptor.Create(10012),  // ITT qualification not found
            ErrorDescriptor.Create(10013),  // HE qualification not found
            ErrorDescriptor.Create(10014),  // Teacher has active sanctions
            ErrorDescriptor.Create(10015),  // Teacher with specified TsPersonId not found
            ErrorDescriptor.Create(10016),  // Another teacher has the specified TsPersonId assigned
            ErrorDescriptor.Create(10017),  // Teacher already has a TsPersonId defined
            ErrorDescriptor.Create(10018),  // Husid already being used for an existing teacher
            ErrorDescriptor.Create(10019),  // Qualification completion date cannot be in the future
            ErrorDescriptor.Create(10020),  // Multiple npq qualiications for Qualification Type
            ErrorDescriptor.Create(10021),  // Npq Qualification not created by api
            ErrorDescriptor.Create(10022),  // Identity user not found
        }.ToDictionary(d => d.ErrorCode, d => d);

        public static Error TeacherWithSpecifiedTrnNotFound() => CreateError(10001);

        public static Error MultipleTeachersFoundWithSpecifiedTrn() => CreateError(10002);

        public static Error TeacherAlreadyHasQtsDate() => CreateError(10003);

        public static Error TeacherAlreadyMultipleIncompleteIttRecords() => CreateError(10004);

        public static Error TeacherHasNoIncompleteIttRecord() => CreateError(10005);

        public static Error TeacherHasNoQtsRecord() => CreateError(10006);

        public static Error TeacherHasMultipleQtsRecords() => CreateError(10007);

        public static Error OrganisationNotFound() => CreateError(10008);

        public static Error SubjectNotFound() => CreateError(10009);

        public static Error CountryNotFound() => CreateError(10010);

        public static Error CannotChangeProgrammeType() => CreateError(10011);

        public static Error IttQualificationNotFound() => CreateError(10012);

        public static Error HeQualificationNotFound() => CreateError(10013);

        public static Error TeacherHasActiveSanctions() => CreateError(10014);

        public static Error TeacherWithSpecifiedTsPersonIdNotFound() => CreateError(10015);

        public static Error AnotherTeacherHasTheSpecifiedTsPersonIdAssigned() => CreateError(10016);

        public static Error TeacherAlreadyHasTsPersonIdDefined() => CreateError(10017);

        public static Error ExistingTeacherAlreadyHasHusId() => CreateError(10018);

        public static Error QualificationCompletionDateInTheFuture() => CreateError(10019);

        public static Error MultipleNpqQualificationWithQualificationType() => CreateError(10020);

        public static Error NpqQualificationNotCreatedByApi() => CreateError(10021);

        public static Error IdentityUserNotFound() => CreateError(10022);

        private static Error CreateError(int errorCode)
        {
            var descriptor = _all[errorCode];
            return new Error(descriptor);
        }
    }
}

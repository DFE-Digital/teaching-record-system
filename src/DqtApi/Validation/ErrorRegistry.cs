using System.Collections.Generic;
using System.Linq;

namespace DqtApi.Validation
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
            ErrorDescriptor.Create(10007),  // Teacher has multiple QTS records,
            ErrorDescriptor.Create(10008)   // Organisation not found
        }.ToDictionary(d => d.ErrorCode, d => d);

        public static Error TeacherWithSpecifiedTrnNotFound() => CreateError(10001);

        public static Error MultipleTeachersFoundWithSpecifiedTrn() => CreateError(10002);

        public static Error TeacherAlreadyHasQtsDate() => CreateError(10003);

        public static Error TeacherAlreadyMultipleIncompleteIttRecords() => CreateError(10004);

        public static Error TeacherHasNoIncompleteIttRecord() => CreateError(10005);

        public static Error TeacherHasNoQtsRecord() => CreateError(10006);

        public static Error TeacherHasMultipleQtsRecords() => CreateError(10007);
        public static Error OrganisationNotFound() => CreateError(10008);
        private static Error CreateError(int errorCode)
        {
            var descriptor = _all[errorCode];
            return new Error(descriptor);
        }
    }
}

using QualifiedTeachersApi.DataStore.Crm.Models;

namespace QualifiedTeachersApi.V2.ApiModels
{
    public enum Gender
    {
        Male = 1,
        Female = 2,
        Other = 389040000,
        NotAvailable = 389040002,
        NotProvided = 389040001,
    }

    public static class GenderExtensions
    {
        public static Contact_GenderCode ConvertToContact_GenderCode(this Gender input) =>
            input.ConvertToEnum<Gender, Contact_GenderCode>();

        public static bool TryConvertToContact_GenderCode(this Gender input, out Contact_GenderCode result) =>
            input.TryConvertToEnum(out result);
    }
}

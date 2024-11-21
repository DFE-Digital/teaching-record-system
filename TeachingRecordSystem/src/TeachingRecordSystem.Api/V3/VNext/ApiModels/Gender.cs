#nullable disable
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.VNext.ApiModels;

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
        input.ConvertToEnumByValue<Gender, Contact_GenderCode>();

    public static bool TryConvertToContact_GenderCode(this Gender input, out Contact_GenderCode result) =>
        input.TryConvertToEnumByValue(out result);
}

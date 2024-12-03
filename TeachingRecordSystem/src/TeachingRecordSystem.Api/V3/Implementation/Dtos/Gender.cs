using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3
}

public static class GenderExtensions
{
    public static Contact_GenderCode ConvertToContact_GenderCode(this Gender input) =>
        input.ConvertToEnumByValue<Gender, Contact_GenderCode>();

    public static bool TryConvertToContact_GenderCode(this Gender input, out Contact_GenderCode result) =>
        input.TryConvertToEnumByValue(out result);
}

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
        input switch
        {
            Gender.Male => Contact_GenderCode.Male,
            Gender.Female => Contact_GenderCode.Female,
            Gender.Other => Contact_GenderCode.Other,
            _ => throw new ArgumentException($"Unknown {nameof(Gender)}: '{input}'.", nameof(input))
        };
}

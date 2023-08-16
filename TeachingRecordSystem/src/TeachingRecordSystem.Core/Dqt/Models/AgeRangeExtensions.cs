namespace TeachingRecordSystem.Core.Dqt.Models;

public static class AgeRange
{
    public static dfeta_AgeRange ConvertFromValue(int age)
    {
        if (!TryConvertFromValue(age, out var result))
        {
            throw new ArgumentException($"{age} cannot be converted to {nameof(dfeta_AgeRange)}.", nameof(age));
        }

        return result;
    }

    public static bool TryConvertFromValue(int age, out dfeta_AgeRange result)
    {
        var converted = age switch
        {
            0 => dfeta_AgeRange._00,
            1 => dfeta_AgeRange._01,
            2 => dfeta_AgeRange._02,
            3 => dfeta_AgeRange._03,
            4 => dfeta_AgeRange._04,
            5 => dfeta_AgeRange._05,
            6 => dfeta_AgeRange._06,
            7 => dfeta_AgeRange._07,
            8 => dfeta_AgeRange._08,
            9 => dfeta_AgeRange._09,
            10 => dfeta_AgeRange._10,
            11 => dfeta_AgeRange._11,
            12 => dfeta_AgeRange._12,
            13 => dfeta_AgeRange._13,
            14 => dfeta_AgeRange._14,
            15 => dfeta_AgeRange._15,
            16 => dfeta_AgeRange._16,
            17 => dfeta_AgeRange._17,
            18 => dfeta_AgeRange._18,
            19 => dfeta_AgeRange._19,
            _ => (dfeta_AgeRange?)null
        };

        if (converted.HasValue)
        {
            result = converted.Value;
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }
}

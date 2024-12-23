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

    public static (int AgeFrom, int AgeTo)? ConvertToValues(this dfeta_AgeRange ageRange)
    {
        return ageRange switch
        {
            dfeta_AgeRange._00 => (0, 0),
            dfeta_AgeRange._01 => (1, 1),
            dfeta_AgeRange._02 => (2, 2),
            dfeta_AgeRange._03 => (3, 3),
            dfeta_AgeRange._04 => (4, 4),
            dfeta_AgeRange._05 => (5, 5),
            dfeta_AgeRange._06 => (6, 6),
            dfeta_AgeRange._07 => (7, 7),
            dfeta_AgeRange._08 => (8, 8),
            dfeta_AgeRange._09 => (9, 9),
            dfeta_AgeRange._10 => (10, 10),
            dfeta_AgeRange._11 => (11, 11),
            dfeta_AgeRange._12 => (12, 12),
            dfeta_AgeRange._13 => (13, 13),
            dfeta_AgeRange._14 => (14, 14),
            dfeta_AgeRange._15 => (15, 15),
            dfeta_AgeRange._16 => (16, 16),
            dfeta_AgeRange._17 => (17, 17),
            dfeta_AgeRange._18 => (18, 18),
            dfeta_AgeRange._19 => (19, 19),
            dfeta_AgeRange._99 => (99, 99),
            dfeta_AgeRange.FoundationStage => (0, 5),
            dfeta_AgeRange.KeyStage1 => (5, 7),
            dfeta_AgeRange.KeyStage2 => (7, 11),
            dfeta_AgeRange.KeyStage3 => (11, 14),
            dfeta_AgeRange.KeyStage4 => (14, 16),
            dfeta_AgeRange.Post16 => (16, 18),
            dfeta_AgeRange.FurtherEducation => (16, 19),
            _ => null
        };
    }

    public static (TrainingAgeSpecialismType TrainingAgeSpecialismType, int? TrainingAgeSpecialismRangeFrom, int? TrainingAgeSpecialismRangeTo)? ConvertToTrsTrainingAgeSpecialism(dfeta_AgeRange? ageRangeFrom, dfeta_AgeRange? ageRangeTo)
    {
        if (ageRangeFrom is null && ageRangeTo is null)
        {
            return null;
        }
        var ageFromValues = ageRangeFrom?.ConvertToValues();
        var ageToValues = ageRangeTo?.ConvertToValues();
        if (ageFromValues is null && ageToValues is null)
        {
            return null;
        }

        // If we have a single value or both values are the same for special ranges then we can convert to the TRS representation directly
        if ((ageRangeFrom is not null && ageRangeFrom!.Value.IsSpecialRange()) ||
            (ageRangeTo is not null && ageRangeTo!.Value.IsSpecialRange()))
        {
            var derivedAgeRangeFrom = ageRangeFrom ?? ageRangeTo;
            var derivedAgeRangeTo = ageRangeTo ?? ageRangeFrom;
            if (derivedAgeRangeFrom == derivedAgeRangeTo)
            {
                var trainingAgeSpecialismType = derivedAgeRangeFrom!.Value switch
                {
                    dfeta_AgeRange.FoundationStage => TrainingAgeSpecialismType.FoundationStage,
                    dfeta_AgeRange.KeyStage1 => TrainingAgeSpecialismType.KeyStage1,
                    dfeta_AgeRange.KeyStage2 => TrainingAgeSpecialismType.KeyStage2,
                    dfeta_AgeRange.KeyStage3 => TrainingAgeSpecialismType.KeyStage3,
                    dfeta_AgeRange.KeyStage4 => TrainingAgeSpecialismType.KeyStage4,
                    dfeta_AgeRange.FurtherEducation => TrainingAgeSpecialismType.FurtherEducation,
                    _ => throw new ArgumentException($"Cannot convert {derivedAgeRangeFrom} to {nameof(TrainingAgeSpecialismType)}.", nameof(derivedAgeRangeFrom))
                };

                return (trainingAgeSpecialismType, null, null);
            }
        }

        // Use the minimum and maximum implied values from the age ranges stored in DQT to determine the age range to use in TRS
        var allAges = (new int?[] { ageFromValues?.AgeFrom, ageFromValues?.AgeTo, ageToValues?.AgeFrom, ageToValues?.AgeTo }.Where(i => i.HasValue).Select(i => i!.Value).Distinct()).ToList();
        var ageFrom = allAges.Min();
        var ageTo = allAges.Max();

        return (TrainingAgeSpecialismType.Range, ageFrom, ageTo);
    }

    public static bool IsSpecialRange(this dfeta_AgeRange ageRange)
    {
        return ageRange switch
        {
            dfeta_AgeRange.FoundationStage or
            dfeta_AgeRange.KeyStage1 or
            dfeta_AgeRange.KeyStage2 or
            dfeta_AgeRange.KeyStage3 or
            dfeta_AgeRange.KeyStage4 or
            dfeta_AgeRange.FurtherEducation => true,
            _ => false
        };
    }
}

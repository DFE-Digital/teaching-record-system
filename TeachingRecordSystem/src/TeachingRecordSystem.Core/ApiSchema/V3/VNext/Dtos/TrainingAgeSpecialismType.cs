namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public enum TrainingAgeSpecialismType
{
    Range,
    FoundationStage,
    FurtherEducation,
    KeyStage1,
    KeyStage2,
    KeyStage3,
    KeyStage4
}

public static class TrainingAgeSpecialismTypeExtensions
{
    public static TrainingAgeSpecialismType ConvertFromTrainingAgeSpecialismType(this Models.TrainingAgeSpecialismType input)
    {
        if (!input.TryConvertFromTrainingAgeSpecialismType(out var result))
        {
            throw new FormatException($"Unknown {typeof(Models.TrainingAgeSpecialismType).Name}: '{input}'.");
        }

        return result;
    }

    public static Models.TrainingAgeSpecialismType ConvertToTrainingAgeSpecialismType(this TrainingAgeSpecialismType input)
    {
        if (!input.TryConvertToTrainingAgeSpecialismType(out var result))
        {
            throw new FormatException($"Unknown {typeof(TrainingAgeSpecialismType).Name}: '{input}'.");
        }

        return result;
    }

    public static bool TryConvertFromTrainingAgeSpecialismType(this Models.TrainingAgeSpecialismType input, out TrainingAgeSpecialismType result)
    {
        var mapped = input switch
        {
            Models.TrainingAgeSpecialismType.None => TrainingAgeSpecialismType.Range,
            Models.TrainingAgeSpecialismType.FoundationStage => TrainingAgeSpecialismType.FoundationStage,
            Models.TrainingAgeSpecialismType.FurtherEducation => TrainingAgeSpecialismType.FurtherEducation,
            Models.TrainingAgeSpecialismType.KeyStage1 => TrainingAgeSpecialismType.KeyStage1,
            Models.TrainingAgeSpecialismType.KeyStage2 => TrainingAgeSpecialismType.KeyStage2,
            Models.TrainingAgeSpecialismType.KeyStage3 => TrainingAgeSpecialismType.KeyStage3,
            Models.TrainingAgeSpecialismType.KeyStage4 => TrainingAgeSpecialismType.KeyStage4,
            _ => (TrainingAgeSpecialismType?)null
        };

        if (mapped is null)
        {
            result = default;
            return false;
        }

        result = mapped.Value;
        return true;
    }

    public static bool TryConvertToTrainingAgeSpecialismType(this TrainingAgeSpecialismType input, out Models.TrainingAgeSpecialismType result)
    {
        var mapped = input switch
        {
            TrainingAgeSpecialismType.Range => Models.TrainingAgeSpecialismType.None,
            TrainingAgeSpecialismType.FoundationStage => Models.TrainingAgeSpecialismType.FoundationStage,
            TrainingAgeSpecialismType.FurtherEducation => Models.TrainingAgeSpecialismType.FurtherEducation,
            TrainingAgeSpecialismType.KeyStage1 => Models.TrainingAgeSpecialismType.KeyStage1,
            TrainingAgeSpecialismType.KeyStage2 => Models.TrainingAgeSpecialismType.KeyStage2,
            TrainingAgeSpecialismType.KeyStage3 => Models.TrainingAgeSpecialismType.KeyStage3,
            TrainingAgeSpecialismType.KeyStage4 => Models.TrainingAgeSpecialismType.KeyStage4,
            _ => (Models.TrainingAgeSpecialismType?)null
        };

        if (mapped is null)
        {
            result = default;
            return false;
        }

        result = mapped.Value;
        return true;
    }
}

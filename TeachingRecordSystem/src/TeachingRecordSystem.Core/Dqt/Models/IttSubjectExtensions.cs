namespace TeachingRecordSystem.Core.Dqt.Models;

public static class IttSubjectExtensions
{
    public static async Task<dfeta_ittsubject> ConvertFromTrsTrainingSubjectReferenceAsync(this string trainingSubjectReference, ReferenceDataCache referenceDataCache)
    {
        var result = await trainingSubjectReference.TryConvertFromTrsTrainingSubjectReferenceAsync(referenceDataCache);
        if (result.IsSuccess)
        {
            return result.Result!;
        }

        throw new ArgumentException($"{trainingSubjectReference} cannot be converted to {nameof(dfeta_ittsubject)}.", nameof(dfeta_ittsubject));
    }

    public static async Task<(bool IsSuccess, dfeta_ittsubject? Result)> TryConvertFromTrsTrainingSubjectReferenceAsync(this string trainingSubjectReference, ReferenceDataCache referenceDataCache)
    {
        var converted = await referenceDataCache.GetIttSubjectBySubjectCodeAsync(trainingSubjectReference);
        if (converted is not null)
        {
            return (true, converted);
        }

        return (false, default);
    }
}

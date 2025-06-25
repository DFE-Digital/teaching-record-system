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

    public static async Task<TrainingSubject> ConvertToTrsTrainingSubjectAsync(this Guid ittSubjectId, ReferenceDataCache referenceDataCache)
    {
        var result = await ittSubjectId.TryConvertToTrsTrainingSubjectAsync(referenceDataCache);
        if (result.IsSuccess)
        {
            return result.Result!;
        }
        throw new ArgumentException($"{ittSubjectId} cannot be converted to {nameof(TrainingSubject)}.", nameof(TrainingSubject));
    }

    public static async Task<(bool IsSuccess, TrainingSubject? Result)> TryConvertToTrsTrainingSubjectAsync(this Guid ittSubjectId, ReferenceDataCache referenceDataCache)
    {
        var ittSubject = await referenceDataCache.GetIttSubjectBySubjectIdAsync(ittSubjectId);
        if (ittSubject is null)
        {
            return (false, default);
        }

        // Very specific mapping to avoid duplicate references
        var trsReference = ittSubject.dfeta_Value;
        if (trsReference == "C600" && ittSubject.dfeta_name == "Physical Education")
        {
            trsReference = "C600PE";
        }

        var trainingSubject = await referenceDataCache.GetTrainingSubjectsByReferenceAsync(trsReference);
        if (trainingSubject is not null)
        {
            return (true, trainingSubject);
        }

        return (false, default);
    }
}

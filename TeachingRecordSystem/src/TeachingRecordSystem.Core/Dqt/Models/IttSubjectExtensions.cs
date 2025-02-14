namespace TeachingRecordSystem.Core.Dqt.Models;

public static class IttSubjectExtensions
{
    public static async Task<dfeta_ittsubject> ConvertFromTrsTrainingSubjectReferenceAsync(this string trainingSubjectReference, ReferenceDataCache referenceDataCache)
    {
        // TODO flesh out mapping from TRS training subject reference to DQT ITT subject code once we have the TRS training subject reference data
        var dqtIttSubjectCode = trainingSubjectReference;
        var converted = await referenceDataCache.GetIttSubjectBySubjectCodeAsync(dqtIttSubjectCode);
        if (converted is null)
        {
            throw new ArgumentException($"{trainingSubjectReference} cannot be converted to {nameof(dfeta_ittsubject)}.", nameof(dfeta_ittsubject));
        }

        return converted;
    }
}

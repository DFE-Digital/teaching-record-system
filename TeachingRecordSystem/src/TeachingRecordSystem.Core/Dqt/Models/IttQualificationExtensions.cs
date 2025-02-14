namespace TeachingRecordSystem.Core.Dqt.Models;

public static class IttQualificationExtensions
{
    public static async Task<dfeta_ittqualification?> ConvertFromTrsDegreeTypeIdAsync(this Guid? degreeTypeId, ReferenceDataCache referenceDataCache)
    {
        if (degreeTypeId is null)
        {
            return null;
        }

        // TODO flesh out mapping from TRS degree type ID to DQT ITT qualification value once we have the TRS degree type ID data
        var dqtIttQualificationTypeCode = "010";
        var converted = await referenceDataCache.GetIttQualificationByValueAsync(dqtIttQualificationTypeCode);
        if (converted is null)
        {
            throw new ArgumentException($"{degreeTypeId} cannot be converted to {nameof(dfeta_ittqualification)}.", nameof(dfeta_ittqualification));
        }

        return converted;
    }
}

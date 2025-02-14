namespace TeachingRecordSystem.Core.Dqt.Models;

public static class CountryExtensions
{
    public static async Task<dfeta_country> ConvertFromTrsCountryReferenceAsync(this string countryReference, ReferenceDataCache referenceDataCache)
    {
        // TODO flesh out mapping from TRS country reference to DQT country code once we have the TRS country reference data
        var dqtCountryCode = countryReference;
        var converted = await referenceDataCache.GetCountryByCountryCodeAsync(dqtCountryCode);
        if (converted is null)
        {
            throw new ArgumentException($"{countryReference} cannot be converted to {nameof(dfeta_country)}.", nameof(dfeta_country));
        }

        return converted;
    }
}

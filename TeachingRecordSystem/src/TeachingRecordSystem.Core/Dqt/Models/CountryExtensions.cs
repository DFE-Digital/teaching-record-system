namespace TeachingRecordSystem.Core.Dqt.Models;

public static class CountryExtensions
{
    public static async Task<dfeta_country> ConvertFromTrsCountryReferenceAsync(this string countryReference, ReferenceDataCache referenceDataCache)
    {
        var result = await countryReference.TryConvertFromTrsCountryReferenceAsync(referenceDataCache);
        if (result.IsSuccess)
        {
            return result.Result!;
        }

        throw new ArgumentException($"{countryReference} cannot be converted to {nameof(dfeta_country)}.", nameof(dfeta_country));
    }

    public static async Task<(bool IsSuccess, dfeta_country? Result)> TryConvertFromTrsCountryReferenceAsync(this string countryReference, ReferenceDataCache referenceDataCache)
    {
        // There are a few of country codes that are different between TRS and DQT
        var dqtCountryCode = countryReference switch
        {
            "GB-ENG" => "XF",
            "GB-NIR" => "XG",
            "GB-SCT" => "XH",
            "GB-WLS" => "XI",
            "GB-CYM" => "XI",
            "XK" => "QO",
            "CY" => "XC",
            _ => countryReference
        };

        var converted = await referenceDataCache.GetCountryByCountryCodeAsync(dqtCountryCode);
        if (converted is not null)
        {
            return (true, converted);
        }

        return (false, default);
    }

    public static async Task<Country?> ConvertToTrsCountryAsync(this Guid countryId, ReferenceDataCache referenceDataCache)
    {
        var result = await countryId.TryConvertToTrsCountryAsync(referenceDataCache);
        if (result.IsSuccess)
        {
            return result.Result;
        }

        throw new ArgumentException($"{countryId} cannot be converted to {nameof(Country)}.", nameof(countryId));
    }

    public static async Task<(bool IsSuccess, Country? Result)> TryConvertToTrsCountryAsync(this Guid countryId, ReferenceDataCache referenceDataCache)
    {
        var dqtCountry = await referenceDataCache.GetCountryByIdAsync(countryId);
        if (dqtCountry is null)
        {
            return (false, default);
        }

        // There are a few of the country codes that are different between TRS and DQT
        var trainingCountryId = dqtCountry.dfeta_Value switch
        {
            "AN" => "NL",
            "XF" => "GB-ENG",
            "XG" => "GB-NIR",
            "XH" => "GB-SCT",
            "XI" => "GB-WLS",
            "XC" => "CY",
            "XA" => "CY",
            "XB" => "TR",
            "XK" => "GB",
            "9639" => "CZ",
            "9880" => "ES",
            "TW" => "CN",
            "XP" => null,
            "EU" => null,
            "EEA Countries" => null,
            "ZZ" => null,
            _ => dqtCountry.dfeta_Value
        };

        if (string.IsNullOrEmpty(trainingCountryId))
        {
            return (true, null);
        }

        var converted = await referenceDataCache.GetTrainingCountryByIdAsync(trainingCountryId);
        if (converted is not null)
        {
            return (true, converted);
        }

        return (false, default);
    }
}

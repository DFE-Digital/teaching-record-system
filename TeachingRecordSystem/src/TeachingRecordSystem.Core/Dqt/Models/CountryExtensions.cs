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
}

namespace TeachingRecordSystem.Core.Dqt.Models;

public static class ProviderExtensions
{
    public static async Task<Account> ConvertFromUkprnAsync(this string ukPrn, ReferenceDataCache referenceDataCache)
    {
        var result = await ukPrn.TryConvertFromUkprnAsync(referenceDataCache);
        if (result.IsSuccess)
        {
            return result.Result!;
        }

        throw new ArgumentException($"{ukPrn} cannot be converted to {nameof(Account)}.", nameof(Account));
    }

    public static async Task<(bool IsSuccess, Account? Result)> TryConvertFromUkprnAsync(this string ukPrn, ReferenceDataCache referenceDataCache)
    {
        var provider = await referenceDataCache.GetIttProviderByUkPrnAsync(ukPrn);
        if (provider is not null)
        {
            return (true, provider);
        }

        return (false, null);
    }
}

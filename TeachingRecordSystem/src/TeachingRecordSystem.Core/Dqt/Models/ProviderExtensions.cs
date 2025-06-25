using TeachingRecordSystem.Core.DataStore.Postgres.Models;

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

    public static async Task<TrainingProvider> ConvertToTrsTrainingProviderAsync(this Guid providerId, ReferenceDataCache referenceDataCache)
    {
        var result = await providerId.TryConvertToTrsTrainingProviderAsync(referenceDataCache);
        if (result.IsSuccess)
        {
            return result.Result!;
        }
        throw new ArgumentException($"{providerId} cannot be converted to {nameof(TrainingProvider)}.", nameof(providerId));
    }

    public static async Task<(bool IsSuccess, TrainingProvider? Result)> TryConvertToTrsTrainingProviderAsync(this Guid providerId, ReferenceDataCache referenceDataCache)
    {
        var ittProvider = await referenceDataCache.GetIttProviderByIdAsync(providerId);
        if (ittProvider is null)
        {
            return (true, default);
        }

        TrainingProvider? trainingProvider = null;
        if (ittProvider.dfeta_UKPRN is not null)
        {
            trainingProvider = await referenceDataCache.GetTrainingProviderByUkprnAsync(ittProvider.dfeta_UKPRN);
            if (trainingProvider is not null)
            {
                return (true, trainingProvider);
            }
        }

        // Try and match against legacy providers without UKPRNs - the ID in TRS is the same as in DQT.
        trainingProvider = await referenceDataCache.GetTrainingProviderByIdAsync(providerId);
        if (trainingProvider is not null)
        {
            return (true, trainingProvider);
        }

        return (false, null);
    }
}

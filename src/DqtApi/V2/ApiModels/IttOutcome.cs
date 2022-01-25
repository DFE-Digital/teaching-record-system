using System;
using DqtApi.DataStore.Crm.Models;

namespace DqtApi.V2.ApiModels
{
    public enum IttOutcome
    {
        Pass = 1,
        Fail = 2,
        Withdrawn = 3,
        Deferred = 4,
        DeferredForSkillsTests = 5
    }

    public static class IttOutcomeExtensions
    {
        public static dfeta_ITTResult ConvertToITTResult(this IttOutcome input)
        {
            if (!TryConvertToITTResult(input, out var result))
            {
                throw new FormatException($"Unknown {typeof(IttOutcome).Name}: '{input}'.");
            }

            return result;
        }

        public static bool TryConvertToITTResult(this IttOutcome input, out dfeta_ITTResult result)
        {
            var mapped = input switch
            {
                IttOutcome.Pass => dfeta_ITTResult.Pass,
                IttOutcome.Fail => dfeta_ITTResult.Fail,
                IttOutcome.Withdrawn => dfeta_ITTResult.Withdrawn,
                IttOutcome.Deferred => dfeta_ITTResult.Deferred,
                IttOutcome.DeferredForSkillsTests => dfeta_ITTResult.DeferredforSkillsTests,
                _ => (dfeta_ITTResult?)null
            };

            if (mapped.HasValue)
            {
                result = mapped.Value;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
    }
}

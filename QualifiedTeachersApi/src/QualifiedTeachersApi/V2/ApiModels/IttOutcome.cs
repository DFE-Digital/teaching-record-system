#nullable disable
using System;
using QualifiedTeachersApi.DataStore.Crm.Models;

namespace QualifiedTeachersApi.V2.ApiModels;

public enum IttOutcome
{
    Pass = 1,
    Fail = 2,
    Withdrawn = 3,
    Deferred = 4,
    DeferredForSkillsTests = 5,
    ApplicationReceived = 6,
    ApplicationUnsuccessful = 7,
    Approved = 8,
    Info = 9,
    InTraining = 10,
    NoResultSubmitted = 11,
    UnderAssessment = 12
}

public static class IttOutcomeExtensions
{
    public static IttOutcome ConvertFromITTResult(this dfeta_ITTResult input)
    {
        if (!TryConvertFromITTResult(input, out var result))
        {
            throw new FormatException($"Unknown {typeof(dfeta_ITTResult).Name}: '{input}'.");
        }

        return result;
    }

    public static dfeta_ITTResult ConvertToITTResult(this IttOutcome input)
    {
        if (!TryConvertToITTResult(input, out var result))
        {
            throw new FormatException($"Unknown {typeof(IttOutcome).Name}: '{input}'.");
        }

        return result;
    }

    public static bool TryConvertFromITTResult(this dfeta_ITTResult input, out IttOutcome result)
    {
        var mapped = input switch
        {
            dfeta_ITTResult.Pass => IttOutcome.Pass,
            dfeta_ITTResult.Fail => IttOutcome.Fail,
            dfeta_ITTResult.Withdrawn => IttOutcome.Withdrawn,
            dfeta_ITTResult.Deferred => IttOutcome.Deferred,
            dfeta_ITTResult.DeferredforSkillsTests => IttOutcome.DeferredForSkillsTests,
            dfeta_ITTResult.ApplicationReceived => IttOutcome.ApplicationReceived,
            dfeta_ITTResult.ApplicationUnsuccessful => IttOutcome.ApplicationUnsuccessful,
            dfeta_ITTResult.Approved => IttOutcome.Approved,
            dfeta_ITTResult.Info => IttOutcome.Info,
            dfeta_ITTResult.InTraining => IttOutcome.InTraining,
            dfeta_ITTResult.NoResultSubmitted => IttOutcome.NoResultSubmitted,
            dfeta_ITTResult.UnderAssessment => IttOutcome.UnderAssessment,
            _ => (IttOutcome?)null
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

    public static bool TryConvertToITTResult(this IttOutcome input, out dfeta_ITTResult result)
    {
        var mapped = input switch
        {
            IttOutcome.Pass => dfeta_ITTResult.Pass,
            IttOutcome.Fail => dfeta_ITTResult.Fail,
            IttOutcome.Withdrawn => dfeta_ITTResult.Withdrawn,
            IttOutcome.Deferred => dfeta_ITTResult.Deferred,
            IttOutcome.DeferredForSkillsTests => dfeta_ITTResult.DeferredforSkillsTests,
            IttOutcome.ApplicationReceived => dfeta_ITTResult.ApplicationReceived,
            IttOutcome.ApplicationUnsuccessful => dfeta_ITTResult.ApplicationUnsuccessful,
            IttOutcome.Approved => dfeta_ITTResult.Approved,
            IttOutcome.Info => dfeta_ITTResult.Info,
            IttOutcome.InTraining => dfeta_ITTResult.InTraining,
            IttOutcome.NoResultSubmitted => dfeta_ITTResult.NoResultSubmitted,
            IttOutcome.UnderAssessment => dfeta_ITTResult.UnderAssessment,
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

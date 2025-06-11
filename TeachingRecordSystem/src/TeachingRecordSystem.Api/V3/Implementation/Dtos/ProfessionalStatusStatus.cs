using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public enum ProfessionalStatusStatus
{
    InTraining,
    Awarded,
    Deferred,
    DeferredForSkillsTest,
    Failed,
    Withdrawn,
    UnderAssessment,
    Approved
}

public static class ProfessionalStatusStatusExtensions
{
    public static ProfessionalStatusStatus ConvertFromProfessionalStatusStatus(this RouteToProfessionalStatusStatus input)
    {
        if (!input.TryConvertFromProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(RouteToProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static RouteToProfessionalStatusStatus ConvertToProfessionalStatusStatus(this ProfessionalStatusStatus input)
    {
        if (!input.TryConvertToProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(ProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static bool TryConvertFromProfessionalStatusStatus(this RouteToProfessionalStatusStatus input, out ProfessionalStatusStatus result)
    {
        var mapped = input switch
        {
            RouteToProfessionalStatusStatus.InTraining => ProfessionalStatusStatus.InTraining,
            RouteToProfessionalStatusStatus.Holds => ProfessionalStatusStatus.Awarded,
            RouteToProfessionalStatusStatus.Deferred => ProfessionalStatusStatus.Deferred,
            RouteToProfessionalStatusStatus.DeferredForSkillsTest => ProfessionalStatusStatus.DeferredForSkillsTest,
            RouteToProfessionalStatusStatus.Failed => ProfessionalStatusStatus.Failed,
            RouteToProfessionalStatusStatus.Withdrawn => ProfessionalStatusStatus.Withdrawn,
            RouteToProfessionalStatusStatus.UnderAssessment => ProfessionalStatusStatus.UnderAssessment,
            _ => (ProfessionalStatusStatus?)null
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

    public static bool TryConvertToProfessionalStatusStatus(this ProfessionalStatusStatus input, out RouteToProfessionalStatusStatus result)
    {
        var mapped = input switch
        {
            ProfessionalStatusStatus.InTraining => RouteToProfessionalStatusStatus.InTraining,
            ProfessionalStatusStatus.Awarded => RouteToProfessionalStatusStatus.Holds,
            ProfessionalStatusStatus.Approved => RouteToProfessionalStatusStatus.Holds,
            ProfessionalStatusStatus.Deferred => RouteToProfessionalStatusStatus.Deferred,
            ProfessionalStatusStatus.DeferredForSkillsTest => RouteToProfessionalStatusStatus.DeferredForSkillsTest,
            ProfessionalStatusStatus.Failed => RouteToProfessionalStatusStatus.Failed,
            ProfessionalStatusStatus.Withdrawn => RouteToProfessionalStatusStatus.Withdrawn,
            ProfessionalStatusStatus.UnderAssessment => RouteToProfessionalStatusStatus.UnderAssessment,
            _ => (RouteToProfessionalStatusStatus?)null
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

    public static dfeta_ITTResult ConvertToITTResult(this ProfessionalStatusStatus input)
    {
        if (!input.TryConvertToITTResult(out var result))
        {
            throw new FormatException($"Unknown {typeof(ProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static bool TryConvertToITTResult(this ProfessionalStatusStatus input, out dfeta_ITTResult result)
    {
        var mapped = input switch
        {
            ProfessionalStatusStatus.InTraining => dfeta_ITTResult.InTraining,
            ProfessionalStatusStatus.Awarded => dfeta_ITTResult.Pass,
            ProfessionalStatusStatus.Deferred => dfeta_ITTResult.Deferred,
            ProfessionalStatusStatus.DeferredForSkillsTest => dfeta_ITTResult.DeferredforSkillsTests,
            ProfessionalStatusStatus.Failed => dfeta_ITTResult.Fail,
            ProfessionalStatusStatus.Withdrawn => dfeta_ITTResult.Withdrawn,
            ProfessionalStatusStatus.UnderAssessment => dfeta_ITTResult.UnderAssessment,
            ProfessionalStatusStatus.Approved => dfeta_ITTResult.Approved,
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

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
    public static ProfessionalStatusStatus ConvertFromProfessionalStatusStatus(this Core.ProfessionalStatusStatus input)
    {
        if (!input.TryConvertFromProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(Core.ProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static Core.ProfessionalStatusStatus ConvertToProfessionalStatusStatus(this ProfessionalStatusStatus input)
    {
        if (!input.TryConvertToProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(ProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static bool TryConvertFromProfessionalStatusStatus(this Core.ProfessionalStatusStatus input, out ProfessionalStatusStatus result)
    {
        var mapped = input switch
        {
            Core.ProfessionalStatusStatus.InTraining => ProfessionalStatusStatus.InTraining,
            Core.ProfessionalStatusStatus.Awarded => ProfessionalStatusStatus.Awarded,
            Core.ProfessionalStatusStatus.Deferred => ProfessionalStatusStatus.Deferred,
            Core.ProfessionalStatusStatus.DeferredForSkillsTest => ProfessionalStatusStatus.DeferredForSkillsTest,
            Core.ProfessionalStatusStatus.Failed => ProfessionalStatusStatus.Failed,
            Core.ProfessionalStatusStatus.Withdrawn => ProfessionalStatusStatus.Withdrawn,
            Core.ProfessionalStatusStatus.UnderAssessment => ProfessionalStatusStatus.UnderAssessment,
            Core.ProfessionalStatusStatus.Approved => ProfessionalStatusStatus.Approved,
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

    public static bool TryConvertToProfessionalStatusStatus(this ProfessionalStatusStatus input, out Core.ProfessionalStatusStatus result)
    {
        var mapped = input switch
        {
            ProfessionalStatusStatus.InTraining => Core.ProfessionalStatusStatus.InTraining,
            ProfessionalStatusStatus.Awarded => Core.ProfessionalStatusStatus.Awarded,
            ProfessionalStatusStatus.Deferred => Core.ProfessionalStatusStatus.Deferred,
            ProfessionalStatusStatus.DeferredForSkillsTest => Core.ProfessionalStatusStatus.DeferredForSkillsTest,
            ProfessionalStatusStatus.Failed => Core.ProfessionalStatusStatus.Failed,
            ProfessionalStatusStatus.Withdrawn => Core.ProfessionalStatusStatus.Withdrawn,
            ProfessionalStatusStatus.UnderAssessment => Core.ProfessionalStatusStatus.UnderAssessment,
            ProfessionalStatusStatus.Approved => Core.ProfessionalStatusStatus.Approved,
            _ => (Core.ProfessionalStatusStatus?)null
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
            ProfessionalStatusStatus.Awarded => dfeta_ITTResult.Approved,
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

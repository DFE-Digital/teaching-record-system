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
    public static ProfessionalStatusStatus ConvertFromProfessionalStatusStatus(this Core.Models.ProfessionalStatusStatus input)
    {
        if (!input.TryConvertFromProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(Core.Models.ProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static Core.Models.ProfessionalStatusStatus ConvertToProfessionalStatusStatus(this ProfessionalStatusStatus input)
    {
        if (!input.TryConvertToProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(ProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static bool TryConvertFromProfessionalStatusStatus(this Core.Models.ProfessionalStatusStatus input, out ProfessionalStatusStatus result)
    {
        var mapped = input switch
        {
            Core.Models.ProfessionalStatusStatus.InTraining => ProfessionalStatusStatus.InTraining,
            Core.Models.ProfessionalStatusStatus.Awarded => ProfessionalStatusStatus.Awarded,
            Core.Models.ProfessionalStatusStatus.Deferred => ProfessionalStatusStatus.Deferred,
            Core.Models.ProfessionalStatusStatus.DeferredForSkillsTest => ProfessionalStatusStatus.DeferredForSkillsTest,
            Core.Models.ProfessionalStatusStatus.Failed => ProfessionalStatusStatus.Failed,
            Core.Models.ProfessionalStatusStatus.Withdrawn => ProfessionalStatusStatus.Withdrawn,
            Core.Models.ProfessionalStatusStatus.UnderAssessment => ProfessionalStatusStatus.UnderAssessment,
            Core.Models.ProfessionalStatusStatus.Approved => ProfessionalStatusStatus.Approved,
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

    public static bool TryConvertToProfessionalStatusStatus(this ProfessionalStatusStatus input, out Core.Models.ProfessionalStatusStatus result)
    {
        var mapped = input switch
        {
            ProfessionalStatusStatus.InTraining => Core.Models.ProfessionalStatusStatus.InTraining,
            ProfessionalStatusStatus.Awarded => Core.Models.ProfessionalStatusStatus.Awarded,
            ProfessionalStatusStatus.Deferred => Core.Models.ProfessionalStatusStatus.Deferred,
            ProfessionalStatusStatus.DeferredForSkillsTest => Core.Models.ProfessionalStatusStatus.DeferredForSkillsTest,
            ProfessionalStatusStatus.Failed => Core.Models.ProfessionalStatusStatus.Failed,
            ProfessionalStatusStatus.Withdrawn => Core.Models.ProfessionalStatusStatus.Withdrawn,
            ProfessionalStatusStatus.UnderAssessment => Core.Models.ProfessionalStatusStatus.UnderAssessment,
            ProfessionalStatusStatus.Approved => Core.Models.ProfessionalStatusStatus.Approved,
            _ => (Core.Models.ProfessionalStatusStatus?)null
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

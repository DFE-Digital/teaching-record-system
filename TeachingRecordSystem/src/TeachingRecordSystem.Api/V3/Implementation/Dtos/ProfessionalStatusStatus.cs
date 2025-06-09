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
    public static ProfessionalStatusStatus ConvertFromProfessionalStatusStatus(this Core.Models.RouteToProfessionalStatusStatus input)
    {
        if (!input.TryConvertFromProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(Core.Models.RouteToProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static Core.Models.RouteToProfessionalStatusStatus ConvertToProfessionalStatusStatus(this ProfessionalStatusStatus input)
    {
        if (!input.TryConvertToProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(ProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static bool TryConvertFromProfessionalStatusStatus(this Core.Models.RouteToProfessionalStatusStatus input, out ProfessionalStatusStatus result)
    {
        var mapped = input switch
        {
            Core.Models.RouteToProfessionalStatusStatus.InTraining => ProfessionalStatusStatus.InTraining,
            Core.Models.RouteToProfessionalStatusStatus.Awarded => ProfessionalStatusStatus.Awarded,
            Core.Models.RouteToProfessionalStatusStatus.Deferred => ProfessionalStatusStatus.Deferred,
            Core.Models.RouteToProfessionalStatusStatus.DeferredForSkillsTest => ProfessionalStatusStatus.DeferredForSkillsTest,
            Core.Models.RouteToProfessionalStatusStatus.Failed => ProfessionalStatusStatus.Failed,
            Core.Models.RouteToProfessionalStatusStatus.Withdrawn => ProfessionalStatusStatus.Withdrawn,
            Core.Models.RouteToProfessionalStatusStatus.UnderAssessment => ProfessionalStatusStatus.UnderAssessment,
            Core.Models.RouteToProfessionalStatusStatus.Approved => ProfessionalStatusStatus.Approved,
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

    public static bool TryConvertToProfessionalStatusStatus(this ProfessionalStatusStatus input, out Core.Models.RouteToProfessionalStatusStatus result)
    {
        var mapped = input switch
        {
            ProfessionalStatusStatus.InTraining => Core.Models.RouteToProfessionalStatusStatus.InTraining,
            ProfessionalStatusStatus.Awarded => Core.Models.RouteToProfessionalStatusStatus.Awarded,
            ProfessionalStatusStatus.Deferred => Core.Models.RouteToProfessionalStatusStatus.Deferred,
            ProfessionalStatusStatus.DeferredForSkillsTest => Core.Models.RouteToProfessionalStatusStatus.DeferredForSkillsTest,
            ProfessionalStatusStatus.Failed => Core.Models.RouteToProfessionalStatusStatus.Failed,
            ProfessionalStatusStatus.Withdrawn => Core.Models.RouteToProfessionalStatusStatus.Withdrawn,
            ProfessionalStatusStatus.UnderAssessment => Core.Models.RouteToProfessionalStatusStatus.UnderAssessment,
            ProfessionalStatusStatus.Approved => Core.Models.RouteToProfessionalStatusStatus.Approved,
            _ => (Core.Models.RouteToProfessionalStatusStatus?)null
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

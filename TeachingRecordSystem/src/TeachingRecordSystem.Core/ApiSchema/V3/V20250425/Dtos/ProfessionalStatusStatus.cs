namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos;

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
    public static ProfessionalStatusStatus ConvertFromProfessionalStatusStatus(this Models.RouteToProfessionalStatusStatus input)
    {
        if (!input.TryConvertFromProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(Models.RouteToProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static Models.RouteToProfessionalStatusStatus ConvertToProfessionalStatusStatus(this ProfessionalStatusStatus input)
    {
        if (!input.TryConvertToProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(ProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static bool TryConvertFromProfessionalStatusStatus(this Models.RouteToProfessionalStatusStatus input, out ProfessionalStatusStatus result)
    {
        var mapped = input switch
        {
            Models.RouteToProfessionalStatusStatus.InTraining => ProfessionalStatusStatus.InTraining,
            Models.RouteToProfessionalStatusStatus.Awarded => ProfessionalStatusStatus.Awarded,
            Models.RouteToProfessionalStatusStatus.Deferred => ProfessionalStatusStatus.Deferred,
            Models.RouteToProfessionalStatusStatus.DeferredForSkillsTest => ProfessionalStatusStatus.DeferredForSkillsTest,
            Models.RouteToProfessionalStatusStatus.Failed => ProfessionalStatusStatus.Failed,
            Models.RouteToProfessionalStatusStatus.Withdrawn => ProfessionalStatusStatus.Withdrawn,
            Models.RouteToProfessionalStatusStatus.UnderAssessment => ProfessionalStatusStatus.UnderAssessment,
            Models.RouteToProfessionalStatusStatus.Approved => ProfessionalStatusStatus.Approved,
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

    public static bool TryConvertToProfessionalStatusStatus(this ProfessionalStatusStatus input, out Models.RouteToProfessionalStatusStatus result)
    {
        var mapped = input switch
        {
            ProfessionalStatusStatus.InTraining => Models.RouteToProfessionalStatusStatus.InTraining,
            ProfessionalStatusStatus.Awarded => Models.RouteToProfessionalStatusStatus.Awarded,
            ProfessionalStatusStatus.Deferred => Models.RouteToProfessionalStatusStatus.Deferred,
            ProfessionalStatusStatus.DeferredForSkillsTest => Models.RouteToProfessionalStatusStatus.DeferredForSkillsTest,
            ProfessionalStatusStatus.Failed => Models.RouteToProfessionalStatusStatus.Failed,
            ProfessionalStatusStatus.Withdrawn => Models.RouteToProfessionalStatusStatus.Withdrawn,
            ProfessionalStatusStatus.UnderAssessment => Models.RouteToProfessionalStatusStatus.UnderAssessment,
            ProfessionalStatusStatus.Approved => Models.RouteToProfessionalStatusStatus.Approved,
            _ => (Models.RouteToProfessionalStatusStatus?)null
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

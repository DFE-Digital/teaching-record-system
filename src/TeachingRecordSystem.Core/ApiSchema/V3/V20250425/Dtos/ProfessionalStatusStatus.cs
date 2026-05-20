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
    public static RouteToProfessionalStatusStatus ConvertToRouteToProfessionalStatusStatus(this ProfessionalStatusStatus input)
    {
        if (!input.TryConvertToRouteToProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(ProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static bool TryConvertToRouteToProfessionalStatusStatus(this ProfessionalStatusStatus input, out RouteToProfessionalStatusStatus result)
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
}

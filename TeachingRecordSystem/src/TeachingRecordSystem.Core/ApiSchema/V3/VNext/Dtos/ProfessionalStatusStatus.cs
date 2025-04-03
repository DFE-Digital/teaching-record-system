namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

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
    public static ProfessionalStatusStatus ConvertFromProfessionalStatusStatus(this Models.ProfessionalStatusStatus input)
    {
        if (!input.TryConvertFromProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(Models.ProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static Models.ProfessionalStatusStatus ConvertToProfessionalStatusStatus(this ProfessionalStatusStatus input)
    {
        if (!input.TryConvertToProfessionalStatusStatus(out var result))
        {
            throw new FormatException($"Unknown {typeof(ProfessionalStatusStatus).Name}: '{input}'.");
        }

        return result;
    }

    public static bool TryConvertFromProfessionalStatusStatus(this Models.ProfessionalStatusStatus input, out ProfessionalStatusStatus result)
    {
        var mapped = input switch
        {
            Models.ProfessionalStatusStatus.InTraining => ProfessionalStatusStatus.InTraining,
            Models.ProfessionalStatusStatus.Awarded => ProfessionalStatusStatus.Awarded,
            Models.ProfessionalStatusStatus.Deferred => ProfessionalStatusStatus.Deferred,
            Models.ProfessionalStatusStatus.DeferredForSkillsTest => ProfessionalStatusStatus.DeferredForSkillsTest,
            Models.ProfessionalStatusStatus.Failed => ProfessionalStatusStatus.Failed,
            Models.ProfessionalStatusStatus.Withdrawn => ProfessionalStatusStatus.Withdrawn,
            Models.ProfessionalStatusStatus.UnderAssessment => ProfessionalStatusStatus.UnderAssessment,
            Models.ProfessionalStatusStatus.Approved => ProfessionalStatusStatus.Approved,
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

    public static bool TryConvertToProfessionalStatusStatus(this ProfessionalStatusStatus input, out Models.ProfessionalStatusStatus result)
    {
        var mapped = input switch
        {
            ProfessionalStatusStatus.InTraining => Models.ProfessionalStatusStatus.InTraining,
            ProfessionalStatusStatus.Awarded => Models.ProfessionalStatusStatus.Awarded,
            ProfessionalStatusStatus.Deferred => Models.ProfessionalStatusStatus.Deferred,
            ProfessionalStatusStatus.DeferredForSkillsTest => Models.ProfessionalStatusStatus.DeferredForSkillsTest,
            ProfessionalStatusStatus.Failed => Models.ProfessionalStatusStatus.Failed,
            ProfessionalStatusStatus.Withdrawn => Models.ProfessionalStatusStatus.Withdrawn,
            ProfessionalStatusStatus.UnderAssessment => Models.ProfessionalStatusStatus.UnderAssessment,
            ProfessionalStatusStatus.Approved => Models.ProfessionalStatusStatus.Approved,
            _ => (Models.ProfessionalStatusStatus?)null
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

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

public enum RouteToProfessionalStatusStatus
{
    InTraining,
    Holds,
    Deferred,
    DeferredForSkillsTest,
    Failed,
    Withdrawn,
    UnderAssessment
}

public static class RouteToProfessionalStatusStatusExtensions
{
    extension(RouteToProfessionalStatusStatus)
    {
        public static RouteToProfessionalStatusStatus Create(Models.RouteToProfessionalStatusStatus source) => source switch
        {
            Models.RouteToProfessionalStatusStatus.InTraining => RouteToProfessionalStatusStatus.InTraining,
            Models.RouteToProfessionalStatusStatus.Holds => RouteToProfessionalStatusStatus.Holds,
            Models.RouteToProfessionalStatusStatus.Deferred => RouteToProfessionalStatusStatus.Deferred,
            Models.RouteToProfessionalStatusStatus.DeferredForSkillsTest => RouteToProfessionalStatusStatus.DeferredForSkillsTest,
            Models.RouteToProfessionalStatusStatus.Failed => RouteToProfessionalStatusStatus.Failed,
            Models.RouteToProfessionalStatusStatus.Withdrawn => RouteToProfessionalStatusStatus.Withdrawn,
            Models.RouteToProfessionalStatusStatus.UnderAssessment => RouteToProfessionalStatusStatus.UnderAssessment,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}

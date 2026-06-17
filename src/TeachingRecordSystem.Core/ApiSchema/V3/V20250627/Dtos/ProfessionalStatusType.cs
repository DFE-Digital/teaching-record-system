namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

public enum ProfessionalStatusType
{
    QualifiedTeacherStatus = 0,
    EarlyYearsTeacherStatus = 1,
    EarlyYearsProfessionalStatus = 2,
    PartialQualifiedTeacherStatus = 3
}

public static class ProfessionalStatusTypeExtensions
{
    extension(ProfessionalStatusType)
    {
        public static ProfessionalStatusType Create(Models.ProfessionalStatusType source) => source switch
        {
            Models.ProfessionalStatusType.QualifiedTeacherStatus => ProfessionalStatusType.QualifiedTeacherStatus,
            Models.ProfessionalStatusType.EarlyYearsTeacherStatus => ProfessionalStatusType.EarlyYearsTeacherStatus,
            Models.ProfessionalStatusType.EarlyYearsProfessionalStatus => ProfessionalStatusType.EarlyYearsProfessionalStatus,
            Models.ProfessionalStatusType.PartialQualifiedTeacherStatus => ProfessionalStatusType.PartialQualifiedTeacherStatus,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}


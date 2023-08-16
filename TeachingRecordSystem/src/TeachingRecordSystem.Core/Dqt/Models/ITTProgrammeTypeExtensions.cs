namespace TeachingRecordSystem.Core.Dqt.Models;

public static class ITTProgrammeTypeExtensions
{
    public static bool IsEarlyYears(this dfeta_ITTProgrammeType programmeType) => programmeType switch
    {
        dfeta_ITTProgrammeType.EYITTAssessmentOnly => true,
        dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased => true,
        dfeta_ITTProgrammeType.EYITTGraduateEntry => true,
        dfeta_ITTProgrammeType.EYITTSchoolDirect_EarlyYears => true,
        dfeta_ITTProgrammeType.EYITTUndergraduate => true,
        _ => false
    };
}

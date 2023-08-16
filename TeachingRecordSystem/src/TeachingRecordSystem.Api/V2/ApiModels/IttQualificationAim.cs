#nullable disable
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.ApiModels;

public enum IttQualificationAim
{
    ProfessionalStatusOnly = 389040000,
    ProfessionalStatusAndAcademicAward = 389040001,
    ProfessionalStatusByAssessmentOnly = 389040002,
}

public static class IttQualificationAimExtensions
{
    public static dfeta_ITTQualificationAim ConvertToIttQualficationAim(this IttQualificationAim input) =>
        input.ConvertToEnum<IttQualificationAim, dfeta_ITTQualificationAim>();
}

#nullable disable
namespace TeachingRecordSystem.Core.Dqt.Models;

public static class QualificationTypeExtensions
{
    public static bool IsNpq(this dfeta_qualification_dfeta_Type qualificationType) => qualificationType switch
    {
        dfeta_qualification_dfeta_Type.NPQEL => true,
        dfeta_qualification_dfeta_Type.NPQEYL => true,
        dfeta_qualification_dfeta_Type.NPQH => true,
        dfeta_qualification_dfeta_Type.NPQLBC => true,
        dfeta_qualification_dfeta_Type.NPQLL => true,
        dfeta_qualification_dfeta_Type.NPQLT => true,
        dfeta_qualification_dfeta_Type.NPQLTD => true,
        dfeta_qualification_dfeta_Type.NPQML => true,
        dfeta_qualification_dfeta_Type.NPQSL => true,
        _ => false
    };

    public static string GetName(this dfeta_qualification_dfeta_Type qualificationType) => qualificationType switch
    {
        dfeta_qualification_dfeta_Type.NPQEL => "National Professional Qualification (NPQ) for Executive Leadership",
        dfeta_qualification_dfeta_Type.NPQEYL => "National Professional Qualification (NPQ) for Early Years Leadership",
        dfeta_qualification_dfeta_Type.NPQH => "National Professional Qualification (NPQ) for Headship",
        dfeta_qualification_dfeta_Type.NPQLBC => "National Professional Qualification (NPQ) for Leading Behaviour and Culture",
        dfeta_qualification_dfeta_Type.NPQLL => "National Professional Qualification (NPQ) for Leading Literacy",
        dfeta_qualification_dfeta_Type.NPQLT => "National Professional Qualification (NPQ) for Leading Teaching",
        dfeta_qualification_dfeta_Type.NPQLTD => "National Professional Qualification (NPQ) for Leading Teacher Development",
        dfeta_qualification_dfeta_Type.NPQML => "National Professional Qualification (NPQ) for Middle Leadership",
        dfeta_qualification_dfeta_Type.NPQSL => "National Professional Qualification (NPQ) for Senior Leadership",
        _ => null
    };
}

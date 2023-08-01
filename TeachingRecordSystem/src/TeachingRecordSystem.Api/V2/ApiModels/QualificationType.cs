#nullable disable
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.ApiModels;

public enum QualificationType
{
    NPQEL,
    NPQH,
    NPQSL,
    NPQLT,
    NPQLTD,
    NPQLBC,
    NPQLL,
    NPQEYL
}

public static class QualificationTypeExtensions
{
    public static dfeta_qualification_dfeta_Type ConvertToQualificationType(this QualificationType input) => input switch
    {
        QualificationType.NPQEL => dfeta_qualification_dfeta_Type.NPQEL,
        QualificationType.NPQSL => dfeta_qualification_dfeta_Type.NPQSL,
        QualificationType.NPQH => dfeta_qualification_dfeta_Type.NPQH,
        QualificationType.NPQLT => dfeta_qualification_dfeta_Type.NPQLT,
        QualificationType.NPQLTD => dfeta_qualification_dfeta_Type.NPQLTD,
        QualificationType.NPQLBC => dfeta_qualification_dfeta_Type.NPQLBC,
        QualificationType.NPQLL => dfeta_qualification_dfeta_Type.NPQLL,
        QualificationType.NPQEYL => dfeta_qualification_dfeta_Type.NPQEYL,
        _ => throw new FormatException($"Unknown {nameof(input)}.")
    };
}

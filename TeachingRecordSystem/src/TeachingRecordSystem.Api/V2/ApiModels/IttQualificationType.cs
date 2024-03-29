#nullable disable

namespace TeachingRecordSystem.Api.V2.ApiModels;

public enum IttQualificationType
{
    BEd = 1,
    BEdHons = 2,
    BSc = 3,
    BScHons = 4,
    BTech_Education = 5,
    BTechHons_Education = 6,
    BA = 7,
    BAHons = 8,
    BACombinedStudies_EducationOfTheDeaf = 9,
    BAHonsCombinedStudies_EducationOfTheDeaf = 10,
    BAEducationQTS = 11,
    BAWithIntercalatedPGCE = 12,
    BScHonsWithIntercalatedPGCE = 13,
    BAHonsWithIntercalatedPGCE = 14,
    BSc_EducationQTS = 15,
    BEngHons_Education = 16,
    BSc_CertificateInEducationQTS = 17,
    BA_CertificateInEducationQTS = 18,
    PostgraduateCertificateInEducationFlexible = 19,
    PostgraduateCertificateInEducation = 20,
    PostgraduateDiplomaInEducation = 21,
    PostgraduateArtTeachersCertificate = 22,
    PostgraduateArtTeachersDiploma = 23,
    GraduateCertificateInScienceAndEducation = 24,
    GraduateCertificateInMathematicsAndEducation = 25,
    PGCEArticledTeachersScheme = 26,
    QTSAwardOnly = 27,
    UndergraduateMasterOfTeaching = 28,
    QTSAssessmentOnly = 29,
    CertificateInEducation = 30,
    ProfessionalGraduateCertificateInEducation = 31,
    MastersNotByResearch = 32,
    GraduateCertificateInEducation = 33,
    CertificateInEducationFurtherEducation = 40,
    PostgraduateCertificateInEducationFurtherEducation = 41,
    TeachersCertificateFurtherEducation = 42,
    TeachersCertificate = 43,
    QualificationGainedInEurope = 49,
    ProfessionalGraduateDiplomaInEducation = 50,
    AssessmentOnlyRoute = 51,
    TeachFirstTNP = 52,
    TroopsToTeach = 53,
    OTTRecognition = 54,
    InternationalQualifiedTeacherStatus = 55,
    GTP = 100,
    RTP = 101,
    OTT = 102,
    OTTExemptFromInduction = 103,
    TeachFirst = 104,
    EEA = 105,
    Scotland = 106,
    NorthernIreland = 107,
    FE = 108,
    ISC = 109,
    FlexiblePGCE = 110,
    FlexibleAssessmentOnly = 111,
    LicensedTeacherProgramme = 112,
    FlexibleProfGCE = 113,
    EYTSOnly = 114,
    EYTSPlusAcademicAward = 115,
    ETYSAssessmentOnly = 116,
    PGCTeachersForTheDeaf = 117,
    Degree = 400,
    HigherDegree = 401,
    DegreeEquivalent = 402,
    NoQualificationRestrictedByOtherGTC = 998,
    Unknown = 999
}

public static class IttQualificationTypeExtensions
{
    public static string GetIttQualificationValue(this IttQualificationType ittQualificationType) => ittQualificationType switch
    {
        IttQualificationType.BEd => "001",
        IttQualificationType.BEdHons => "002",
        IttQualificationType.BSc => "003",
        IttQualificationType.BScHons => "004",
        IttQualificationType.BTech_Education => "005",
        IttQualificationType.BTechHons_Education => "006",
        IttQualificationType.BA => "007",
        IttQualificationType.BAHons => "008",
        IttQualificationType.BACombinedStudies_EducationOfTheDeaf => "009",
        IttQualificationType.BAHonsCombinedStudies_EducationOfTheDeaf => "010",
        IttQualificationType.BAEducationQTS => "011",
        IttQualificationType.BAWithIntercalatedPGCE => "012",
        IttQualificationType.BScHonsWithIntercalatedPGCE => "013",
        IttQualificationType.BAHonsWithIntercalatedPGCE => "014",
        IttQualificationType.BSc_EducationQTS => "015",
        IttQualificationType.BEngHons_Education => "016",
        IttQualificationType.BSc_CertificateInEducationQTS => "017",
        IttQualificationType.BA_CertificateInEducationQTS => "018",
        IttQualificationType.PostgraduateCertificateInEducationFlexible => "019",
        IttQualificationType.PostgraduateCertificateInEducation => "020",
        IttQualificationType.PostgraduateDiplomaInEducation => "021",
        IttQualificationType.PostgraduateArtTeachersCertificate => "022",
        IttQualificationType.PostgraduateArtTeachersDiploma => "023",
        IttQualificationType.GraduateCertificateInScienceAndEducation => "024",
        IttQualificationType.GraduateCertificateInMathematicsAndEducation => "025",
        IttQualificationType.PGCEArticledTeachersScheme => "026",
        IttQualificationType.QTSAwardOnly => "027",
        IttQualificationType.UndergraduateMasterOfTeaching => "028",
        IttQualificationType.QTSAssessmentOnly => "029",
        IttQualificationType.CertificateInEducation => "030",
        IttQualificationType.ProfessionalGraduateCertificateInEducation => "031",
        IttQualificationType.MastersNotByResearch => "032",
        IttQualificationType.GraduateCertificateInEducation => "033",
        IttQualificationType.CertificateInEducationFurtherEducation => "040",
        IttQualificationType.PostgraduateCertificateInEducationFurtherEducation => "041",
        IttQualificationType.TeachersCertificateFurtherEducation => "042",
        IttQualificationType.TeachersCertificate => "043",
        IttQualificationType.QualificationGainedInEurope => "049",
        IttQualificationType.ProfessionalGraduateDiplomaInEducation => "050",
        IttQualificationType.AssessmentOnlyRoute => "051",
        IttQualificationType.TeachFirstTNP => "052",
        IttQualificationType.TroopsToTeach => "053",
        IttQualificationType.OTTRecognition => "054",
        IttQualificationType.InternationalQualifiedTeacherStatus => "055",
        IttQualificationType.GTP => "100",
        IttQualificationType.RTP => "101",
        IttQualificationType.OTT => "102",
        IttQualificationType.OTTExemptFromInduction => "103",
        IttQualificationType.TeachFirst => "104",
        IttQualificationType.EEA => "105",
        IttQualificationType.Scotland => "106",
        IttQualificationType.NorthernIreland => "107",
        IttQualificationType.FE => "108",
        IttQualificationType.ISC => "109",
        IttQualificationType.FlexiblePGCE => "110",
        IttQualificationType.FlexibleAssessmentOnly => "111",
        IttQualificationType.LicensedTeacherProgramme => "112",
        IttQualificationType.FlexibleProfGCE => "113",
        IttQualificationType.EYTSOnly => "114",
        IttQualificationType.EYTSPlusAcademicAward => "115",
        IttQualificationType.ETYSAssessmentOnly => "116",
        IttQualificationType.PGCTeachersForTheDeaf => "117",
        IttQualificationType.Degree => "400",
        IttQualificationType.HigherDegree => "401",
        IttQualificationType.DegreeEquivalent => "402",
        IttQualificationType.NoQualificationRestrictedByOtherGTC => "998",
        IttQualificationType.Unknown => "999",
        _ => throw new FormatException($"Unknown {nameof(ittQualificationType)}: '{ittQualificationType}'.")
    };
}

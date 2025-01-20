namespace TeachingRecordSystem.Core.Models;

public enum RouteToProfessionalStatus2
{
    [RouteToProfessionalStatusInfo("Apply for QTS", QualificationType.QualifiedTeacherStatus)]
    ApplyForQts,
    [RouteToProfessionalStatusInfo("Apprenticeship", QualificationType.QualifiedTeacherStatus)]
    Apprenticeship,
    [RouteToProfessionalStatusInfo("Assessment Only Route", QualificationType.QualifiedTeacherStatus)]
    AssessmentOnlyRoute,  // Do we need the 'Route' suffix here?
    [RouteToProfessionalStatusInfo("Authorised Teacher Programme", QualificationType.QualifiedTeacherStatus)]
    AuthorisedTeacherProgramme,  // Nit: English/American spelling?
    [RouteToProfessionalStatusInfo("Core - Core Programme Type", QualificationType.QualifiedTeacherStatus)]
    CoreProgrammeType,
    [RouteToProfessionalStatusInfo("Core Flexible", QualificationType.QualifiedTeacherStatus)]
    CoreFlexible,
    [RouteToProfessionalStatusInfo("CTC or CCTA", QualificationType.QualifiedTeacherStatus)]
    CtcOrCcta,
    [RouteToProfessionalStatusInfo("Early Years ITT Assessment Only", QualificationType.EarlyYearsTeacherStatus)]
    EarlyYearsIttAssessmentOnly,
    [RouteToProfessionalStatusInfo("Early Years ITT Graduate Employment Based", QualificationType.EarlyYearsTeacherStatus)]
    EarlyYearsIttGraduateEmploymentBased,
    [RouteToProfessionalStatusInfo("Early Years ITT Graduate Entry", QualificationType.EarlyYearsTeacherStatus)]
    EarlyYearsIttGraduateEntry,
    [RouteToProfessionalStatusInfo("Early Years ITT School Direct", QualificationType.EarlyYearsTeacherStatus)]
    EarlyYearsIttSchoolDirect,
    [RouteToProfessionalStatusInfo("Early Years ITT Undergraduate", QualificationType.EarlyYearsTeacherStatus)]
    EarlyYearsIttUndergraduate,
    [RouteToProfessionalStatusInfo("EC directive", QualificationType.QualifiedTeacherStatus)]
    EcDirective,
    [RouteToProfessionalStatusInfo("European Recognition", QualificationType.QualifiedTeacherStatus)]
    EuropeanRecognition,
    [RouteToProfessionalStatusInfo("European Recognition - PQTS", QualificationType.PartialQualifiedTeacherStatus)]
    EuropeanRecognitionPqts,
    [RouteToProfessionalStatusInfo("EYPS", QualificationType.EarlyYearsProfessionalStatus)]
    Eyps,
    [RouteToProfessionalStatusInfo("EYPS ITT Migrated", QualificationType.EarlyYearsProfessionalStatus)]
    EypsIttMigrated,
    [RouteToProfessionalStatusInfo("EYTS ITT Migrated", QualificationType.EarlyYearsTeacherStatus)]
    EytsIttMigrated,
    [RouteToProfessionalStatusInfo("FE Recognition 2000-2004", QualificationType.QualifiedTeacherStatus)]
    FeRecognition2000To2004,
    [RouteToProfessionalStatusInfo("Flexible ITT", QualificationType.QualifiedTeacherStatus)]
    FlexibleItt,
    [RouteToProfessionalStatusInfo("Future Teaching Scholars", QualificationType.QualifiedTeacherStatus)]
    FutureTeachingScholars,
    [RouteToProfessionalStatusInfo("Graduate non-trained", QualificationType.QualifiedTeacherStatus)]
    GraduateNonTrained,
    [RouteToProfessionalStatusInfo("Graduate Teacher Programme", QualificationType.QualifiedTeacherStatus)]
    GraduateTeacherProgramme,
    [RouteToProfessionalStatusInfo("HEI - HEI Programme Type", QualificationType.QualifiedTeacherStatus)]
    HeiProgrammeType,
    [RouteToProfessionalStatusInfo("HEI - Historic", QualificationType.QualifiedTeacherStatus)]
    HeiHistoric,
    [RouteToProfessionalStatusInfo("High Potential ITT", QualificationType.QualifiedTeacherStatus)]
    HighPotentialItt,
    [RouteToProfessionalStatusInfo("International Qualified Teacher Status", QualificationType.QualifiedTeacherStatus)]
    InternationalQualifiedTeacherStatus,
    [RouteToProfessionalStatusInfo("Legacy ITT", QualificationType.QualifiedTeacherStatus)]
    LegacyItt,
    [RouteToProfessionalStatusInfo("Legacy Migration", QualificationType.QualifiedTeacherStatus)]
    LegacyMigration,
    [RouteToProfessionalStatusInfo("Licensed Teacher Programme", QualificationType.QualifiedTeacherStatus)]
    LicensedTeacherProgramme,
    [RouteToProfessionalStatusInfo("Licensed Teacher Programme - Armed Forces", QualificationType.QualifiedTeacherStatus)]
    LicensedTeacherProgrammeArmedForces,
    [RouteToProfessionalStatusInfo("Licensed Teacher Programme - FE", QualificationType.QualifiedTeacherStatus)]
    LicensedTeacherProgrammeFe,
    [RouteToProfessionalStatusInfo("Licensed Teacher Programme - Independent School", QualificationType.QualifiedTeacherStatus)]
    LicensedTeacherProgrammeIndependentSchool,
    [RouteToProfessionalStatusInfo("Licensed Teacher Programme - Maintained School", QualificationType.QualifiedTeacherStatus)]
    LicensedTeacherProgrammeMaintainedSchool,
    [RouteToProfessionalStatusInfo("Licensed Teacher Programme - OTT", QualificationType.QualifiedTeacherStatus)]
    LicensedTeacherProgrammeOtt,
    [RouteToProfessionalStatusInfo("Long Service", QualificationType.QualifiedTeacherStatus)]
    LongService,
    [RouteToProfessionalStatusInfo("NI R", QualificationType.QualifiedTeacherStatus)]
    NiR,
    [RouteToProfessionalStatusInfo("Other Qualifications non ITT", QualificationType.QualifiedTeacherStatus)]
    OtherQualificationsNonItt,
    [RouteToProfessionalStatusInfo("Overseas Trained Teacher Programme", QualificationType.QualifiedTeacherStatus)]
    OverseasTrainedTeacherProgramme,
    [RouteToProfessionalStatusInfo("Overseas Trained Teacher Recognition", QualificationType.QualifiedTeacherStatus)]
    OverseasTrainedTeacherRecognition,
    [RouteToProfessionalStatusInfo("PGATC ITT", QualificationType.QualifiedTeacherStatus)]
    PgatcItt,
    [RouteToProfessionalStatusInfo("PGATD ITT", QualificationType.QualifiedTeacherStatus)]
    PgatcdItt,
    [RouteToProfessionalStatusInfo("PGCE ITT", QualificationType.QualifiedTeacherStatus)]
    PgceItt,
    [RouteToProfessionalStatusInfo("PGDE ITT", QualificationType.QualifiedTeacherStatus)]
    PgdeItt,
    [RouteToProfessionalStatusInfo("Primary and secondary postgraduate fee funded", QualificationType.QualifiedTeacherStatus)]
    PrimaryAndSecondaryPostgraduateFeeFunded,
    [RouteToProfessionalStatusInfo("Primary and secondary undergraduate fee funded", QualificationType.QualifiedTeacherStatus)]
    PrimaryAndSecondaryUndergraduateFeeFunded,
    [RouteToProfessionalStatusInfo("ProfGCE ITT", QualificationType.QualifiedTeacherStatus)]
    ProfGceItt,
    [RouteToProfessionalStatusInfo("ProfGDE ITT", QualificationType.QualifiedTeacherStatus)]
    ProfGdeItt,
    [RouteToProfessionalStatusInfo("Provider led Postgrad", QualificationType.QualifiedTeacherStatus)]
    ProviderLedPostgrad,
    [RouteToProfessionalStatusInfo("Provider led Undergrad", QualificationType.QualifiedTeacherStatus)]
    ProviderLedUndergrad,
    [RouteToProfessionalStatusInfo("QTLS and SET Membership", QualificationType.QualifiedTeacherStatus)]
    QtlsAndSetMembership,
    [RouteToProfessionalStatusInfo("Registered Teacher Programme", QualificationType.QualifiedTeacherStatus)]
    RegisteredTeacherProgramme,
    [RouteToProfessionalStatusInfo("School Centered ITT", QualificationType.QualifiedTeacherStatus)]
    SchoolCenteredItt,
    [RouteToProfessionalStatusInfo("School Direct Training Programme", QualificationType.QualifiedTeacherStatus)]
    SchoolDirectTrainingProgramme,
    [RouteToProfessionalStatusInfo("School Direct Training Programme Salaried", QualificationType.QualifiedTeacherStatus)]
    SchoolDirectTrainingProgrammeSalaried,
    [RouteToProfessionalStatusInfo("School Direct Training Programme Self Funded", QualificationType.QualifiedTeacherStatus)]
    SchoolDirectTrainingProgrammeSelfFunded,
    [RouteToProfessionalStatusInfo("Scotland R", QualificationType.QualifiedTeacherStatus)]
    ScotlandR,
    [RouteToProfessionalStatusInfo("TC ITT", QualificationType.QualifiedTeacherStatus)]
    TcItt,
    [RouteToProfessionalStatusInfo("TCMH", QualificationType.QualifiedTeacherStatus)]
    Tcmh,
    [RouteToProfessionalStatusInfo("Teach First Programme", QualificationType.QualifiedTeacherStatus)]
    TeachFirstProgramme,
    [RouteToProfessionalStatusInfo("Troops to Teach", QualificationType.QualifiedTeacherStatus)]
    TroopsToTeach,
    [RouteToProfessionalStatusInfo("UGMT ITT", QualificationType.QualifiedTeacherStatus)]
    UgmtItt,
    [RouteToProfessionalStatusInfo("Undergraduate Opt In", QualificationType.QualifiedTeacherStatus)]
    UndergraduateOptIn,
    [RouteToProfessionalStatusInfo("Welsh R", QualificationType.QualifiedTeacherStatus)]
    WelshR,
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class RouteToProfessionalStatusInfoAttribute(string name, QualificationType qualificationType) : Attribute
{
    public string Name => name;
    public QualificationType QualificationType => qualificationType;
}

namespace TeachingRecordSystem.Core.Models;

public enum RouteToProfessionalStatus2
{
    [RouteToProfessionalStatusInfo("Apply for QTS", ProfessionalStatusType.QualifiedTeacherStatus)]
    ApplyForQts,
    [RouteToProfessionalStatusInfo("Apprenticeship", ProfessionalStatusType.QualifiedTeacherStatus)]
    Apprenticeship,
    [RouteToProfessionalStatusInfo("Assessment Only Route", ProfessionalStatusType.QualifiedTeacherStatus)]
    AssessmentOnlyRoute,  // Do we need the 'Route' suffix here?
    [RouteToProfessionalStatusInfo("Authorised Teacher Programme", ProfessionalStatusType.QualifiedTeacherStatus)]
    AuthorisedTeacherProgramme,  // Nit: English/American spelling?
    [RouteToProfessionalStatusInfo("Core - Core Programme Type", ProfessionalStatusType.QualifiedTeacherStatus)]
    CoreProgrammeType,
    [RouteToProfessionalStatusInfo("Core Flexible", ProfessionalStatusType.QualifiedTeacherStatus)]
    CoreFlexible,
    [RouteToProfessionalStatusInfo("CTC or CCTA", ProfessionalStatusType.QualifiedTeacherStatus)]
    CtcOrCcta,
    [RouteToProfessionalStatusInfo("Early Years ITT Assessment Only", ProfessionalStatusType.EarlyYearsTeacherStatus)]
    EarlyYearsIttAssessmentOnly,
    [RouteToProfessionalStatusInfo("Early Years ITT Graduate Employment Based", ProfessionalStatusType.EarlyYearsTeacherStatus)]
    EarlyYearsIttGraduateEmploymentBased,
    [RouteToProfessionalStatusInfo("Early Years ITT Graduate Entry", ProfessionalStatusType.EarlyYearsTeacherStatus)]
    EarlyYearsIttGraduateEntry,
    [RouteToProfessionalStatusInfo("Early Years ITT School Direct", ProfessionalStatusType.EarlyYearsTeacherStatus)]
    EarlyYearsIttSchoolDirect,
    [RouteToProfessionalStatusInfo("Early Years ITT Undergraduate", ProfessionalStatusType.EarlyYearsTeacherStatus)]
    EarlyYearsIttUndergraduate,
    [RouteToProfessionalStatusInfo("EC directive", ProfessionalStatusType.QualifiedTeacherStatus)]
    EcDirective,
    [RouteToProfessionalStatusInfo("European Recognition", ProfessionalStatusType.QualifiedTeacherStatus)]
    EuropeanRecognition,
    [RouteToProfessionalStatusInfo("European Recognition - PQTS", ProfessionalStatusType.PartialQualifiedTeacherStatus)]
    EuropeanRecognitionPqts,
    [RouteToProfessionalStatusInfo("EYPS", ProfessionalStatusType.EarlyYearsProfessionalStatus)]
    Eyps,
    [RouteToProfessionalStatusInfo("EYPS ITT Migrated", ProfessionalStatusType.EarlyYearsProfessionalStatus)]
    EypsIttMigrated,
    [RouteToProfessionalStatusInfo("EYTS ITT Migrated", ProfessionalStatusType.EarlyYearsTeacherStatus)]
    EytsIttMigrated,
    [RouteToProfessionalStatusInfo("FE Recognition 2000-2004", ProfessionalStatusType.QualifiedTeacherStatus)]
    FeRecognition2000To2004,
    [RouteToProfessionalStatusInfo("Flexible ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    FlexibleItt,
    [RouteToProfessionalStatusInfo("Future Teaching Scholars", ProfessionalStatusType.QualifiedTeacherStatus)]
    FutureTeachingScholars,
    [RouteToProfessionalStatusInfo("Graduate non-trained", ProfessionalStatusType.QualifiedTeacherStatus)]
    GraduateNonTrained,
    [RouteToProfessionalStatusInfo("Graduate Teacher Programme", ProfessionalStatusType.QualifiedTeacherStatus)]
    GraduateTeacherProgramme,
    [RouteToProfessionalStatusInfo("HEI - HEI Programme Type", ProfessionalStatusType.QualifiedTeacherStatus)]
    HeiProgrammeType,
    [RouteToProfessionalStatusInfo("HEI - Historic", ProfessionalStatusType.QualifiedTeacherStatus)]
    HeiHistoric,
    [RouteToProfessionalStatusInfo("High Potential ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    HighPotentialItt,
    [RouteToProfessionalStatusInfo("International Qualified Teacher Status", ProfessionalStatusType.QualifiedTeacherStatus)]
    InternationalQualifiedTeacherStatus,
    [RouteToProfessionalStatusInfo("Legacy ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    LegacyItt,
    [RouteToProfessionalStatusInfo("Legacy Migration", ProfessionalStatusType.QualifiedTeacherStatus)]
    LegacyMigration,
    [RouteToProfessionalStatusInfo("Licensed Teacher Programme", ProfessionalStatusType.QualifiedTeacherStatus)]
    LicensedTeacherProgramme,
    [RouteToProfessionalStatusInfo("Licensed Teacher Programme - Armed Forces", ProfessionalStatusType.QualifiedTeacherStatus)]
    LicensedTeacherProgrammeArmedForces,
    [RouteToProfessionalStatusInfo("Licensed Teacher Programme - FE", ProfessionalStatusType.QualifiedTeacherStatus)]
    LicensedTeacherProgrammeFe,
    [RouteToProfessionalStatusInfo("Licensed Teacher Programme - Independent School", ProfessionalStatusType.QualifiedTeacherStatus)]
    LicensedTeacherProgrammeIndependentSchool,
    [RouteToProfessionalStatusInfo("Licensed Teacher Programme - Maintained School", ProfessionalStatusType.QualifiedTeacherStatus)]
    LicensedTeacherProgrammeMaintainedSchool,
    [RouteToProfessionalStatusInfo("Licensed Teacher Programme - OTT", ProfessionalStatusType.QualifiedTeacherStatus)]
    LicensedTeacherProgrammeOtt,
    [RouteToProfessionalStatusInfo("Long Service", ProfessionalStatusType.QualifiedTeacherStatus)]
    LongService,
    [RouteToProfessionalStatusInfo("NI R", ProfessionalStatusType.QualifiedTeacherStatus)]
    NiR,
    [RouteToProfessionalStatusInfo("Other Qualifications non ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    OtherQualificationsNonItt,
    [RouteToProfessionalStatusInfo("Overseas Trained Teacher Programme", ProfessionalStatusType.QualifiedTeacherStatus)]
    OverseasTrainedTeacherProgramme,
    [RouteToProfessionalStatusInfo("Overseas Trained Teacher Recognition", ProfessionalStatusType.QualifiedTeacherStatus)]
    OverseasTrainedTeacherRecognition,
    [RouteToProfessionalStatusInfo("PGATC ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    PgatcItt,
    [RouteToProfessionalStatusInfo("PGATD ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    PgatcdItt,
    [RouteToProfessionalStatusInfo("PGCE ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    PgceItt,
    [RouteToProfessionalStatusInfo("PGDE ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    PgdeItt,
    [RouteToProfessionalStatusInfo("Primary and secondary postgraduate fee funded", ProfessionalStatusType.QualifiedTeacherStatus)]
    PrimaryAndSecondaryPostgraduateFeeFunded,
    [RouteToProfessionalStatusInfo("Primary and secondary undergraduate fee funded", ProfessionalStatusType.QualifiedTeacherStatus)]
    PrimaryAndSecondaryUndergraduateFeeFunded,
    [RouteToProfessionalStatusInfo("ProfGCE ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    ProfGceItt,
    [RouteToProfessionalStatusInfo("ProfGDE ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    ProfGdeItt,
    [RouteToProfessionalStatusInfo("Provider led Postgrad", ProfessionalStatusType.QualifiedTeacherStatus)]
    ProviderLedPostgrad,
    [RouteToProfessionalStatusInfo("Provider led Undergrad", ProfessionalStatusType.QualifiedTeacherStatus)]
    ProviderLedUndergrad,
    [RouteToProfessionalStatusInfo("QTLS and SET Membership", ProfessionalStatusType.QualifiedTeacherStatus)]
    QtlsAndSetMembership,
    [RouteToProfessionalStatusInfo("Registered Teacher Programme", ProfessionalStatusType.QualifiedTeacherStatus)]
    RegisteredTeacherProgramme,
    [RouteToProfessionalStatusInfo("School Centered ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    SchoolCenteredItt,
    [RouteToProfessionalStatusInfo("School Direct Training Programme", ProfessionalStatusType.QualifiedTeacherStatus)]
    SchoolDirectTrainingProgramme,
    [RouteToProfessionalStatusInfo("School Direct Training Programme Salaried", ProfessionalStatusType.QualifiedTeacherStatus)]
    SchoolDirectTrainingProgrammeSalaried,
    [RouteToProfessionalStatusInfo("School Direct Training Programme Self Funded", ProfessionalStatusType.QualifiedTeacherStatus)]
    SchoolDirectTrainingProgrammeSelfFunded,
    [RouteToProfessionalStatusInfo("Scotland R", ProfessionalStatusType.QualifiedTeacherStatus)]
    ScotlandR,
    [RouteToProfessionalStatusInfo("TC ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    TcItt,
    [RouteToProfessionalStatusInfo("TCMH", ProfessionalStatusType.QualifiedTeacherStatus)]
    Tcmh,
    [RouteToProfessionalStatusInfo("Teach First Programme", ProfessionalStatusType.QualifiedTeacherStatus)]
    TeachFirstProgramme,
    [RouteToProfessionalStatusInfo("Troops to Teach", ProfessionalStatusType.QualifiedTeacherStatus)]
    TroopsToTeach,
    [RouteToProfessionalStatusInfo("UGMT ITT", ProfessionalStatusType.QualifiedTeacherStatus)]
    UgmtItt,
    [RouteToProfessionalStatusInfo("Undergraduate Opt In", ProfessionalStatusType.QualifiedTeacherStatus)]
    UndergraduateOptIn,
    [RouteToProfessionalStatusInfo("Welsh R", ProfessionalStatusType.QualifiedTeacherStatus)]
    WelshR,
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class RouteToProfessionalStatusInfoAttribute(string name, ProfessionalStatusType professionalStatusType) : Attribute
{
    public string Name => name;
    public ProfessionalStatusType ProfessionalStatusType => professionalStatusType;
}

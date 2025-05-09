namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class RouteToProfessionalStatusType
{
    public const string InductionExemptionReasonIdIndexName = "ix_route_to_professional_status_induction_exemption_reason_id";
    public const string InductionExemptionReasonForeignKeyName = "fk_route_to_professional_status_induction_exemption_reason";

    public static Guid ApplyForQtsId { get; } = new("6F27BDEB-D00A-4EF9-B0EA-26498CE64713");
    public static Guid AssessmentOnlyRouteId { get; } = new("57B86CEF-98E2-4962-A74A-D47C7A34B838");
    public static Guid EuropeanRecognitionId { get; } = new("2B106B9D-BA39-4E2D-A42E-0CE827FDC324");
    public static Guid HeiProgrammeTypeId { get; } = new("10078157-E8C3-42F7-A050-D8B802E83F7B");
    public static Guid SchoolDirectTrainingProgrammeId { get; } = new("D9490E58-ACDC-4A38-B13E-5A5C21417737");
    public static Guid InternationalQualifiedTeacherStatusId { get; } = new("D0B60864-AB1C-4D49-A5C2-FF4BD9872EE1");
    public static Guid NiRId { get; } = new("3604EF30-8F11-4494-8B52-A2F9C5371E03");
    public static Guid OverseasTrainedTeacherRecognitionId { get; } = new("CE61056E-E681-471E-AF48-5FFBF2653500");
    public static Guid QtlsAndSetMembershipId { get; } = new("BE6EAF8C-92DD-4EFF-AAD3-1C89C4BEC18C");
    public static Guid ScotlandRId { get; } = new("52835B1F-1F2E-4665-ABC6-7FB1EF0A80BB");
    public static Guid EarlyYearsIttGraduateEntryId { get; } = new("DBC4125B-9235-41E4-ABD2-BAABBF63F829");
    public static Guid EarlyYearsIttUndergraduateId { get; } = new("C97C0FD2-FD84-4949-97C7-B0E2422FB3C8");
    public static Guid WelshRId { get; } = new("877ba701-fe26-4951-9f15-171f3755d50d");
    public static Guid ECDirective { get; } = new("f4da123b-5c37-4060-ab00-52de4bd3599e");

    public required Guid RouteToProfessionalStatusTypeId { get; init; }
    public required string Name { get; init; }
    public required ProfessionalStatusType ProfessionalStatusType { get; init; }
    public required bool IsActive { get; set; }
    public required FieldRequirement TrainingStartDateRequired { get; init; }
    public required FieldRequirement TrainingEndDateRequired { get; init; }
    public required FieldRequirement HoldsFromRequired { get; init; }
    public required FieldRequirement InductionExemptionRequired { get; init; }
    public required FieldRequirement TrainingProviderRequired { get; init; }
    public required FieldRequirement DegreeTypeRequired { get; init; }
    public required FieldRequirement TrainingCountryRequired { get; init; }
    public required FieldRequirement TrainingAgeSpecialismTypeRequired { get; init; }
    public required FieldRequirement TrainingSubjectsRequired { get; init; }
    public InductionExemptionReason? InductionExemptionReason { get; }
    public required Guid? InductionExemptionReasonId { get; init; }
}

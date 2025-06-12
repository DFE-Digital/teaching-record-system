namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class InductionExemptionReason
{
    public static Guid OverseasTrainedTeacherId { get; } = new("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9");
    public static Guid PassedInductionInNorthernIrelandId { get; } = new("3471ab35-e6e4-4fa9-a72b-b8bd113df591");
    public static Guid HasOrIsEligibleForFullRegistrationInScotlandId { get; } = new("a112e691-1694-46a7-8f33-5ec5b845c181");
    public static Guid PassedInWalesId { get; } = new("39550fa9-3147-489d-b808-4feea7f7f979");
    public static Guid QtlsId { get; } = new("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db");
    public static Guid QualifiedBefore07May2000Id = new("5a80cee8-98a8-426b-8422-b0e81cb49b36");
    public static Guid QualifiedBetween07May1999And01April2003FirstPostInWalesId = new("15014084-2d8d-4f51-9198-b0e1881f8896");
    public static Guid QualifiedThroughFurtherEducationRouteBetween1Sep2001And1Sep2004Id = new("0997ab13-7412-4560-8191-e51ed4d58d2a");
    public static Guid PassedInductionInGuernseyId = new("fea2db23-93e0-49af-96fd-83c815c17c0b");
    public static Guid PassedInductionInIsleOfManId = new("e5c3847d-8fb6-4b31-8726-812392da8c5c");
    public static Guid PassedInductionInJerseyId = new("243b21a8-0be4-4af5-8874-85944357e7f8");
    public static Guid PassedInductionInServiceChildrensEducationSchoolsInGermanyOrCyprusId = new("7d17d904-c1c6-451b-9e09-031314bd35f7");
    public static Guid PassedProbationaryPeriodInGibraltarId = new("a751494a-7e7a-4836-96cb-00b9ed6e1b5f");
    public static Guid ExemptId = new("a5faff9f-29ce-4a6b-a7b8-0c1f57f15920");
    public static Guid ExemptDataLossOrErrorCriteriaId = new("204f86eb-0383-40eb-b793-6fccb76ecee2");
    public static Guid QualifiedThroughEEAMutualRecognitionRouteId = new("e7118bab-c2b1-4fe8-ad3f-4095d73f5b85");
    public static Guid RegisteredTeacherWithAtLeast2YearsFullTimeTeachingExperienceneId = new("42bb7bbc-a92c-4886-b319-3c1a5eac319a");

    public required Guid InductionExemptionReasonId { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; set; }
    public required bool RouteImplicitExemption { get; set; }
    public required bool RouteOnlyExemption { get; init; }
}

using System.ComponentModel.DataAnnotations;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public enum ExemptionReasonCategory
{
    [Display(Name = "Miscellaneous exemptions")]
    Miscellaneous = 1,
    [Display(Name = "Exemptions for historical qualification routes")]
    HistoricalQualificationRoute = 2,
    [Display(Name = "Induction completed outside England")]
    InductionCompletedOutsideEngland = 3,
    None = 4
}

public static class ExemptionReasonCategories
{
    private static Dictionary<Guid, ExemptionReasonCategory> ExemptionReasonCategoryMapOld => new()
    {
        { new Guid("0997ab13-7412-4560-8191-e51ed4d58d2a"), ExemptionReasonCategory.Miscellaneous },
        { new Guid("15014084-2d8d-4f51-9198-b0e1881f8896"), ExemptionReasonCategory.HistoricalQualificationRoute },
        { new Guid("204f86eb-0383-40eb-b793-6fccb76ecee2"), ExemptionReasonCategory.Miscellaneous },
        { new Guid("243b21a8-0be4-4af5-8874-85944357e7f8"), ExemptionReasonCategory.InductionCompletedOutsideEngland },
        { new Guid("3471ab35-e6e4-4fa9-a72b-b8bd113df591"), ExemptionReasonCategory.InductionCompletedOutsideEngland },
        { InductionExemptionReason.QtlsId, ExemptionReasonCategory.None },
        { new Guid("39550fa9-3147-489d-b808-4feea7f7f979"), ExemptionReasonCategory.InductionCompletedOutsideEngland },
        { new Guid("42bb7bbc-a92c-4886-b319-3c1a5eac319a"), ExemptionReasonCategory.HistoricalQualificationRoute },
        { InductionExemptionReason.OverseasTrainedTeacherId, ExemptionReasonCategory.None },
        { new Guid("5a80cee8-98a8-426b-8422-b0e81cb49b36"), ExemptionReasonCategory.HistoricalQualificationRoute },
        { new Guid("7d17d904-c1c6-451b-9e09-031314bd35f7"), ExemptionReasonCategory.InductionCompletedOutsideEngland },
        { new Guid("a112e691-1694-46a7-8f33-5ec5b845c181"), ExemptionReasonCategory.Miscellaneous },
        { new Guid("a5faff9f-29ce-4a6b-a7b8-0c1f57f15920"), ExemptionReasonCategory.Miscellaneous},
        { new Guid("a751494a-7e7a-4836-96cb-00b9ed6e1b5f"), ExemptionReasonCategory.InductionCompletedOutsideEngland },
        { new Guid("e5c3847d-8fb6-4b31-8726-812392da8c5c"), ExemptionReasonCategory.InductionCompletedOutsideEngland},
        { new Guid("e7118bab-c2b1-4fe8-ad3f-4095d73f5b85"), ExemptionReasonCategory.Miscellaneous },
        { new Guid("fea2db23-93e0-49af-96fd-83c815c17c0b"), ExemptionReasonCategory.InductionCompletedOutsideEngland }
    };

    private static Dictionary<ExemptionReasonCategory, IEnumerable<Guid>> ExemptionReasonCategoryMap => new()
    {
        { ExemptionReasonCategory.Miscellaneous, new List<Guid> {
            new Guid("0997ab13-7412-4560-8191-e51ed4d58d2a"),
            new Guid("204f86eb-0383-40eb-b793-6fccb76ecee2"),
            new Guid("a112e691-1694-46a7-8f33-5ec5b845c181"),
            new Guid("a5faff9f-29ce-4a6b-a7b8-0c1f57f15920"),
            new Guid("e7118bab-c2b1-4fe8-ad3f-4095d73f5b85")} },
        { ExemptionReasonCategory.HistoricalQualificationRoute, new List<Guid> {
            new Guid("15014084-2d8d-4f51-9198-b0e1881f8896"),
            new Guid("42bb7bbc-a92c-4886-b319-3c1a5eac319a"),
            new Guid("5a80cee8-98a8-426b-8422-b0e81cb49b36")} },
        { ExemptionReasonCategory.InductionCompletedOutsideEngland, new List<Guid> {
            new Guid("243b21a8-0be4-4af5-8874-85944357e7f8"),
            new Guid("3471ab35-e6e4-4fa9-a72b-b8bd113df591"),
            new Guid("39550fa9-3147-489d-b808-4feea7f7f979"),
            new Guid("7d17d904-c1c6-451b-9e09-031314bd35f7"),
            new Guid("a751494a-7e7a-4836-96cb-00b9ed6e1b5f"),
            new Guid("e5c3847d-8fb6-4b31-8726-812392da8c5c"),
            new Guid("fea2db23-93e0-49af-96fd-83c815c17c0b")} }
    };

    public static IEnumerable<Guid> GetExemptionReasonIdsForCategory(ExemptionReasonCategory category)
    {
        return ExemptionReasonCategoryMap.ContainsKey(category) ? ExemptionReasonCategoryMap[category] : new List<Guid>();
    }

    public static IEnumerable<Guid> ExemptionReasonIds =>
        ExemptionReasonCategoryMap.Values.SelectMany(g => g);

    public static IEnumerable<ExemptionReasonCategory> All => Enum.GetValues(typeof(ExemptionReasonCategory)).Cast<ExemptionReasonCategory>();
}

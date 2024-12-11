namespace TeachingRecordSystem.SupportUi.Pages.Induction.CompletionDate;

public class EditInductionCompletionDateState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditInductionCompletionDate,
        typeof(EditInductionCompletionDateState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public Guid? InductionId { get; set; }

    public DateOnly? StartDate { get; set; }
}

namespace TeachingRecordSystem.SupportUi.Pages.Induction.StartDate;

public class EditInductionStartDateState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditInductionStartDate,
        typeof(EditInductionStartDateState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public Guid? InductionId { get; set; }

    public DateOnly? StartDate { get; set; }

    public bool? IsComplete => StartDate.HasValue;
}

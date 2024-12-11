namespace TeachingRecordSystem.SupportUi.Pages.Induction.Status;

public class EditInductionStatusState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditInductionStatus,
        typeof(EditInductionStatusState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);
}

using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;
namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

public class AddAlertState : IRegisterJourney, IJourneyWithSteps
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.AddAlert,
        typeof(AddAlertState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public required JourneySteps Steps { get; init; }

    public Guid? AlertTypeId { get; set; }

    public string? AlertTypeName { get; set; }

    public string? Details { get; set; }

    public bool? AddLink { get; set; }

    public string? Link { get; set; }

    public DateOnly? StartDate { get; set; }

    public AddAlertReasonOption? AddReason { get; set; }

    public bool? HasAdditionalReasonDetail { get; set; }

    public string? AddReasonDetail { get; set; }

    public EvidenceUploadModel Evidence { get; set; } = new();

}
public class AddAlertStateJourneyStateFactory(SupportUiLinkGenerator linkGenerator) : IJourneyStateFactory<AddAlertState>
{
    public Task<AddAlertState> CreateAsync(CreateJourneyStateContext context)
    {
        var personId = context.HttpContext.GetCurrentPersonFeature().PersonId;
        var firstStep = new JourneyStep(linkGenerator.Alerts.AddAlert.Index(personId, context.InstanceId));
        var state = new AddAlertState { Steps = JourneySteps.Create(firstStep) };
        return Task.FromResult(state);
    }
}

using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

public class DeleteRouteState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.DeleteRouteToProfessionalStatus,
        typeof(DeleteRouteState),
        requestDataKeys: ["qualificationId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public bool Completed => ChangeReason is not null &&
        ChangeReasonDetail.IsComplete;

    public ChangeReasonOption? ChangeReason { get; set; }
    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();

    [JsonIgnore]
    public bool ChangeReasonIsComplete => ChangeReason is not null && ChangeReasonDetail is not null && ChangeReasonDetail.IsComplete;
}

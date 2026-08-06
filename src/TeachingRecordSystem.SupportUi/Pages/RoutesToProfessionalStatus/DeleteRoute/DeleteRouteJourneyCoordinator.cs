using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

[JourneyCoordinator(JourneyNames.DeleteRouteToProfessionalStatus, routeValueKeys: ["qualificationId"])]
public class DeleteRouteJourneyCoordinator(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : JourneyCoordinator<DeleteRouteState>
{
    public Guid PersonId => HttpContext.GetCurrentPersonFeature().PersonId;

    public string PageCaption => $"Delete route - {HttpContext.GetCurrentPersonFeature().Name}";

    public override DeleteRouteState GetStartingState() => new();

    // Returns the URL to send the user back to.
    public async Task<string> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(State.ChangeReasonDetail.Evidence.UploadedEvidenceFile);
        DeleteInstance();
        return linkGenerator.Persons.PersonDetail.Qualifications(PersonId);
    }
}

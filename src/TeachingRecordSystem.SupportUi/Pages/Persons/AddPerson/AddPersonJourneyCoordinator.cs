using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[JourneyCoordinator(JourneyNames.AddPerson, routeValueKeys: [])]
public class AddPersonJourneyCoordinator(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : JourneyCoordinator<AddPersonState>
{
    public override AddPersonState GetStartingState() => new();

    /// <summary>
    /// Discards the journey along with any evidence file uploaded during it and returns the URL to
    /// send the user back to.
    /// </summary>
    public async Task<string> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(State.Evidence.UploadedEvidenceFile);
        DeleteInstance();
        return linkGenerator.Persons.AddPerson.Index();
    }
}

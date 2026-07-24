using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[JourneyCoordinator(JourneyNames.SetStatus, routeValueKeys: ["personId", "targetStatus"])]
public class SetStatusJourneyCoordinator(
    PersonService personService,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : JourneyCoordinator<SetStatusState>
{
    public Guid PersonId => Guid.Parse(InstanceId.RouteValues["personId"]!.ToString()!);

    public PersonStatus TargetStatus => Enum.Parse<PersonStatus>(InstanceId.RouteValues["targetStatus"]!.ToString()!);

    public override SetStatusState GetStartingState() => new();

    /// <summary>
    /// Gets the person whose status is being changed, or <see langword="null"/> if no such record
    /// exists.
    /// </summary>
    public Task<Person?> GetPersonAsync() =>
        personService.GetPersonAsync(PersonId, includeDeactivatedPersons: true);

    /// <summary>
    /// Gets whether the status change this journey is for still applies to <paramref name="person"/>.
    /// </summary>
    /// <remarks>
    /// A person can't be reactivated if they were deactivated as part of a merge where they were
    /// merged into another person (i.e. they were the secondary person).
    /// </remarks>
    public bool StatusChangeIsApplicable(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        return person.Status != TargetStatus &&
            !(person.Status == PersonStatus.Deactivated && person.MergedWithPersonId is not null);
    }

    /// <summary>
    /// Discards the journey along with any evidence file uploaded during it and returns the URL to
    /// send the user back to.
    /// </summary>
    public async Task<string> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(State.Evidence.UploadedEvidenceFile);
        DeleteInstance();
        return linkGenerator.Persons.PersonDetail.Index(PersonId);
    }
}

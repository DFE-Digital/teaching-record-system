using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public abstract class EditDetailsTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    /// <summary>
    /// Builds the state the coordinator starts the journey with — the person's details as both the
    /// original and the current values — optionally applying the edits a test is interested in.
    /// </summary>
    protected static EditDetailsState CreateState(CreatePersonResult person, Action<EditDetailsState>? configure = null)
    {
        var emailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(person.EmailAddress);
        var nationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(person.NationalInsuranceNumber);

        var state = new EditDetailsState
        {
            OriginalFirstName = person.FirstName,
            OriginalMiddleName = person.MiddleName,
            OriginalLastName = person.LastName,
            OriginalDateOfBirth = person.DateOfBirth,
            OriginalEmailAddress = emailAddress,
            OriginalNationalInsuranceNumber = nationalInsuranceNumber,
            OriginalGender = person.Gender,
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = person.Gender
        };

        configure?.Invoke(state);

        return state;
    }

    protected Task<EditDetailsJourneyCoordinator> CreateJourneyInstanceAsync(CreatePersonResult person, Action<EditDetailsState>? configure = null) =>
        CreateJourneyInstanceAsync(person.PersonId, CreateState(person, configure));

    protected Task<EditDetailsJourneyCoordinator> CreateJourneyInstanceAsync(Guid personId, EditDetailsState state) =>
        // Seed the path the journey would actually have taken for this state — the reason questions
        // are only asked when the corresponding details changed — so that back links and step
        // validation behave as they do in the real journey.
        JourneyHelper.CreateInstanceAsync<EditDetailsJourneyCoordinator>(
            JourneyNames.EditDetails,
            new RouteValueDictionary { ["personId"] = personId },
            _ => Task.FromResult<object>(state),
            pathUrls:
            [
                $"/persons/{personId}/edit-details",
                .. state.NameChanged ? new[] { $"/persons/{personId}/edit-details/name-change-reason" } : [],
                .. state.OtherDetailsChanged ? new[] { $"/persons/{personId}/edit-details/other-details-change-reason" } : [],
                $"/persons/{personId}/edit-details/check-answers",
            ],
            // The coordinator has constructor dependencies, so it can't be Activator-created.
            coordinatorFactory: () => ActivatorUtilities.CreateInstance<EditDetailsJourneyCoordinator>(HostFixture.Services));

    protected EditDetailsState? GetJourneyInstanceState(EditDetailsJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (EditDetailsState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}

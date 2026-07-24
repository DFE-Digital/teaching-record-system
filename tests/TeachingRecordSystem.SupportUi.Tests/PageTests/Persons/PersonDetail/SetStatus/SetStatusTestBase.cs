using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.SetStatus;

public class SetStatusTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task<CreatePersonResult> CreatePersonWithCurrentStatus(PersonStatus currentStatus, Action<CreatePersonBuilder>? configure = null)
    {
        configure ??= _ => { };

        var person = await TestData.CreatePersonAsync(configure);

        if (currentStatus == PersonStatus.Deactivated)
        {
            await WithDbContextAsync(async dbContext =>
            {
                dbContext.Attach(person.Person);
                person.Person.Status = PersonStatus.Deactivated;
                await dbContext.SaveChangesAsync();
            });
        }

        return person;
    }

    protected async Task<CreatePersonResult> CreatePersonToBecomeStatus(PersonStatus targetStatus, Action<CreatePersonBuilder>? configure = null)
    {
        configure ??= _ => { };

        var person = await TestData.CreatePersonAsync(configure);

        if (targetStatus == PersonStatus.Active)
        {
            await WithDbContextAsync(async dbContext =>
            {
                dbContext.Attach(person.Person);
                person.Person.Status = PersonStatus.Deactivated;
                await dbContext.SaveChangesAsync();
            });
        }

        return person;
    }

    public static TheoryData<PersonStatus> GetAllStatuses() =>
        new(PersonStatus.Active, PersonStatus.Deactivated);

    protected Task<SetStatusJourneyCoordinator> CreateJourneyInstanceAsync(Guid personId, PersonStatus targetStatus, SetStatusState? state = null) =>
        // Seed the whole journey path so that any page under test is reachable (the real journey builds
        // this path up as the user advances through the steps).
        JourneyHelper.CreateInstanceAsync<SetStatusJourneyCoordinator>(
            JourneyNames.SetStatus,
            new RouteValueDictionary { ["personId"] = personId, ["targetStatus"] = targetStatus },
            _ => Task.FromResult<object>(state ?? new SetStatusState()),
            pathUrls:
            [
                $"/persons/{personId}/set-status/{targetStatus}/reason",
                $"/persons/{personId}/set-status/{targetStatus}/check-answers",
            ],
            coordinatorFactory: CreateJourneyCoordinator<SetStatusJourneyCoordinator>);

    protected SetStatusState? GetJourneyInstanceState(SetStatusJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (SetStatusState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }
}

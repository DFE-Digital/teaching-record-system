using GovUk.Questions.AspNetCore.State;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TrnRequests.Resolve;

public abstract class ResolveApiTrnRequestTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task<ResolveTrnRequestState> CreateStateAsync(SupportTask supportTask)
    {
        // Resolve TrnRequestService from its own scope so that it doesn't share a DbContext with the
        // test's other operations.
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var trnRequestService = scope.ServiceProvider.GetRequiredService<TrnRequestService>();

        return await ResolveTrnRequestJourneyCoordinator.CreateStateAsync(trnRequestService, supportTask);
    }

    protected Task<ResolveTrnRequestJourneyCoordinator> CreateJourneyInstanceAsync(
        string supportTaskReference,
        ResolveTrnRequestState state)
    {
        var basePath = $"/support-tasks/trn-requests/{supportTaskReference}/resolve";

        // Seed the path the real journey would have built up by this point, so that every page is
        // reachable and back links match production. Creating a new record skips the Merge step —
        // there are no attribute sources to choose — so it must not appear in the path.
        string[] pathUrls = state.PersonId == ResolveTrnRequestState.CreateNewRecordPersonIdSentinel
            ? [$"{basePath}/matches", $"{basePath}/check-answers"]
            : [$"{basePath}/matches", $"{basePath}/merge", $"{basePath}/check-answers"];

        return JourneyHelper.CreateInstanceAsync<ResolveTrnRequestJourneyCoordinator>(
            JourneyNames.ResolveTrnRequest,
            new RouteValueDictionary { ["supportTaskReference"] = supportTaskReference },
            _ => Task.FromResult<object>(state),
            pathUrls: pathUrls,
            // JourneyHelper activates coordinators with Activator.CreateInstance, which can't supply
            // this one's constructor dependencies.
            coordinatorFactory: () => ActivatorUtilities.CreateInstance<ResolveTrnRequestJourneyCoordinator>(HostFixture.Services));
    }

    protected ResolveTrnRequestState? GetJourneyInstanceState(ResolveTrnRequestJourneyCoordinator coordinator)
    {
        var stateStorage = HostFixture.Services.GetRequiredService<IJourneyStateStorage>();
        return (ResolveTrnRequestState?)stateStorage.GetState(coordinator.InstanceId, coordinator.Journey)?.State;
    }

    protected async Task<(SupportTask SupportTask, TestData.CreatePersonResult MatchedPerson)> CreateSupportTaskWithAllDifferences(Guid applicationUserId)
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());

        var (supportTask, _, _) = await TestData.CreateTrnRequestSupportTaskAsync(
            applicationUserId,
            t => t
                .WithMatchedPersons(matchedPerson.PersonId)
                .WithFirstName(TestData.GenerateChangedFirstName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithMiddleName(TestData.GenerateChangedMiddleName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithLastName(TestData.GenerateChangedLastName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(matchedPerson.DateOfBirth))
                .WithEmailAddress(TestData.GenerateUniqueEmail())
                .WithNationalInsuranceNumber(TestData.GenerateChangedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber!))
                .WithGender(TestData.GenerateGender()));

        return (supportTask, matchedPerson);
    }

    protected async Task<(SupportTask SupportTask, TestData.CreatePersonResult MatchedPerson)> CreateSupportTaskWithSingleDifferenceToMatch(
        Guid applicationUserId,
        PersonMatchedAttribute differentAttribute,
        bool matchedPersonHasFlags = false)
    {
        var matchedPerson = await TestData.CreatePersonAsync(p =>
        {
            p
                .WithNationalInsuranceNumber()
                .WithEmailAddress(TestData.GenerateUniqueEmail())
                .WithGender(TestData.GenerateGender());

            if (matchedPersonHasFlags)
            {
                p.WithQts().WithEyts().WithAlert();
            }
        });

        var (supportTask, _, _) = await TestData.CreateTrnRequestSupportTaskAsync(
            applicationUserId,
            t => t
                .WithMatchedPersons(matchedPerson.PersonId)
                .WithFirstName(
                    differentAttribute != PersonMatchedAttribute.FirstName
                        ? matchedPerson.FirstName
                        : TestData.GenerateChangedFirstName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithMiddleName(
                    differentAttribute != PersonMatchedAttribute.MiddleName
                        ? matchedPerson.MiddleName
                        : TestData.GenerateChangedMiddleName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithLastName(
                    differentAttribute != PersonMatchedAttribute.LastName
                        ? matchedPerson.LastName
                        : TestData.GenerateChangedLastName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithDateOfBirth(
                    differentAttribute != PersonMatchedAttribute.DateOfBirth
                        ? matchedPerson.DateOfBirth
                        : TestData.GenerateChangedDateOfBirth(matchedPerson.DateOfBirth))
                .WithEmailAddress(
                    differentAttribute != PersonMatchedAttribute.EmailAddress
                        ? matchedPerson.EmailAddress
                        : TestData.GenerateUniqueEmail())
                .WithNationalInsuranceNumber(
                    differentAttribute != PersonMatchedAttribute.NationalInsuranceNumber
                        ? matchedPerson.NationalInsuranceNumber
                        : TestData.GenerateChangedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber!))
                .WithGender(
                    differentAttribute != PersonMatchedAttribute.Gender
                        ? matchedPerson.Gender
                        : TestData.GenerateChangedGender(matchedPerson.Gender)));

        return (supportTask, matchedPerson);
    }
}


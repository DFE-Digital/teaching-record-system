using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonMergingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectlyForRetainedPerson()
    {
        // Arrange
        var (retainedPerson, deactivatedPerson, process) = await CreateMergeAsync();

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, retainedPerson.PersonId);

        // Assert
        AssertTitle(entry, $"Record merged with TRN {deactivatedPerson.Trn}");
        Assert.NotNull(entry.GetElementByTestId("details"));
        Assert.NotNull(entry.GetElementByTestId("previous-details"));
    }

    [Fact]
    public async Task ProcessRendersCorrectlyForDeactivatedPerson()
    {
        // Arrange
        var (retainedPerson, deactivatedPerson, process) = await CreateMergeAsync();

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, deactivatedPerson.PersonId);

        // Assert
        AssertTitle(entry, $"Record merged into TRN {retainedPerson.Trn} and deactivated");
        Assert.Null(entry.GetElementByTestId("details"));
        Assert.Null(entry.GetElementByTestId("previous-details"));
    }

    [Fact]
    public async Task ProcessRendersInOneLoginContext()
    {
        // A merge that re-points a One Login user puts the process on that user's change history, where there's
        // no person being looked at.
        // Arrange
        var (_, deactivatedPerson, process) = await CreateMergeAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(deactivatedPerson);

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            contextType: "oneLogin",
            oneLoginSubject: oneLoginUser.Subject);

        // Assert
        AssertTitle(entry, $"Record merged with TRN {deactivatedPerson.Trn}");
    }

    private async Task<(Person RetainedPerson, Person DeactivatedPerson, Process Process)> CreateMergeAsync()
    {
        var user = await TestData.CreateUserAsync();
        var retainedPerson = await TestData.CreatePersonAsync();
        var deactivatedPerson = await TestData.CreatePersonAsync();
        var newLastName = TestData.GenerateChangedLastName(retainedPerson.LastName);

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonMerging,
            user.UserId,
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = null,
                Details = "Some comments",
                EvidenceFile = null,
                AdditionalInformation = null
            },
            new PersonDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = deactivatedPerson.PersonId,
                MergedWithPersonId = retainedPerson.PersonId,
                Changes = PersonDeactivatedEventChanges.MergedWithPersonId,
                DateOfDeath = null
            },
            new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = retainedPerson.PersonId,
                PersonDetails = CreatePersonDetails(retainedPerson.FirstName, retainedPerson.MiddleName, newLastName, retainedPerson.DateOfBirth),
                OldPersonDetails = CreatePersonDetails(retainedPerson.FirstName, retainedPerson.MiddleName, retainedPerson.LastName, retainedPerson.DateOfBirth),
                Changes = PersonDetailsUpdatedEventChanges.LastName
            });

        return (retainedPerson, deactivatedPerson, process);
    }

    private static EventModels.PersonDetails CreatePersonDetails(
        string firstName,
        string? middleName,
        string lastName,
        DateOnly? dateOfBirth) => new()
        {
            FirstName = firstName,
            MiddleName = middleName ?? string.Empty,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null
        };
}

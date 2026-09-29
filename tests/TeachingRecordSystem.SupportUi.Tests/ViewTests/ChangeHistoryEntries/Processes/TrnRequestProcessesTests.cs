namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class TrnRequestProcessesTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    // A TRN request that creates a person is how most records come into TRS, so the creation has to render on
    // the person's change history from each of the processes that can do it.
    [Theory]
    [InlineData(ProcessType.TrnRequestCreating, "TRN request received", "Record created from a TRN request.")]
    [InlineData(ProcessType.TrnRequestResolving, "TRN request resolved", "Record created while resolving a TRN request.")]
    [InlineData(ProcessType.TrnRequestActivating, "TRN request activated", "Record created when a dormant TRN request was activated.")]
    public async Task ProcessThatCreatedAPerson_RendersTheRecordDetails(
        ProcessType processType,
        string expectedTitle,
        string expectedSummary)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress().WithGender());
        var user = await TestData.CreateUserAsync();

        var process = await TestData.CreateProcessAsync(
            processType,
            user.UserId,
            changeReason: null,
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = EventModels.PersonDetails.FromModel(person),
                TrnRequestMetadata = null
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, expectedTitle);
        Assert.Equal(expectedSummary, entry.GetElementByTestId("trn-request-summary")?.TrimmedText());

        entry.AssertSummaryListRowValue("details", "Name", v =>
            Assert.Equal($"{person.FirstName} {person.MiddleName} {person.LastName}", v.TrimmedText()));
        entry.AssertSummaryListRowValue("details", "Date of birth", v =>
            Assert.Equal(person.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat), v.TrimmedText()));
    }

    [Fact]
    public async Task ProcessThatMatchedAnExistingPerson_RendersWithoutRecordDetails()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var supportTaskResult = await TestData.CreateTrnRequestSupportTaskAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.TrnRequestCreating,
            user.UserId,
            changeReason: null,
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTaskResult.SupportTask)
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "TRN request received");
        Assert.Equal("A TRN request was matched to this record.", entry.GetElementByTestId("trn-request-summary")?.TrimmedText());
        Assert.Null(entry.GetElementByTestId("details"));
    }
}

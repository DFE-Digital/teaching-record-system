using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogPersonCreatingProcessTests : TestBase
{
    public ChangeLogPersonCreatingProcessTests(HostFixture hostFixture) : base(hostFixture)
    {
        // Toggle between GMT and BST to ensure we're testing rendering dates in local time
        var nows = new[]
        {
            new DateTime(2024, 1, 1, 12, 13, 14, DateTimeKind.Utc),  // GMT
            new DateTime(2024, 7, 5, 19, 20, 21, DateTimeKind.Utc)   // BST
        };
        TimeProvider.SetUtcNow(new DateTimeOffset(nows.SingleRandom(), TimeSpan.Zero));
    }

    [Fact]
    public async Task Person_WithPersonCreatingProcess_RendersExpectedContent()
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress().WithGender());

        var evidenceFile = new EventModels.File { FileId = Guid.NewGuid(), Name = "other-evidence.jpg" };

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = PersonCreateReason.AnotherReason.GetDisplayName(),
            Details = "Reason detail",
            EvidenceFile = evidenceFile,
            AdditionalInformation = "some additional information"
        };

        var process = await CreatePersonCreatingProcessAsync(person, createdByUser.UserId, changeReason);

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(process.ProcessId, "Record created manually", createdByUser.Name, process.CreatedOn);

        var name = doc.GetElementByTestId("name");
        Assert.NotNull(name);
        Assert.Equal($"Record created for {person.FirstName} {person.MiddleName} {person.LastName}", name!.TrimmedText());

        doc.AssertSummaryListRowValue("create-reason", "Reason", v =>
            Assert.Equal(changeReason.Details, v.TrimmedText()));
        doc.AssertSummaryListRowValue("create-reason", "Additional information", v =>
            Assert.Equal(changeReason.AdditionalInformation, v.TrimmedText()));
        doc.AssertSummaryListRowValue("create-reason", "Evidence", v =>
            Assert.Equal($"{evidenceFile.Name} (opens in new tab)", v.TrimmedText()));
    }

    [Fact]
    public async Task Person_WithPersonCreatingProcessAndNoOptionalDetails_DoesNotRenderThem()
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var process = await CreatePersonCreatingProcessAsync(person, createdByUser.UserId, changeReason: null);

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(process.ProcessId, "Record created manually", createdByUser.Name, process.CreatedOn);

        Assert.Null(doc.GetElementByTestId("create-reason"));
    }

    [Fact]
    public async Task Person_WithTeacherPensionsRecordImportingProcess_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherPensionsRecordImporting,
            SystemUser.Instance.UserId,
            changeReason: null,
            CreatePersonCreatedEvent(person));

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Record imported from Teachers’ Pensions",
            SystemUser.Instance.Name,
            process.CreatedOn);
    }

    private Task<Process> CreatePersonCreatingProcessAsync(
        Person person,
        Guid userId,
        ChangeReasonWithDetailsAndEvidence? changeReason) =>
        TestData.CreateProcessAsync(
            ProcessType.PersonCreating,
            userId,
            changeReason,
            CreatePersonCreatedEvent(person));

    private static PersonCreatedEvent CreatePersonCreatedEvent(Person person) => new()
    {
        EventId = Guid.NewGuid(),
        PersonId = person.PersonId,
        Details = EventModels.PersonDetails.FromModel(person),
        TrnRequestMetadata = null
    };
}

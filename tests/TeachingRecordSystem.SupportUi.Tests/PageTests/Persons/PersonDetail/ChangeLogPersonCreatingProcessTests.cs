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

        doc.AssertHasChangeHistoryEntry(process.ProcessId, "Record created", createdByUser.Name, process.CreatedOn);

        doc.AssertSummaryListRowValue("details", "Name", v =>
            Assert.Equal($"{person.FirstName} {person.MiddleName} {person.LastName}", v.TrimmedText()));
        doc.AssertSummaryListRowValue("details", "Date of birth", v =>
            Assert.Equal(person.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat), v.TrimmedText()));

        doc.AssertSummaryListRowValue("create-reason", "Reason", v =>
            Assert.Equal(changeReason.Reason, v.TrimmedText()));
        doc.AssertSummaryListRowValue("create-reason", "Reason details", v =>
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

        doc.AssertHasChangeHistoryEntry(process.ProcessId, "Record created", createdByUser.Name, process.CreatedOn);

        doc.AssertSummaryListRowDoesNotExist("details", "Email address");
        doc.AssertSummaryListRowDoesNotExist("details", "National Insurance number");
        doc.AssertSummaryListRowDoesNotExist("details", "Gender");

        doc.AssertSummaryListRowValue("create-reason", "Reason details", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertSummaryListRowValue("create-reason", "Evidence", v => Assert.Equal("Not provided", v.TrimmedText()));
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

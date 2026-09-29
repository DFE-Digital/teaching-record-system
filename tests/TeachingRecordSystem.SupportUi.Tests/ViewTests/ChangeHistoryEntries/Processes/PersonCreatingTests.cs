using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class PersonCreatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress().WithGender());
        var user = await TestData.CreateUserAsync();

        var evidenceFile = new EventModels.File { FileId = Guid.NewGuid(), Name = "evidence.jpg" };

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = PersonCreateReason.AnotherReason.GetDisplayName(),
            Details = "Reason detail",
            EvidenceFile = evidenceFile,
            AdditionalInformation = "some additional information"
        };

        // Act
        var entry = await PublishPersonCreatedEventAsync(
            person.PersonId,
            user.UserId,
            changeReason,
            CreatePersonDetails(
                firstName: person.FirstName,
                middleName: person.MiddleName,
                lastName: person.LastName,
                dateOfBirth: person.DateOfBirth,
                emailAddress: person.EmailAddress,
                nationalInsuranceNumber: person.NationalInsuranceNumber,
                gender: person.Gender));

        // Assert
        AssertTitle(entry, "Record created manually");

        var name = entry.GetElementByTestId("name");
        Assert.NotNull(name);
        Assert.Equal($"Record created for {person.FirstName} {person.MiddleName} {person.LastName}", name!.TrimmedText());

        entry.AssertSummaryListRowValue("create-reason", "Reason", v =>
            Assert.Equal(changeReason.Details, v.TrimmedText()));
        entry.AssertSummaryListRowValue("create-reason", "Additional information", v =>
            Assert.Equal(changeReason.AdditionalInformation, v.TrimmedText()));
        entry.AssertSummaryListRowValue("create-reason", "Evidence", v =>
            Assert.Equal($"{evidenceFile.Name} (opens in new tab)", v.TrimmedText()));

        Assert.Null(entry.GetElementByTestId("email-sent-message"));
    }

    [Fact]
    public async Task ProcessWithoutChangeReason_RendersWithoutReasonSection()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        // Act
        var entry = await PublishPersonCreatedEventAsync(
            person.PersonId,
            user.UserId,
            changeReason: null,
            CreatePersonDetails(
                firstName: person.FirstName,
                middleName: person.MiddleName,
                lastName: person.LastName,
                dateOfBirth: person.DateOfBirth,
                emailAddress: null,
                nationalInsuranceNumber: null,
                gender: null));

        // Assert
        AssertTitle(entry, "Record created manually");

        Assert.Null(entry.GetElementByTestId("create-reason"));
    }

    [Fact]
    public async Task ProcessCreatedBySystemUser_RendersRecordCreatedTitle()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        // Act
        var entry = await PublishPersonCreatedEventAsync(
            person.PersonId,
            SystemUser.SystemUserId,
            changeReason: null,
            CreatePersonDetails(
                firstName: person.FirstName,
                middleName: person.MiddleName,
                lastName: person.LastName,
                dateOfBirth: person.DateOfBirth,
                emailAddress: null,
                nationalInsuranceNumber: null,
                gender: null));

        // Assert
        AssertTitle(entry, "Record created");
    }

    [Fact]
    public async Task ProcessWithEmailSentEvent_RendersEmailSentMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var details = CreatePersonDetails(
            firstName: person.FirstName,
            middleName: person.MiddleName,
            lastName: person.LastName,
            dateOfBirth: person.DateOfBirth,
            emailAddress: null,
            nationalInsuranceNumber: null,
            gender: null);

        var email = new EventModels.Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = "template-123",
            EmailAddress = Faker.Internet.Email(),
            Personalization = new Dictionary<string, string>(),
            Metadata = new Dictionary<string, object>(),
            SentOn = TimeProvider.UtcNow,
            EmailReplyToId = null
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonCreating,
            SystemUser.SystemUserId,
            changeReason: null,
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = details,
                TrnRequestMetadata = null
            },
            new EmailSentEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Email = email
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Record created");

        var emailSentMessage = entry.GetElementByTestId("email-sent-message");
        Assert.NotNull(emailSentMessage);
        Assert.Equal("We\u2019ve sent them an email confirming their TRN.", emailSentMessage!.TrimmedText());
    }

    private async Task<IHtmlElement> PublishPersonCreatedEventAsync(
        Guid personId,
        Guid userId,
        ChangeReasonWithDetailsAndEvidence? changeReason,
        EventModels.PersonDetails details)
    {
        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonCreating,
            userId,
            changeReason,
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                Details = details,
                TrnRequestMetadata = null
            });

        return await GetEntryHtmlAsync(process.ProcessId);
    }

    private static EventModels.PersonDetails CreatePersonDetails(
        string firstName = "Jane",
        string middleName = "Alice",
        string lastName = "Smith",
        DateOnly? dateOfBirth = null,
        string? emailAddress = "jane.smith@example.com",
        string? nationalInsuranceNumber = "QQ123456C",
        Gender? gender = Gender.Female) =>
        new()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = gender
        };
}

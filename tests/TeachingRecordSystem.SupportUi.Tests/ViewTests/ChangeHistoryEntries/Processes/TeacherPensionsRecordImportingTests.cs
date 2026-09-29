using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class TeacherPensionsRecordImportingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ImportThatCreatedAPerson_RendersTheRecordDetails()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress().WithGender());

        // Act
        var entry = await PublishImportAsync(person, raiseDuplicateTask: false);

        // Assert
        AssertTitle(entry, "Record imported from Teachers’ Pensions");

        Assert.Equal(
            "Record created from the Teachers’ Pensions import.",
            entry.GetElementByTestId("import-summary")?.TrimmedText());

        entry.AssertSummaryListRowValue("details", "Name", v =>
            Assert.Equal($"{person.FirstName} {person.MiddleName} {person.LastName}", v.TrimmedText()));
        entry.AssertSummaryListRowValue("details", "Date of birth", v =>
            Assert.Equal(person.DateOfBirth!.Value.ToString(WebConstants.DateDisplayFormat), v.TrimmedText()));
    }

    [Fact]
    public async Task ImportThatAlsoRaisedADuplicateTask_SaysSo()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        // Act
        var entry = await PublishImportAsync(person, raiseDuplicateTask: true);

        // Assert
        Assert.Equal(
            "Record created from the Teachers’ Pensions import and flagged as a potential duplicate.",
            entry.GetElementByTestId("import-summary")?.TrimmedText());
    }

    [Fact]
    public async Task ImportThatOnlyRaisedADuplicateTask_RendersWithoutRecordDetails()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync();

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherPensionsRecordImporting,
            SystemUser.Instance.UserId,
            changeReason: null,
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Record imported from Teachers’ Pensions");

        Assert.Equal(
            "Teachers’ Pensions record flagged as a potential duplicate.",
            entry.GetElementByTestId("import-summary")?.TrimmedText());

        Assert.Null(entry.GetElementByTestId("details"));
    }

    private async Task<IHtmlElement> PublishImportAsync(Person person, bool raiseDuplicateTask)
    {
        var events = new List<IEvent>
        {
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = EventModels.PersonDetails.FromModel(person),
                TrnRequestMetadata = null
            }
        };

        if (raiseDuplicateTask)
        {
            var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync();

            events.Add(new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            });
        }

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherPensionsRecordImporting,
            SystemUser.Instance.UserId,
            changeReason: null,
            [.. events]);

        return await GetEntryHtmlAsync(process.ProcessId);
    }
}

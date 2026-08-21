using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogDqtInductionProcessTests : TestBase
{
    public ChangeLogDqtInductionProcessTests(HostFixture hostFixture) : base(hostFixture)
    {
        // Toggle between GMT and BST to ensure we're testing rendering dates in local time
        var nows = new[]
        {
            new DateTime(2024, 1, 1, 12, 13, 14, DateTimeKind.Utc),  // GMT
            new DateTime(2024, 7, 5, 19, 20, 21, DateTimeKind.Utc)   // BST
        };
        TimeProvider.SetUtcNow(new DateTimeOffset(nows.SingleRandom(), TimeSpan.Zero));
    }

    [Theory]
    [InlineData(DqtInductionFields.None)]
    [InlineData(DqtInductionFields.StartDate)]
    [InlineData(DqtInductionFields.CompletionDate)]
    [InlineData(DqtInductionFields.Status)]
    [InlineData(DqtInductionFields.ExemptionReason)]
    [InlineData(DqtInductionFields.StartDate | DqtInductionFields.Status)]
    [InlineData(DqtInductionFields.StartDate | DqtInductionFields.CompletionDate | DqtInductionFields.Status)]
    [InlineData(DqtInductionFields.StartDate | DqtInductionFields.CompletionDate | DqtInductionFields.Status | DqtInductionFields.ExemptionReason)]
    public async Task Person_WithInductionCreatingInDqtProcess_RendersExpectedContent(DqtInductionFields populatedFields)
    {
        // Arrange
        var raisedBy = CreateDqtUser();
        var person = await TestData.CreatePersonAsync();

        DateOnly? startDate = TimeProvider.Today.AddYears(-1);
        DateOnly? completionDate = TimeProvider.Today.AddDays(-10);
        var inductionStatus = populatedFields.HasFlag(DqtInductionFields.ExemptionReason) ? InductionStatus.Exempt : InductionStatus.InProgress;
        var inductionExemptionReason = await ReferenceDataCache.GetInductionExemptionReasonByIdAsync(
            InductionExemptionReason.QualifiedThroughEeaMutualRecognitionRouteId);

        var induction = new EventModels.DqtInduction
        {
            InductionId = Guid.NewGuid(),
            StartDate = populatedFields.HasFlag(DqtInductionFields.StartDate) ? Option.Some(startDate) : Option.None<DateOnly?>(),
            CompletionDate = populatedFields.HasFlag(DqtInductionFields.CompletionDate) ? Option.Some(completionDate) : Option.None<DateOnly?>(),
            InductionStatus = populatedFields.HasFlag(DqtInductionFields.Status) ? Option.Some<string?>(inductionStatus.ToString()) : Option.None<string?>(),
            InductionExemptionReason = populatedFields.HasFlag(DqtInductionFields.ExemptionReason) ? Option.Some(inductionExemptionReason.ToString()) : Option.None<string?>()
        };

        var process = await CreateDqtProcessAsync(
            ProcessType.InductionCreatingInDqt,
            raisedBy,
            new DqtInductionCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Induction = induction
            });

        // Only the fields the event carries a value for are rendered, in this order.
        var expectedRows = new List<(string Key, string? Value)>();
        if (populatedFields.HasFlag(DqtInductionFields.Status))
        {
            expectedRows.Add(("Induction status", inductionStatus.ToString()));
        }
        if (populatedFields.HasFlag(DqtInductionFields.ExemptionReason))
        {
            expectedRows.Add(("Exemption reason", inductionExemptionReason.ToString()));
        }
        if (populatedFields.HasFlag(DqtInductionFields.StartDate))
        {
            expectedRows.Add(("Started on", startDate?.ToString(WebConstants.DateDisplayFormat)));
        }
        if (populatedFields.HasFlag(DqtInductionFields.CompletionDate))
        {
            expectedRows.Add(("Completed on", completionDate?.ToString(WebConstants.DateDisplayFormat)));
        }

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Induction created",
            raisedBy.DqtUserName!,
            process.CreatedOn,
            expectedRows,
            expectedPreviousDataSummaryListRows: []);
    }

    [Fact]
    public async Task Person_WithInductionImportingIntoDqtProcess_RendersExpectedContent()
    {
        // Arrange
        var raisedBy = CreateDqtUser();
        var person = await TestData.CreatePersonAsync();

        var process = await CreateDqtProcessAsync(
            ProcessType.InductionImportingIntoDqt,
            raisedBy,
            new DqtInductionImportedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Induction = CreateEmptyInduction(),
                DqtState = 0
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(process.ProcessId, "Induction imported", raisedBy.DqtUserName!, process.CreatedOn);
    }

    [Fact]
    public async Task Person_WithInductionDeactivatingInDqtProcess_RendersExpectedContent()
    {
        // Arrange
        var raisedBy = CreateDqtUser();
        var person = await TestData.CreatePersonAsync();

        var process = await CreateDqtProcessAsync(
            ProcessType.InductionDeactivatingInDqt,
            raisedBy,
            new DqtInductionDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Induction = CreateEmptyInduction()
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(process.ProcessId, "Induction deactivated", raisedBy.DqtUserName!, process.CreatedOn);
    }

    [Fact]
    public async Task Person_WithInductionReactivatingInDqtProcess_RendersExpectedContent()
    {
        // Arrange
        var raisedBy = CreateDqtUser();
        var person = await TestData.CreatePersonAsync();

        var process = await CreateDqtProcessAsync(
            ProcessType.InductionReactivatingInDqt,
            raisedBy,
            new DqtInductionReactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Induction = CreateEmptyInduction()
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(process.ProcessId, "Induction reactivated", raisedBy.DqtUserName!, process.CreatedOn);
    }

    [Theory]
    [InlineData(DqtInductionUpdatedEventChanges.StartDate, false, false)]
    [InlineData(DqtInductionUpdatedEventChanges.StartDate, true, false)]
    [InlineData(DqtInductionUpdatedEventChanges.StartDate, false, true)]
    [InlineData(DqtInductionUpdatedEventChanges.CompletionDate, false, false)]
    [InlineData(DqtInductionUpdatedEventChanges.CompletionDate, true, false)]
    [InlineData(DqtInductionUpdatedEventChanges.CompletionDate, false, true)]
    [InlineData(DqtInductionUpdatedEventChanges.Status, false, false)]
    [InlineData(DqtInductionUpdatedEventChanges.Status, true, false)]
    [InlineData(DqtInductionUpdatedEventChanges.Status, false, true)]
    [InlineData(DqtInductionUpdatedEventChanges.ExemptionReason, false, false)]
    [InlineData(DqtInductionUpdatedEventChanges.ExemptionReason, true, false)]
    [InlineData(DqtInductionUpdatedEventChanges.ExemptionReason, false, true)]
    [InlineData(DqtInductionUpdatedEventChanges.StartDate | DqtInductionUpdatedEventChanges.Status, false, false)]
    [InlineData(DqtInductionUpdatedEventChanges.StartDate | DqtInductionUpdatedEventChanges.CompletionDate | DqtInductionUpdatedEventChanges.Status, false, false)]
    [InlineData(DqtInductionUpdatedEventChanges.StartDate | DqtInductionUpdatedEventChanges.CompletionDate | DqtInductionUpdatedEventChanges.Status | DqtInductionUpdatedEventChanges.ExemptionReason, false, false)]
    [InlineData(DqtInductionUpdatedEventChanges.StartDate | DqtInductionUpdatedEventChanges.Status, true, false)]
    [InlineData(DqtInductionUpdatedEventChanges.StartDate | DqtInductionUpdatedEventChanges.CompletionDate | DqtInductionUpdatedEventChanges.Status, true, false)]
    [InlineData(DqtInductionUpdatedEventChanges.StartDate | DqtInductionUpdatedEventChanges.CompletionDate | DqtInductionUpdatedEventChanges.Status | DqtInductionUpdatedEventChanges.ExemptionReason, true, false)]
    [InlineData(DqtInductionUpdatedEventChanges.StartDate | DqtInductionUpdatedEventChanges.Status, false, true)]
    [InlineData(DqtInductionUpdatedEventChanges.StartDate | DqtInductionUpdatedEventChanges.CompletionDate | DqtInductionUpdatedEventChanges.Status, false, true)]
    [InlineData(DqtInductionUpdatedEventChanges.StartDate | DqtInductionUpdatedEventChanges.CompletionDate | DqtInductionUpdatedEventChanges.Status | DqtInductionUpdatedEventChanges.ExemptionReason, false, true)]
    public async Task Person_WithInductionUpdatingInDqtProcess_RendersExpectedContent(DqtInductionUpdatedEventChanges changes, bool previousValueIsNull, bool newValueIsNull)
    {
        // Arrange
        var raisedBy = CreateDqtUser();
        var person = await TestData.CreatePersonAsync();

        var inductionId = Guid.NewGuid();
        DateOnly? oldStartDate = TimeProvider.Today.AddYears(-1);
        DateOnly? oldCompletionDate = TimeProvider.Today.AddDays(-10);
        var oldInductionStatus = changes.HasFlag(DqtInductionUpdatedEventChanges.ExemptionReason) ? InductionStatus.Exempt : InductionStatus.InProgress;
        var oldInductionExemptionReason = InductionExemptionReason.QualifiedThroughEeaMutualRecognitionRouteId;

        DateOnly? startDate = TimeProvider.Today.AddYears(-1).AddDays(1);
        DateOnly? completionDate = TimeProvider.Today.AddDays(-9);
        var inductionStatus = changes.HasFlag(DqtInductionUpdatedEventChanges.ExemptionReason) ? InductionStatus.Exempt : InductionStatus.Passed;
        var inductionExemptionReason = await ReferenceDataCache.GetInductionExemptionReasonByIdAsync(
            InductionExemptionReason.OverseasTrainedTeacherId);

        var induction = new EventModels.DqtInduction
        {
            InductionId = inductionId,
            StartDate = changes.HasFlag(DqtInductionUpdatedEventChanges.StartDate) && !newValueIsNull ? Option.Some(startDate) : Option.None<DateOnly?>(),
            CompletionDate = changes.HasFlag(DqtInductionUpdatedEventChanges.CompletionDate) && !newValueIsNull ? Option.Some(completionDate) : Option.None<DateOnly?>(),
            InductionStatus = changes.HasFlag(DqtInductionUpdatedEventChanges.Status) && !newValueIsNull ? Option.Some<string?>(inductionStatus.ToString()) : Option.None<string?>(),
            InductionExemptionReason = changes.HasFlag(DqtInductionUpdatedEventChanges.ExemptionReason) && !newValueIsNull ? Option.Some(inductionExemptionReason.ToString()) : Option.None<string?>()
        };

        var oldInduction = new EventModels.DqtInduction
        {
            InductionId = inductionId,
            StartDate = changes.HasFlag(DqtInductionUpdatedEventChanges.StartDate) && !previousValueIsNull ? Option.Some(oldStartDate) : Option.None<DateOnly?>(),
            CompletionDate = changes.HasFlag(DqtInductionUpdatedEventChanges.CompletionDate) && !previousValueIsNull ? Option.Some(oldCompletionDate) : Option.None<DateOnly?>(),
            InductionStatus = changes.HasFlag(DqtInductionUpdatedEventChanges.Status) && !previousValueIsNull ? Option.Some<string?>(oldInductionStatus.ToString()) : Option.None<string?>(),
            InductionExemptionReason = changes.HasFlag(DqtInductionUpdatedEventChanges.ExemptionReason) && !previousValueIsNull ? Option.Some<string?>(oldInductionExemptionReason.ToString()) : Option.None<string?>()
        };

        var process = await CreateDqtProcessAsync(
            ProcessType.InductionUpdatingInDqt,
            raisedBy,
            new DqtInductionUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Induction = induction,
                OldInduction = oldInduction,
                Changes = changes
            });

        // A row is rendered for each changed field, whether or not the event carries a value for it.
        var expectedRows = new List<(string Key, string? Value)>();
        var expectedPreviousRows = new List<(string Key, string? Value)>();
        if (changes.HasFlag(DqtInductionUpdatedEventChanges.Status))
        {
            expectedRows.Add(("Induction status", newValueIsNull ? null : inductionStatus.ToString()));
            expectedPreviousRows.Add(("Induction status", previousValueIsNull ? null : oldInductionStatus.ToString()));
        }
        if (changes.HasFlag(DqtInductionUpdatedEventChanges.ExemptionReason))
        {
            expectedRows.Add(("Exemption reason", newValueIsNull ? null : inductionExemptionReason.ToString()));
            expectedPreviousRows.Add(("Exemption reason", previousValueIsNull ? null : oldInductionExemptionReason.ToString()));
        }
        if (changes.HasFlag(DqtInductionUpdatedEventChanges.StartDate))
        {
            expectedRows.Add(("Started on", newValueIsNull ? null : startDate?.ToString(WebConstants.DateDisplayFormat)));
            expectedPreviousRows.Add(("Started on", previousValueIsNull ? null : oldStartDate?.ToString(WebConstants.DateDisplayFormat)));
        }
        if (changes.HasFlag(DqtInductionUpdatedEventChanges.CompletionDate))
        {
            expectedRows.Add(("Completed on", newValueIsNull ? null : completionDate?.ToString(WebConstants.DateDisplayFormat)));
            expectedPreviousRows.Add(("Completed on", previousValueIsNull ? null : oldCompletionDate?.ToString(WebConstants.DateDisplayFormat)));
        }

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "DQT induction updated",
            raisedBy.DqtUserName!,
            process.CreatedOn,
            expectedRows,
            expectedPreviousRows);
    }

    [Fact]
    public async Task Person_WithPersonInductionStatusChangingInDqtProcess_RendersExpectedContent()
    {
        // Arrange
        var raisedBy = CreateDqtUser();
        var person = await TestData.CreatePersonAsync();
        var oldInductionStatus = InductionStatus.RequiredToComplete;
        var inductionStatus = InductionStatus.InProgress;

        var process = await CreateDqtProcessAsync(
            ProcessType.PersonInductionStatusChangingInDqt,
            raisedBy,
            new DqtContactInductionStatusChangedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                InductionStatus = inductionStatus.ToString(),
                OldInductionStatus = oldInductionStatus.ToString()
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Person induction status updated",
            raisedBy.DqtUserName!,
            process.CreatedOn,
            [("Induction status", inductionStatus.ToString())],
            [("Induction status", oldInductionStatus.ToString())]);
    }

    private static EventModels.RaisedByUserInfo CreateDqtUser() =>
        EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");

    private static EventModels.DqtInduction CreateEmptyInduction() => new()
    {
        InductionId = Guid.NewGuid(),
        StartDate = Option.None<DateOnly?>(),
        CompletionDate = Option.None<DateOnly?>(),
        InductionStatus = Option.None<string?>(),
        InductionExemptionReason = Option.None<string?>()
    };

    public enum DqtInductionFields
    {
        None = 0,
        StartDate = 1 << 0,
        CompletionDate = 1 << 2,
        Status = 1 << 3,
        ExemptionReason = 1 << 4
    }

    // Creates the process directly rather than going via TestData so that the DQT user columns can be populated.
    private Task<Process> CreateDqtProcessAsync(ProcessType processType, EventModels.RaisedByUserInfo raisedBy, IEvent @event) =>
        WithDbContextAsync(async dbContext =>
        {
            var process = new Process
            {
                ProcessId = Guid.NewGuid(),
                ProcessType = processType,
                CreatedOn = TimeProvider.UtcNow,
                UpdatedOn = TimeProvider.UtcNow,
                UserId = raisedBy.UserId,
                DqtUserId = raisedBy.DqtUserId,
                DqtUserName = raisedBy.DqtUserName,
                PersonIds = [.. @event.PersonIds],
                OneLoginUserSubjects = [],
                SupportTaskReferences = [],
                ChangeReason = null
            };

            dbContext.Processes.Add(process);

            dbContext.Set<ProcessEvent>().Add(new ProcessEvent
            {
                ProcessEventId = @event.EventId,
                ProcessId = process.ProcessId,
                EventName = @event.GetType().Name,
                Payload = @event,
                PersonIds = @event.PersonIds,
                OneLoginUserSubjects = @event.OneLoginUserSubjects,
                SupportTaskReferences = @event.SupportTaskReferences,
                CreatedOn = TimeProvider.UtcNow
            });

            await dbContext.SaveChangesAsync();

            return process;
        });
}

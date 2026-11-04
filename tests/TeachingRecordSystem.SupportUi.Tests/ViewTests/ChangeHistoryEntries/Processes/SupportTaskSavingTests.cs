using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class SupportTaskSavingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Theory]
    [InlineData(ProcessType.TrnRequestSupportTaskSaving, SupportTaskType.TrnRequest, "TRN request task saved")]
    [InlineData(ProcessType.TeacherPensionsSupportTaskSaving, SupportTaskType.TeacherPensionsPotentialDuplicate, "Teachers’ Pensions task saved")]
    [InlineData(ProcessType.OneLoginUserRecordMatchingSupportTaskSaving, SupportTaskType.OneLoginUserRecordMatching, "GOV.UK One Login record matching task saved")]
    [InlineData(ProcessType.OneLoginUserIdVerificationSupportTaskSaving, SupportTaskType.OneLoginUserIdVerification, "GOV.UK One Login ID verification task saved")]
    public async Task StatusChanged_RendersTitleAndStatusChange(ProcessType processType, SupportTaskType taskType, string expectedTitle)
    {
        // Arrange
        var process = await CreateProcessAsync(processType, taskType, SupportTaskStatus.Open, SupportTaskStatus.InProgress);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: "TEST-ST-1");

        // Assert
        AssertTitle(entry, expectedTitle);
        var message = entry.QuerySelector("[data-testid='status-changed']");
        Assert.NotNull(message);
        Assert.Equal("Status changed from Not started to In progress", message.TrimmedText());
    }

    [Theory]
    [InlineData(ProcessType.TrnRequestSupportTaskSaving, SupportTaskType.TrnRequest)]
    [InlineData(ProcessType.TeacherPensionsSupportTaskSaving, SupportTaskType.TeacherPensionsPotentialDuplicate)]
    [InlineData(ProcessType.OneLoginUserRecordMatchingSupportTaskSaving, SupportTaskType.OneLoginUserRecordMatching)]
    [InlineData(ProcessType.OneLoginUserIdVerificationSupportTaskSaving, SupportTaskType.OneLoginUserIdVerification)]
    public async Task StatusUnchanged_DoesNotRenderStatusChange(ProcessType processType, SupportTaskType taskType)
    {
        // Arrange
        var process = await CreateProcessAsync(processType, taskType, SupportTaskStatus.InProgress, SupportTaskStatus.InProgress);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: "TEST-ST-1");

        // Assert
        Assert.Null(entry.QuerySelector("[data-testid='status-changed']"));
    }

    [Theory]
    [InlineData(ProcessType.OneLoginUserRecordMatchingSupportTaskSaving, SupportTaskType.OneLoginUserRecordMatching)]
    [InlineData(ProcessType.OneLoginUserIdVerificationSupportTaskSaving, SupportTaskType.OneLoginUserIdVerification)]
    public async Task OneLoginContext_RendersTaskSavedLink(ProcessType processType, SupportTaskType taskType)
    {
        // Arrange
        var process = await CreateProcessAsync(processType, taskType, SupportTaskStatus.Open, SupportTaskStatus.InProgress);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "oneLogin", oneLoginSubject: "subject");

        // Assert
        var message = entry.QuerySelector("[data-testid='task-saved-message']");
        Assert.NotNull(message);
        Assert.Contains("has been saved.", message.TrimmedText());
        var link = message.QuerySelector("a") as IHtmlAnchorElement;
        Assert.NotNull(link);
        Assert.Contains("TEST-ST-1", link.Href);
    }

    [Theory]
    [InlineData(ProcessType.OneLoginUserRecordMatchingSupportTaskSaving, SupportTaskType.OneLoginUserRecordMatching)]
    [InlineData(ProcessType.OneLoginUserIdVerificationSupportTaskSaving, SupportTaskType.OneLoginUserIdVerification)]
    public async Task SupportTaskContext_DoesNotRenderTaskSavedLink(ProcessType processType, SupportTaskType taskType)
    {
        // Arrange
        var process = await CreateProcessAsync(processType, taskType, SupportTaskStatus.Open, SupportTaskStatus.InProgress);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: "TEST-ST-1");

        // Assert
        Assert.Null(entry.QuerySelector("[data-testid='task-saved-message']"));
    }

    private Task<Core.DataStore.Postgres.Models.Process> CreateProcessAsync(
        ProcessType processType,
        SupportTaskType taskType,
        SupportTaskStatus oldStatus,
        SupportTaskStatus newStatus)
    {
        var oldSupportTask = new EventModels.SupportTask
        {
            SupportTaskReference = "TEST-ST-1",
            SupportTaskType = taskType,
            Status = oldStatus,
            OneLoginUserSubject = null,
            PersonId = null,
            Data = new TrnRequestData(),
            SourceApplicationUserId = null,
            ResolveJourneySavedState = null,
            AssignedToUserId = null,
            ZendeskTickets = [],
            Outcome = null
        };

        return TestData.CreateProcessAsync(
            processType,
            changeReason: null,
            events: new SupportTaskUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = "TEST-ST-1",
                Changes = oldStatus != newStatus
                    ? SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.ResolveJourneySavedState
                    : SupportTaskUpdatedEventChanges.ResolveJourneySavedState,
                OldSupportTask = oldSupportTask,
                SupportTask = oldSupportTask with { Status = newStatus },
                Comments = null,
                RejectionReason = null
            });
    }
}

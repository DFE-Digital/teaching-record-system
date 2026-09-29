using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class NpqTrnRequestRejectingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync("Apply for QTS");
        var evidenceFileId = Guid.NewGuid();

        var requestData = CreateRequestData(applicationUser.UserId, evidenceFileId);
        var process = await CreateRejectingProcessAsync(user.UserId, requestData, "Insufficient evidence");

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, supportTaskReference: "TEST-ST-1");

        // Assert
        AssertTitle(entry, "NPQ TRN request rejected");

        entry.AssertSummaryListRowValue("rejection-reason", "Reason", v => Assert.Equal("Insufficient evidence", v.TrimmedText()));

        var evidenceLink = entry.GetElementByTestId("uploaded-evidence-link");
        Assert.NotNull(evidenceLink);
        Assert.Equal($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}", evidenceLink.GetAttribute("href"));

        entry.AssertSummaryListRowValue("request-data", "Source", v => Assert.Equal("Apply for QTS", v.TrimmedText()));
        entry.AssertSummaryListRowValue("request-data", "Request ID", v => Assert.Equal("TEST-TRN-1", v.TrimmedText()));
        entry.AssertSummaryListRowValue("request-data", "Name", v => Assert.Equal("Megan Thee Stallion", v.TrimmedText()));
    }

    [Fact]
    public async Task ProcessWithUnknownApplicationSource_RendersCorrectly()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();

        var requestData = CreateRequestData(applicationUserId: Guid.NewGuid(), evidenceFileId: null);
        var process = await CreateRejectingProcessAsync(user.UserId, requestData, rejectionReason: null);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, supportTaskReference: "TEST-ST-1");

        // Assert
        AssertTitle(entry, "NPQ TRN request rejected");

        entry.AssertSummaryListRowValue("rejection-reason", "Reason", v => Assert.Equal(WebConstants.EmptyFallbackContent, v.TrimmedText()));
        entry.AssertSummaryListRowValue("request-data", "Source", v => Assert.Equal(WebConstants.EmptyFallbackContent, v.TrimmedText()));

        var evidenceLink = entry.GetElementByTestId("uploaded-evidence-link");
        Assert.NotNull(evidenceLink);
        Assert.Equal(WebConstants.EmptyFallbackContent, evidenceLink.TrimmedText());
    }

    private Task<Core.DataStore.Postgres.Models.Process> CreateRejectingProcessAsync(
        Guid userId,
        EventModels.TrnRequestMetadata requestData,
        string? rejectionReason)
    {
        var oldSupportTask = new EventModels.SupportTask
        {
            SupportTaskReference = "TEST-ST-1",
            SupportTaskType = SupportTaskType.NpqTrnRequest,
            Status = SupportTaskStatus.Open,
            OneLoginUserSubject = null,
            PersonId = null,
            Data = new NpqTrnRequestData(),
            SourceApplicationUserId = null,
            ResolveJourneySavedState = null,
            AssignedToUserId = null,
            ZendeskTickets = [],
            Outcome = null
        };

        return TestData.CreateProcessAsync(
            ProcessType.NpqTrnRequestRejecting,
            userId,
            changeReason: null,
            new SupportTaskUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = oldSupportTask.SupportTaskReference,
                Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data,
                SupportTask = oldSupportTask with
                {
                    Status = SupportTaskStatus.Closed,
                    Outcome = SupportTaskOutcome.NpqTrnRequest_Rejected
                },
                OldSupportTask = oldSupportTask,
                Comments = null,
                RejectionReason = rejectionReason
            },
            new TrnRequestUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SourceApplicationUserId = requestData.ApplicationUserId,
                RequestId = requestData.RequestId,
                Changes = TrnRequestUpdatedChanges.Status,
                TrnRequest = requestData,
                OldTrnRequest = requestData with { Status = TrnRequestStatus.Pending },
                ReasonDetails = null
            });
    }

    private EventModels.TrnRequestMetadata CreateRequestData(Guid applicationUserId, Guid? evidenceFileId) => new()
    {
        ApplicationUserId = applicationUserId,
        RequestId = "TEST-TRN-1",
        CreatedOn = TimeProvider.UtcNow,
        IdentityVerified = null,
        EmailAddress = null,
        OneLoginUserSubject = null,
        FirstName = "Megan",
        MiddleName = "Thee",
        LastName = "Stallion",
        PreviousFirstName = null,
        PreviousLastName = null,
        Name = ["Megan", "Thee", "Stallion"],
        DateOfBirth = new DateOnly(1990, 1, 1),
        PotentialDuplicate = null,
        NationalInsuranceNumber = null,
        Gender = null,
        AddressLine1 = null,
        AddressLine2 = null,
        AddressLine3 = null,
        City = null,
        Postcode = null,
        Country = null,
        TrnToken = null,
        ResolvedPersonId = null,
        Matches = null,
        NpqApplicationId = null,
        NpqEvidenceFileId = evidenceFileId,
        NpqEvidenceFileName = evidenceFileId is not null ? "evidence.pdf" : null,
        NpqName = null,
        NpqTrainingProvider = null,
        NpqWorkingInEducationalSetting = null,
        Status = TrnRequestStatus.Rejected
    };
}

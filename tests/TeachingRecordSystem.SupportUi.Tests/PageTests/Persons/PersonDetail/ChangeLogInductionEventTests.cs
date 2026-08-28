using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Services.ChangeHistory;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogInductionEventTests : TestBase
{
    public ChangeLogInductionEventTests(HostFixture hostFixture) : base(hostFixture)
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
    [InlineData(DqtInductionFields.StartDate | DqtInductionFields.CompletionDate)]
    [InlineData(DqtInductionFields.StartDate | DqtInductionFields.CompletionDate | DqtInductionFields.ExemptionReason)]
    public async Task Person_WithInductionMigratedEvent_RendersExpectedContent(DqtInductionFields populatedFields)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePersonAsync();

        DateOnly? startDate = TimeProvider.Today.AddYears(-1);
        DateOnly? completionDate = TimeProvider.Today.AddDays(-10);
        var inductionStatus = populatedFields.HasFlag(DqtInductionFields.ExemptionReason) ? InductionStatus.Exempt : InductionStatus.InProgress;
        string dqtInductionStatus = populatedFields.HasFlag(DqtInductionFields.ExemptionReason) ? "Exempt" : "In progress";
        var inductionExemptionReasonId = InductionExemptionReason.QualifiedThroughEeaMutualRecognitionRouteId;
        var inductionExemptionReason = await ReferenceDataCache.GetInductionExemptionReasonByIdAsync(inductionExemptionReasonId);
        InductionStatus migratedInductionStatus = inductionStatus == InductionStatus.Exempt ? InductionStatus.Exempt : InductionStatus.InProgress;
        var exemptionReason = await ReferenceDataCache.GetInductionExemptionReasonByIdAsync(InductionExemptionReason.PassedInWalesId);
        Guid? migratedInductionExemptionReasonId = exemptionReason.InductionExemptionReasonId;

        var induction = new EventModels.DqtInduction
        {
            InductionId = Guid.NewGuid(),
            StartDate = populatedFields.HasFlag(DqtInductionFields.StartDate) ? Option.Some(startDate) : Option.None<DateOnly?>(),
            CompletionDate = populatedFields.HasFlag(DqtInductionFields.CompletionDate) ? Option.Some(completionDate) : Option.None<DateOnly?>(),
            InductionStatus = populatedFields.HasFlag(DqtInductionFields.Status) ? Option.Some<string?>(inductionStatus.ToString()) : Option.None<string?>(),
            InductionExemptionReason = populatedFields.HasFlag(DqtInductionFields.ExemptionReason) ? Option.Some(inductionExemptionReason.ToString()) : Option.None<string?>()
        };

        var migratedEvent = new LegacyEvents.InductionMigratedEvent
        {
            EventId = Guid.NewGuid(),
            Key = $"{induction.InductionId}-Migrated",
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = createdByDqtUser,
            PersonId = person.PersonId,
            InductionStatus = migratedInductionStatus,
            InductionExemptionReasonId = populatedFields.HasFlag(DqtInductionFields.ExemptionReason) ? migratedInductionExemptionReasonId : null,
            InductionStartDate = populatedFields.HasFlag(DqtInductionFields.StartDate) ? startDate : null,
            InductionCompletedDate = populatedFields.HasFlag(DqtInductionFields.CompletionDate) ? completionDate : null,
            DqtInduction = induction,
            DqtInductionStatus = dqtInductionStatus
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(migratedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-induction-migrated-event"),
            item =>
            {
                Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TrimmedText());
                Assert.Equal(TimeProvider.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
                if (populatedFields.HasFlag(DqtInductionFields.StartDate))
                {
                    Assert.Equal(startDate?.ToString(WebConstants.DateDisplayFormat), item.GetElementByTestId("start-date")?.TrimmedText());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("start-date"));
                }
                if (populatedFields.HasFlag(DqtInductionFields.CompletionDate))
                {
                    Assert.Equal(completionDate?.ToString(WebConstants.DateDisplayFormat), item.GetElementByTestId("completed-date")?.TrimmedText());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("completed-date"));
                }
                Assert.Equal(migratedInductionStatus.GetTitle(), item.GetElementByTestId("induction-status")?.TrimmedText());
                //Assert.Equal(inductionStatus.ToString(), item.GetElementByTestId("dqt-induction-status")?.TrimmedTextContent());
                if (populatedFields.HasFlag(DqtInductionFields.ExemptionReason))
                {
                    Assert.Equal(exemptionReason.Name, item.GetElementByTestId("exemption-reason")?.TrimmedText());
                    Assert.Equal(inductionExemptionReason.ToString(), item.GetElementByTestId("dqt-exemption-reason")?.TrimmedText());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("exemption-reason"));
                }
            });
    }

    [Theory]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStartDate, false, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStartDate, true, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStartDate, false, true)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionCompletedDate, false, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionCompletedDate, true, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionCompletedDate, false, true)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStatus, false, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStatus, true, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStatus, false, true)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionExemptionReasons, false, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionExemptionReasons, true, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionExemptionReasons, false, true)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStartDate | PersonInductionUpdatedEventChanges.InductionStatus, false, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStartDate | PersonInductionUpdatedEventChanges.InductionCompletedDate | PersonInductionUpdatedEventChanges.InductionStatus, false, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStartDate | PersonInductionUpdatedEventChanges.InductionCompletedDate | PersonInductionUpdatedEventChanges.InductionStatus | PersonInductionUpdatedEventChanges.InductionExemptionReasons, false, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStartDate | PersonInductionUpdatedEventChanges.InductionStatus, true, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStartDate | PersonInductionUpdatedEventChanges.InductionCompletedDate | PersonInductionUpdatedEventChanges.InductionStatus, true, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStartDate | PersonInductionUpdatedEventChanges.InductionCompletedDate | PersonInductionUpdatedEventChanges.InductionStatus | PersonInductionUpdatedEventChanges.InductionExemptionReasons, true, false)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStartDate | PersonInductionUpdatedEventChanges.InductionStatus, false, true)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStartDate | PersonInductionUpdatedEventChanges.InductionCompletedDate | PersonInductionUpdatedEventChanges.InductionStatus, false, true)]
    [InlineData(PersonInductionUpdatedEventChanges.InductionStartDate | PersonInductionUpdatedEventChanges.InductionCompletedDate | PersonInductionUpdatedEventChanges.InductionStatus | PersonInductionUpdatedEventChanges.InductionExemptionReasons, false, true)]
    public async Task Person_WithPersonInductionUpdatingProcess_RendersExpectedContent(PersonInductionUpdatedEventChanges changes, bool previousValueIsDefault, bool newValueIsDefault)
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        DateOnly? oldStartDate = TimeProvider.Today.AddYears(-1);
        DateOnly? oldCompletedDate = TimeProvider.Today.AddDays(-10);
        InductionStatus oldInductionStatus = changes.HasFlag(PersonInductionUpdatedEventChanges.InductionExemptionReasons) ? InductionStatus.Exempt : InductionStatus.InProgress;
        Guid[] oldExemptionReasons = [Guid.Parse("5a80cee8-98a8-426b-8422-b0e81cb49b36"), Guid.Parse("15014084-2d8d-4f51-9198-b0e1881f8896")];
        string[] oldExemptionReasonNames = ["They qualified before 07 May 2000", "They qualified between 7 May 1999 and 1 April 2003 and first taught in Wales for at least 2 terms"];

        DateOnly? startDate = TimeProvider.Today.AddYears(-1).AddDays(1);
        DateOnly? completedDate = TimeProvider.Today.AddDays(-9);
        InductionStatus inductionStatus = changes.HasFlag(PersonInductionUpdatedEventChanges.InductionExemptionReasons) ? InductionStatus.Exempt : InductionStatus.RequiredToComplete;
        Guid[] exemptionReasons = [Guid.Parse("0997ab13-7412-4560-8191-e51ed4d58d2a")];
        string[] exemptionReasonNames = ["They qualified through a further education route between 1 September 2001 and 1 September 2004"];

        var changeReason = PersonInductionChangeReason.AnotherReason.GetDisplayName();
        var changeReasonDetail = "Reason detail";
        var additionalInformation = "Additional information";
        var evidenceFile = new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        };

        var induction = new EventModels.Induction
        {
            StartDate = changes.HasFlag(PersonInductionUpdatedEventChanges.InductionStartDate) && !newValueIsDefault ? startDate : null,
            CompletedDate = changes.HasFlag(PersonInductionUpdatedEventChanges.InductionCompletedDate) && !newValueIsDefault ? completedDate : null,
            Status = changes.HasFlag(PersonInductionUpdatedEventChanges.InductionStatus) && !newValueIsDefault ? inductionStatus : InductionStatus.None,
            StatusWithoutExemption = InductionStatus.RequiredToComplete,
            ExemptionReasonIds = changes.HasFlag(PersonInductionUpdatedEventChanges.InductionExemptionReasons) && !newValueIsDefault ? exemptionReasons : [],
            CpdCpdModifiedOn = Option.None<DateTime>(),
            InductionExemptWithoutReason = false
        };

        var oldInduction = new EventModels.Induction
        {
            StartDate = changes.HasFlag(PersonInductionUpdatedEventChanges.InductionStartDate) && !previousValueIsDefault ? oldStartDate : null,
            CompletedDate = changes.HasFlag(PersonInductionUpdatedEventChanges.InductionCompletedDate) && !previousValueIsDefault ? oldCompletedDate : null,
            Status = changes.HasFlag(PersonInductionUpdatedEventChanges.InductionStatus) && !previousValueIsDefault ? oldInductionStatus : InductionStatus.None,
            StatusWithoutExemption = InductionStatus.RequiredToComplete,
            ExemptionReasonIds = changes.HasFlag(PersonInductionUpdatedEventChanges.InductionExemptionReasons) && !previousValueIsDefault ? oldExemptionReasons : [],
            CpdCpdModifiedOn = Option.None<DateTime>(),
            InductionExemptWithoutReason = false
        };

        var processId = await CreateInductionUpdatingProcessAsync(
            person.PersonId,
            EventModels.RaisedByUserInfo.FromUserId(createdByUser.UserId),
            induction,
            oldInduction,
            changes,
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = changeReason,
                Details = changeReasonDetail,
                EvidenceFile = evidenceFile,
                AdditionalInformation = additionalInformation
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByDataAttribute("data-process-id", processId.ToString());
        Assert.NotNull(item);

        var date = item.GetElementsByClassName("moj-timeline__date").SingleOrDefault();
        Assert.Contains($"By {createdByUser.Name} on", date?.TrimmedText().ReplaceLineEndings(" "));

        if (changes.HasFlag(PersonInductionUpdatedEventChanges.InductionStartDate))
        {
            Assert.Equal(newValueIsDefault ? WebConstants.EmptyFallbackContent : startDate?.ToString(WebConstants.DateDisplayFormat), item.GetElementByTestId("start-date")?.TrimmedText());
            Assert.Equal(previousValueIsDefault ? WebConstants.EmptyFallbackContent : oldStartDate?.ToString(WebConstants.DateDisplayFormat), item.GetElementByTestId("old-start-date")?.TrimmedText());
        }
        else
        {
            Assert.Null(item.GetElementByTestId("start-date"));
            Assert.Null(item.GetElementByTestId("old-start-date"));
        }

        if (changes.HasFlag(PersonInductionUpdatedEventChanges.InductionCompletedDate))
        {
            Assert.Equal(newValueIsDefault ? WebConstants.EmptyFallbackContent : completedDate?.ToString(WebConstants.DateDisplayFormat), item.GetElementByTestId("completed-date")?.TrimmedText());
            Assert.Equal(previousValueIsDefault ? WebConstants.EmptyFallbackContent : oldCompletedDate?.ToString(WebConstants.DateDisplayFormat), item.GetElementByTestId("old-completed-date")?.TrimmedText());
        }
        else
        {
            Assert.Null(item.GetElementByTestId("completed-date"));
            Assert.Null(item.GetElementByTestId("old-completed-date"));
        }

        if (changes.HasFlag(PersonInductionUpdatedEventChanges.InductionStatus))
        {
            Assert.Equal(newValueIsDefault ? InductionStatus.None.GetTitle() : inductionStatus.GetTitle(), item.GetElementByTestId("induction-status")?.TrimmedText());
            Assert.Equal(previousValueIsDefault ? InductionStatus.None.GetTitle() : oldInductionStatus.GetTitle(), item.GetElementByTestId("old-induction-status")?.TrimmedText());
        }
        else
        {
            Assert.Null(item.GetElementByTestId("induction-status"));
            Assert.Null(item.GetElementByTestId("old-induction-status"));
        }

        if (changes.HasFlag(PersonInductionUpdatedEventChanges.InductionExemptionReasons))
        {
            if (newValueIsDefault)
            {
                Assert.Equal(WebConstants.EmptyFallbackContent, item.GetElementByTestId("exemption-reason")?.TrimmedText());
            }
            else
            {
                var exemptionReasonItems = item.GetElementByTestId("exemption-reason")?.QuerySelectorAll("li");
                Assert.Single(exemptionReasonItems!);
                Assert.Equal(exemptionReasonNames[0], exemptionReasonItems![0].TrimmedText());
            }

            if (previousValueIsDefault)
            {
                Assert.Equal(WebConstants.EmptyFallbackContent, item.GetElementByTestId("old-exemption-reason")?.TrimmedText());
            }
            else
            {
                var oldExemptionReasonItems = item.GetElementByTestId("old-exemption-reason")?.QuerySelectorAll("li");
                Assert.Equal(2, oldExemptionReasons!.Length);
                var oldExemptionReasonNamesActual = oldExemptionReasonItems!.Select(e => e.TrimmedText()).ToArray();
                Assert.Contains(oldExemptionReasonNames[0], oldExemptionReasonNamesActual);
                Assert.Contains(oldExemptionReasonNames[1], oldExemptionReasonNamesActual);
            }
        }
        else
        {
            Assert.Null(item.GetElementByTestId("exemption-reason"));
            Assert.Null(item.GetElementByTestId("old-exemption-reason"));
        }

        Assert.Null(item.GetElementByTestId("old-cpd-modified-on"));
        Assert.Equal(changeReason, item.GetElementByTestId("reason")?.TrimmedText());
        Assert.Equal(changeReasonDetail, item.GetElementByTestId("reason-detail")?.TrimmedText());
        Assert.Equal(additionalInformation, item.GetElementByTestId("additional-information")?.TrimmedText());
        Assert.Equal($"{evidenceFile.Name} (opens in new tab)", item.GetElementByTestId("uploaded-evidence-link")?.TrimmedText());
    }

    [Fact]
    public async Task Person_WithPersonInductionUpdatingProcess_ChangesNotRelevant_ProcessNotRendered()
    {
        // Arrange
        var changes = PersonInductionUpdatedEventChanges.InductionExemptWithoutReason;
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var induction = new EventModels.Induction
        {
            StartDate = null,
            CompletedDate = null,
            Status = InductionStatus.None,
            StatusWithoutExemption = InductionStatus.Passed,
            ExemptionReasonIds = [],
            CpdCpdModifiedOn = Option.None<DateTime>(),
            InductionExemptWithoutReason = false
        };

        var oldInduction = induction with { InductionExemptWithoutReason = true };

        var processId = await CreateInductionUpdatingProcessAsync(
            person.PersonId,
            EventModels.RaisedByUserInfo.FromUserId(createdByUser.UserId),
            induction,
            oldInduction,
            changes,
            changeReason: null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByDataAttribute("data-process-id", processId.ToString()));
    }

    [Fact]
    public async Task Person_WithPersonCpdInductionUpdatingProcess_RendersTheSameEntry()
    {
        // Arrange
        // CPD has its own process type, but the entry it renders is the same one.
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var startDate = TimeProvider.Today.AddYears(-1);
        var completedDate = TimeProvider.Today.AddDays(-10);

        var induction = new EventModels.Induction
        {
            StartDate = startDate,
            CompletedDate = completedDate,
            Status = InductionStatus.Passed,
            StatusWithoutExemption = InductionStatus.Passed,
            ExemptionReasonIds = [],
            CpdCpdModifiedOn = Option.Some(TimeProvider.UtcNow),
            InductionExemptWithoutReason = false
        };

        var oldInduction = induction with
        {
            Status = InductionStatus.InProgress,
            StatusWithoutExemption = InductionStatus.InProgress,
            CompletedDate = null,
            CpdCpdModifiedOn = Option.None<DateTime>()
        };

        var processId = await CreateInductionUpdatingProcessAsync(
            person.PersonId,
            EventModels.RaisedByUserInfo.FromUserId(createdByUser.UserId),
            induction,
            oldInduction,
            PersonInductionUpdatedEvent.GetChanges(induction, oldInduction),
            changeReason: null,
            ProcessType.PersonCpdInductionUpdating);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByDataAttribute("data-process-id", processId.ToString());
        Assert.NotNull(item);
        Assert.Equal(InductionStatus.Passed.GetTitle(), item.GetElementByTestId("induction-status")?.TrimmedText());
        Assert.Equal(InductionStatus.InProgress.GetTitle(), item.GetElementByTestId("old-induction-status")?.TrimmedText());
        Assert.Equal(completedDate.ToString(WebConstants.DateDisplayFormat), item.GetElementByTestId("completed-date")?.TrimmedText());
    }

    private async Task<Guid> CreateInductionUpdatingProcessAsync(
        Guid personId,
        EventModels.RaisedByUserInfo raisedBy,
        EventModels.Induction induction,
        EventModels.Induction oldInduction,
        PersonInductionUpdatedEventChanges changes,
        IChangeReasonInfo? changeReason,
        ProcessType processType = ProcessType.PersonInductionUpdating)
    {
        var processId = Guid.NewGuid();

        var updatedEvent = new PersonInductionUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = personId,
            Induction = induction,
            OldInduction = oldInduction,
            Changes = changes
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Processes.Add(new Process
            {
                ProcessId = processId,
                ProcessType = processType,
                CreatedOn = TimeProvider.UtcNow,
                UpdatedOn = TimeProvider.UtcNow,
                UserId = raisedBy.UserId,
                DqtUserId = raisedBy.DqtUserId,
                DqtUserName = raisedBy.DqtUserName,
                PersonIds = [personId],
                OneLoginUserSubjects = [],
                SupportTaskReferences = [],
                ChangeReason = changeReason
            });

            dbContext.Set<ProcessEvent>().Add(new ProcessEvent
            {
                ProcessEventId = updatedEvent.EventId,
                ProcessId = processId,
                EventName = nameof(PersonInductionUpdatedEvent),
                Payload = updatedEvent,
                PersonIds = [personId],
                OneLoginUserSubjects = [],
                SupportTaskReferences = [],
                CreatedOn = TimeProvider.UtcNow
            });

            await dbContext.SaveChangesAsync();
        });

        return processId;
    }

    [Flags]
    public enum DqtInductionFields
    {
        None = 0,
        StartDate = 1 << 0,
        CompletionDate = 1 << 2,
        Status = 1 << 3,
        ExemptionReason = 1 << 4
    }
}

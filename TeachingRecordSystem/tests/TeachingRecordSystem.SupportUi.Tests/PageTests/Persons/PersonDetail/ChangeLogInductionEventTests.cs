using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

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
        Clock.UtcNow = nows.RandomOne();
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
    public async Task Person_WithDqtInductionCreatedEvent_RendersExpectedContent(DqtInductionFields populatedFields)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePersonAsync();

        DateOnly? startDate = Clock.Today.AddYears(-1);
        DateOnly? completionDate = Clock.Today.AddDays(-10);
        dfeta_InductionStatus? inductionStatus = populatedFields.HasFlag(DqtInductionFields.ExemptionReason) ? Core.Dqt.Models.dfeta_InductionStatus.Exempt : Core.Dqt.Models.dfeta_InductionStatus.InProgress;
        dfeta_InductionExemptionReason? inductionExemptionReason = Core.Dqt.Models.dfeta_InductionExemptionReason.QualifiedthroughEEAmutualrecognitionroute;

        var induction = new EventModels.DqtInduction
        {
            InductionId = Guid.NewGuid(),
            StartDate = populatedFields.HasFlag(DqtInductionFields.StartDate) ? Option.Some(startDate) : Option.None<DateOnly?>(),
            CompletionDate = populatedFields.HasFlag(DqtInductionFields.CompletionDate) ? Option.Some(completionDate) : Option.None<DateOnly?>(),
            InductionStatus = populatedFields.HasFlag(DqtInductionFields.Status) ? Option.Some(inductionStatus.ToString()) : Option.None<string?>(),
            InductionExemptionReason = populatedFields.HasFlag(DqtInductionFields.ExemptionReason) ? Option.Some(inductionExemptionReason.ToString()) : Option.None<string?>()
        };

        var createdEvent = new DqtInductionCreatedEvent
        {
            EventId = Guid.NewGuid(),
            Key = $"{induction.InductionId}-Created",
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByDqtUser,
            PersonId = person.PersonId,
            Induction = induction
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(createdEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-dqt-induction-created-event"),
            item =>
            {
                Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                if (populatedFields.HasFlag(DqtInductionFields.StartDate))
                {
                    Assert.Equal(startDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("start-date"));
                }
                if (populatedFields.HasFlag(DqtInductionFields.CompletionDate))
                {
                    Assert.Equal(completionDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("completion-date")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("completion-date"));
                }
                if (populatedFields.HasFlag(DqtInductionFields.Status))
                {
                    Assert.Equal(inductionStatus?.ToString(), item.GetElementByTestId("induction-status")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("induction-status"));
                }
                if (populatedFields.HasFlag(DqtInductionFields.ExemptionReason))
                {
                    Assert.Equal(inductionExemptionReason?.ToString(), item.GetElementByTestId("exemption-reason")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("exemption-reason"));
                }
            });
    }

    [Fact]
    public async Task Person_WithDqtInductionImportedEvent_RendersExpectedContent()
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePersonAsync();

        var induction = new EventModels.DqtInduction
        {
            InductionId = Guid.NewGuid(),
            StartDate = Option.None<DateOnly?>(),
            CompletionDate = Option.None<DateOnly?>(),
            InductionStatus = Option.None<string?>(),
            InductionExemptionReason = Option.None<string?>()
        };

        var importedEvent = new DqtInductionImportedEvent
        {
            EventId = Guid.NewGuid(),
            Key = $"{induction.InductionId}-Imported",
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByDqtUser,
            PersonId = person.PersonId,
            Induction = induction,
            DqtState = (int)dfeta_inductionState.Active
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(importedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-dqt-induction-imported-event"),
            item =>
            {
                Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Person_WithDqtInductionDeactivatedEvent_RendersExpectedContent()
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePersonAsync();

        var induction = new EventModels.DqtInduction
        {
            InductionId = Guid.NewGuid(),
            StartDate = Option.None<DateOnly?>(),
            CompletionDate = Option.None<DateOnly?>(),
            InductionStatus = Option.None<string?>(),
            InductionExemptionReason = Option.None<string?>()
        };

        var deactivatedEvent = new DqtInductionDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            Key = $"{induction.InductionId}-Deactivated",
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByDqtUser,
            PersonId = person.PersonId,
            Induction = induction
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(deactivatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-dqt-induction-deactivated-event"),
            item =>
            {
                Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Person_WithDqtInductionReactivatedEvent_RendersExpectedContent()
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePersonAsync();

        var induction = new EventModels.DqtInduction
        {
            InductionId = Guid.NewGuid(),
            StartDate = Option.None<DateOnly?>(),
            CompletionDate = Option.None<DateOnly?>(),
            InductionStatus = Option.None<string?>(),
            InductionExemptionReason = Option.None<string?>()
        };

        var reactivatedEvent = new DqtInductionReactivatedEvent
        {
            EventId = Guid.NewGuid(),
            Key = $"{induction.InductionId}-Reactivated",
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByDqtUser,
            PersonId = person.PersonId,
            Induction = induction
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(reactivatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-dqt-induction-reactivated-event"),
            item =>
            {
                Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
            });
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
    public async Task Person_WithDqtInductionUpdatedEvent_RendersExpectedContent(DqtInductionUpdatedEventChanges changes, bool previousValueIsNull, bool newValueIsNull)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePersonAsync();

        var inductionId = Guid.NewGuid();
        DateOnly? oldStartDate = Clock.Today.AddYears(-1);
        DateOnly? oldCompletionDate = Clock.Today.AddDays(-10);
        dfeta_InductionStatus? oldInductionStatus = changes.HasFlag(DqtInductionUpdatedEventChanges.ExemptionReason) ? Core.Dqt.Models.dfeta_InductionStatus.Exempt : Core.Dqt.Models.dfeta_InductionStatus.InProgress;
        dfeta_InductionExemptionReason? oldInductionExemptionReason = Core.Dqt.Models.dfeta_InductionExemptionReason.QualifiedthroughEEAmutualrecognitionroute;

        DateOnly? startDate = Clock.Today.AddYears(-1).AddDays(1);
        DateOnly? completionDate = Clock.Today.AddDays(-9);
        dfeta_InductionStatus? inductionStatus = changes.HasFlag(DqtInductionUpdatedEventChanges.ExemptionReason) ? Core.Dqt.Models.dfeta_InductionStatus.Exempt : Core.Dqt.Models.dfeta_InductionStatus.PassedinWales;
        dfeta_InductionExemptionReason? inductionExemptionReason = Core.Dqt.Models.dfeta_InductionExemptionReason.OverseasTrainedTeacher;

        var induction = new EventModels.DqtInduction
        {
            InductionId = inductionId,
            StartDate = changes.HasFlag(DqtInductionUpdatedEventChanges.StartDate) && !newValueIsNull ? Option.Some(startDate) : Option.None<DateOnly?>(),
            CompletionDate = changes.HasFlag(DqtInductionUpdatedEventChanges.CompletionDate) && !newValueIsNull ? Option.Some(completionDate) : Option.None<DateOnly?>(),
            InductionStatus = changes.HasFlag(DqtInductionUpdatedEventChanges.Status) && !newValueIsNull ? Option.Some(inductionStatus.ToString()) : Option.None<string?>(),
            InductionExemptionReason = changes.HasFlag(DqtInductionUpdatedEventChanges.ExemptionReason) && !newValueIsNull ? Option.Some(inductionExemptionReason.ToString()) : Option.None<string?>()
        };

        var oldInduction = new EventModels.DqtInduction
        {
            InductionId = inductionId,
            StartDate = changes.HasFlag(DqtInductionUpdatedEventChanges.StartDate) && !previousValueIsNull ? Option.Some(oldStartDate) : Option.None<DateOnly?>(),
            CompletionDate = changes.HasFlag(DqtInductionUpdatedEventChanges.CompletionDate) && !previousValueIsNull ? Option.Some(oldCompletionDate) : Option.None<DateOnly?>(),
            InductionStatus = changes.HasFlag(DqtInductionUpdatedEventChanges.Status) && !previousValueIsNull ? Option.Some(oldInductionStatus.ToString()) : Option.None<string?>(),
            InductionExemptionReason = changes.HasFlag(DqtInductionUpdatedEventChanges.ExemptionReason) && !previousValueIsNull ? Option.Some(oldInductionExemptionReason.ToString()) : Option.None<string?>()
        };

        var updatedEvent = new DqtInductionUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            Key = $"{induction.InductionId}-Updated",
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByDqtUser,
            PersonId = person.PersonId,
            Induction = induction,
            OldInduction = oldInduction,
            Changes = changes
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-dqt-induction-updated-event"),
            item =>
            {
                Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                if (changes.HasFlag(DqtInductionUpdatedEventChanges.StartDate))
                {
                    Assert.Equal(newValueIsNull ? UiDefaults.EmptyDisplayContent : startDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(previousValueIsNull ? UiDefaults.EmptyDisplayContent : oldStartDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("old-start-date")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("start-date"));
                    Assert.Null(item.GetElementByTestId("old-start-date"));
                }
                if (changes.HasFlag(DqtInductionUpdatedEventChanges.CompletionDate))
                {
                    Assert.Equal(newValueIsNull ? UiDefaults.EmptyDisplayContent : completionDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("completion-date")?.TextContent.Trim());
                    Assert.Equal(previousValueIsNull ? UiDefaults.EmptyDisplayContent : oldCompletionDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("old-completion-date")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("completion-date"));
                    Assert.Null(item.GetElementByTestId("old-completion-date"));
                }
                if (changes.HasFlag(DqtInductionUpdatedEventChanges.Status))
                {
                    Assert.Equal(newValueIsNull ? UiDefaults.EmptyDisplayContent : inductionStatus?.ToString(), item.GetElementByTestId("induction-status")?.TextContent.Trim());
                    Assert.Equal(previousValueIsNull ? UiDefaults.EmptyDisplayContent : oldInductionStatus?.ToString(), item.GetElementByTestId("old-induction-status")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("induction-status"));
                    Assert.Null(item.GetElementByTestId("old-induction-status"));
                }
                if (changes.HasFlag(DqtInductionUpdatedEventChanges.ExemptionReason))
                {
                    Assert.Equal(newValueIsNull ? UiDefaults.EmptyDisplayContent : inductionExemptionReason?.ToString(), item.GetElementByTestId("exemption-reason")?.TextContent.Trim());
                    Assert.Equal(previousValueIsNull ? UiDefaults.EmptyDisplayContent : oldInductionExemptionReason?.ToString(), item.GetElementByTestId("old-exemption-reason")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("exemption-reason"));
                    Assert.Null(item.GetElementByTestId("old-exemption-reason"));
                }
            });
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

        DateOnly? startDate = Clock.Today.AddYears(-1);
        DateOnly? completionDate = Clock.Today.AddDays(-10);
        dfeta_InductionStatus? inductionStatus = populatedFields.HasFlag(DqtInductionFields.ExemptionReason) ? Core.Dqt.Models.dfeta_InductionStatus.Exempt : Core.Dqt.Models.dfeta_InductionStatus.InProgress;
        dfeta_InductionExemptionReason? inductionExemptionReason = Core.Dqt.Models.dfeta_InductionExemptionReason.QualifiedthroughEEAmutualrecognitionroute;
        InductionStatus migratedInductionStatus = inductionStatus == dfeta_InductionStatus.Exempt ? InductionStatus.Exempt : InductionStatus.InProgress;
        var exemptionReason = await ReferenceDataCache.GetInductionExemptionReasonByIdAsync(InductionExemptionReason.PassedInWalesId);
        Guid? migratedInductionExemptionReasonId = exemptionReason.InductionExemptionReasonId;

        var induction = new EventModels.DqtInduction
        {
            InductionId = Guid.NewGuid(),
            StartDate = populatedFields.HasFlag(DqtInductionFields.StartDate) ? Option.Some(startDate) : Option.None<DateOnly?>(),
            CompletionDate = populatedFields.HasFlag(DqtInductionFields.CompletionDate) ? Option.Some(completionDate) : Option.None<DateOnly?>(),
            InductionStatus = populatedFields.HasFlag(DqtInductionFields.Status) ? Option.Some(inductionStatus.ToString()) : Option.None<string?>(),
            InductionExemptionReason = populatedFields.HasFlag(DqtInductionFields.ExemptionReason) ? Option.Some(inductionExemptionReason.ToString()) : Option.None<string?>()
        };

        var migratedEvent = new InductionMigratedEvent
        {
            EventId = Guid.NewGuid(),
            Key = $"{induction.InductionId}-Migrated",
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByDqtUser,
            PersonId = person.PersonId,
            InductionStatus = migratedInductionStatus,
            InductionExemptionReasonId = populatedFields.HasFlag(DqtInductionFields.ExemptionReason) ? migratedInductionExemptionReasonId : null,
            InductionStartDate = populatedFields.HasFlag(DqtInductionFields.StartDate) ? startDate : null,
            InductionCompletedDate = populatedFields.HasFlag(DqtInductionFields.CompletionDate) ? completionDate : null,
            DqtInduction = induction
        };

        await WithDbContext(async dbContext =>
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
                Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                if (populatedFields.HasFlag(DqtInductionFields.StartDate))
                {
                    Assert.Equal(startDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("start-date"));
                }
                if (populatedFields.HasFlag(DqtInductionFields.CompletionDate))
                {
                    Assert.Equal(completionDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("completed-date")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("completed-date"));
                }
                Assert.Equal(migratedInductionStatus.GetTitle(), item.GetElementByTestId("induction-status")?.TextContent.Trim());
                //Assert.Equal(inductionStatus.ToString(), item.GetElementByTestId("dqt-induction-status")?.TextContent.Trim());
                if (populatedFields.HasFlag(DqtInductionFields.ExemptionReason))
                {
                    Assert.Equal(exemptionReason.Name, item.GetElementByTestId("exemption-reason")?.TextContent.Trim());
                    Assert.Equal(inductionExemptionReason.ToString(), item.GetElementByTestId("dqt-exemption-reason")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("exemption-reason"));
                }
            });
    }

    [Fact]
    public async Task Person_WithDqtContactInductionStatusChangedEvent_RendersExpectedContent()
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePersonAsync();
        var oldInductionStatus = dfeta_InductionStatus.RequiredtoComplete;
        var inductionStatus = dfeta_InductionStatus.InProgress;

        var statusChangedEvent = new DqtContactInductionStatusChangedEvent
        {
            EventId = Guid.NewGuid(),
            Key = $"{person.ContactId}-StatusChanged",
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByDqtUser,
            PersonId = person.PersonId,
            InductionStatus = inductionStatus.ToString(),
            OldInductionStatus = oldInductionStatus.ToString()
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(statusChangedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-dqt-contact-induction-status-changed-event"),
            item =>
            {
                Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                Assert.Equal(inductionStatus.ToString(), item.GetElementByTestId("induction-status")?.TextContent.Trim());
                Assert.Equal(oldInductionStatus.ToString(), item.GetElementByTestId("old-induction-status")?.TextContent.Trim());
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
    public async Task Person_WithPersonInductionUpdatedEvent_RendersExpectedContent(PersonInductionUpdatedEventChanges changes, bool previousValueIsDefault, bool newValueIsDefault)
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        DateOnly? oldStartDate = Clock.Today.AddYears(-1);
        DateOnly? oldCompletedDate = Clock.Today.AddDays(-10);
        InductionStatus oldInductionStatus = changes.HasFlag(PersonInductionUpdatedEventChanges.InductionExemptionReasons) ? InductionStatus.Exempt : InductionStatus.InProgress;
        Guid[] oldExemptionReasons = [Guid.Parse("5a80cee8-98a8-426b-8422-b0e81cb49b36"), Guid.Parse("15014084-2d8d-4f51-9198-b0e1881f8896")];
        string[] oldExemptionReasonNames = ["Qualified before 07 May 2000", "Qualified between 07 May 1999 and 01 Apr 2003. First post was in Wales and lasted a minimum of two terms."];
        var oldCpdModifiedOn = Clock.UtcNow.AddDays(-2);

        DateOnly? startDate = Clock.Today.AddYears(-1).AddDays(1);
        DateOnly? completedDate = Clock.Today.AddDays(-9);
        InductionStatus inductionStatus = changes.HasFlag(PersonInductionUpdatedEventChanges.InductionExemptionReasons) ? InductionStatus.Exempt : InductionStatus.RequiredToComplete;
        Guid[] exemptionReasons = [Guid.Parse("0997ab13-7412-4560-8191-e51ed4d58d2a")];
        string[] exemptionReasonNames = ["Qualified through Further Education route between 1 Sep 2001 and 1 Sep 2004"];
        var cpdModifiedOn = Clock.UtcNow;

        var changeReason = InductionChangeReasonOption.AnotherReason.GetDisplayName();
        var changeReasonDetail = "Reason detail";
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

        var updatedEvent = new PersonInductionUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            Induction = induction,
            OldInduction = oldInduction,
            Changes = changes,
            ChangeReason = changeReason,
            ChangeReasonDetail = changeReasonDetail,
            EvidenceFile = evidenceFile
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-person-induction-updated-event"),
            item =>
            {
                Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                if (changes.HasFlag(PersonInductionUpdatedEventChanges.InductionStartDate))
                {
                    Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : startDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldStartDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("old-start-date")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("start-date"));
                    Assert.Null(item.GetElementByTestId("old-start-date"));
                }
                if (changes.HasFlag(PersonInductionUpdatedEventChanges.InductionCompletedDate))
                {
                    Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : completedDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("completed-date")?.TextContent.Trim());
                    Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldCompletedDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("old-completed-date")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("completed-date"));
                    Assert.Null(item.GetElementByTestId("old-completed-date"));
                }
                if (changes.HasFlag(PersonInductionUpdatedEventChanges.InductionStatus))
                {
                    Assert.Equal(newValueIsDefault ? InductionStatus.None.GetTitle() : inductionStatus.GetTitle(), item.GetElementByTestId("induction-status")?.TextContent.Trim());
                    Assert.Equal(previousValueIsDefault ? InductionStatus.None.GetTitle() : oldInductionStatus.GetTitle(), item.GetElementByTestId("old-induction-status")?.TextContent.Trim());
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
                        Assert.Equal(UiDefaults.EmptyDisplayContent, item.GetElementByTestId("exemption-reason")?.TextContent.Trim());
                    }
                    else
                    {
                        var exemptionReasons = item.GetElementByTestId("exemption-reason")?.QuerySelectorAll("li");
                        Assert.Single(exemptionReasons!);
                        Assert.Equal(exemptionReasonNames[0], exemptionReasons![0].TextContent.Trim());
                    }

                    if (previousValueIsDefault)
                    {
                        Assert.Equal(UiDefaults.EmptyDisplayContent, item.GetElementByTestId("old-exemption-reason")?.TextContent.Trim());
                    }
                    else
                    {
                        var oldExemptionReasonItems = item.GetElementByTestId("old-exemption-reason")?.QuerySelectorAll("li");
                        Assert.Equal(2, oldExemptionReasons!.Length);
                        var oldExemptionReasonNamesActual = oldExemptionReasonItems!.Select(e => e.TextContent.Trim()).ToArray();
                        Assert.Contains(oldExemptionReasonNames[0], oldExemptionReasonNamesActual);
                        Assert.Contains(oldExemptionReasonNames[1], oldExemptionReasonNamesActual);
                    }
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("exemption-reason"));
                    Assert.Null(item.GetElementByTestId("old-exemption-reason"));
                }
                if (induction.CpdCpdModifiedOn.HasValue)
                {
                    Assert.Equal(cpdModifiedOn.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("cpd-modified-on")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("cpd-modified-on"));
                }
                if (oldInduction.CpdCpdModifiedOn.HasValue)
                {
                    Assert.Equal(oldCpdModifiedOn.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("old-cpd-modified-on")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("old-cpd-modified-on"));
                }
                Assert.Equal(changeReason, item.GetElementByTestId("reason")?.TextContent.Trim());
                Assert.Equal(changeReasonDetail, item.GetElementByTestId("reason-detail")?.TextContent.Trim());
                Assert.Equal($"{evidenceFile.Name} (opens in new tab)", item.GetElementByTestId("uploaded-evidence-link")?.TextContent);
            });
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

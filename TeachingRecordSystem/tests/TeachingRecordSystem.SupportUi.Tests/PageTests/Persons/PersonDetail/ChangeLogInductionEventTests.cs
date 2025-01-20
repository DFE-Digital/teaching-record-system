using Optional;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

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
    [InlineData(InductionFields.None)]
    [InlineData(InductionFields.StartDate)]
    [InlineData(InductionFields.CompletionDate)]
    [InlineData(InductionFields.Status)]
    [InlineData(InductionFields.ExemptionReason)]
    [InlineData(InductionFields.StartDate | InductionFields.Status)]
    [InlineData(InductionFields.StartDate | InductionFields.CompletionDate | InductionFields.Status)]
    [InlineData(InductionFields.StartDate | InductionFields.CompletionDate | InductionFields.Status | InductionFields.ExemptionReason)]
    public async Task Person_WithDqtInductionCreatedEvent_RendersExpectedContent(InductionFields populatedFields)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePersonAsync();

        DateOnly? startDate = Clock.Today.AddYears(-1);
        DateOnly? completionDate = Clock.Today.AddDays(-10);
        dfeta_InductionStatus? inductionStatus = populatedFields.HasFlag(InductionFields.ExemptionReason) ? Core.Dqt.Models.dfeta_InductionStatus.Exempt : Core.Dqt.Models.dfeta_InductionStatus.InProgress;
        dfeta_InductionExemptionReason? inductionExemptionReason = Core.Dqt.Models.dfeta_InductionExemptionReason.QualifiedthroughEEAmutualrecognitionroute;

        var induction = new EventModels.DqtInduction
        {
            InductionId = Guid.NewGuid(),
            StartDate = populatedFields.HasFlag(InductionFields.StartDate) ? Option.Some(startDate) : Option.None<DateOnly?>(),
            CompletionDate = populatedFields.HasFlag(InductionFields.CompletionDate) ? Option.Some(completionDate) : Option.None<DateOnly?>(),
            InductionStatus = populatedFields.HasFlag(InductionFields.Status) ? Option.Some(inductionStatus.ToString()) : Option.None<string?>(),
            InductionExemptionReason = populatedFields.HasFlag(InductionFields.ExemptionReason) ? Option.Some(inductionExemptionReason.ToString()) : Option.None<string?>()
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
                if (populatedFields.HasFlag(InductionFields.StartDate))
                {
                    Assert.Equal(startDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("start-date"));
                }
                if (populatedFields.HasFlag(InductionFields.CompletionDate))
                {
                    Assert.Equal(completionDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("completion-date")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("completion-date"));
                }
                if (populatedFields.HasFlag(InductionFields.Status))
                {
                    Assert.Equal(inductionStatus?.ToString(), item.GetElementByTestId("induction-status")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("induction-status"));
                }
                if (populatedFields.HasFlag(InductionFields.ExemptionReason))
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
    [InlineData(InductionFields.None)]
    [InlineData(InductionFields.StartDate)]
    [InlineData(InductionFields.CompletionDate)]
    [InlineData(InductionFields.Status)]
    [InlineData(InductionFields.ExemptionReason)]
    [InlineData(InductionFields.StartDate | InductionFields.Status)]
    [InlineData(InductionFields.StartDate | InductionFields.CompletionDate | InductionFields.Status)]
    [InlineData(InductionFields.StartDate | InductionFields.CompletionDate | InductionFields.Status | InductionFields.ExemptionReason)]
    public async Task Person_WithInductionMigratedEvent_RendersExpectedContent(InductionFields populatedFields)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePersonAsync();

        DateOnly? startDate = Clock.Today.AddYears(-1);
        DateOnly? completionDate = Clock.Today.AddDays(-10);
        dfeta_InductionStatus? inductionStatus = populatedFields.HasFlag(InductionFields.ExemptionReason) ? Core.Dqt.Models.dfeta_InductionStatus.Exempt : Core.Dqt.Models.dfeta_InductionStatus.InProgress;
        dfeta_InductionExemptionReason? inductionExemptionReason = Core.Dqt.Models.dfeta_InductionExemptionReason.QualifiedthroughEEAmutualrecognitionroute;
        string? migratedInductionStatus = inductionStatus == dfeta_InductionStatus.Exempt ? "Exempt" : "In progress";
        string? migratedInductionExemptionReason = "Qualified through EEA mutual recognition route";

        var induction = new EventModels.DqtInduction
        {
            InductionId = Guid.NewGuid(),
            StartDate = populatedFields.HasFlag(InductionFields.StartDate) ? Option.Some(startDate) : Option.None<DateOnly?>(),
            CompletionDate = populatedFields.HasFlag(InductionFields.CompletionDate) ? Option.Some(completionDate) : Option.None<DateOnly?>(),
            InductionStatus = populatedFields.HasFlag(InductionFields.Status) ? Option.Some(inductionStatus.ToString()) : Option.None<string?>(),
            InductionExemptionReason = populatedFields.HasFlag(InductionFields.ExemptionReason) ? Option.Some(inductionExemptionReason.ToString()) : Option.None<string?>()
        };

        var migratedEvent = new InductionMigratedEvent
        {
            EventId = Guid.NewGuid(),
            Key = $"{induction.InductionId}-Migrated",
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByDqtUser,
            PersonId = person.PersonId,
            InductionStatus = populatedFields.HasFlag(InductionFields.Status) ? migratedInductionStatus : null,
            InductionExemptionReason = populatedFields.HasFlag(InductionFields.ExemptionReason) ? migratedInductionExemptionReason : null,
            InductionStartDate = populatedFields.HasFlag(InductionFields.StartDate) ? startDate : null,
            InductionCompletedDate = populatedFields.HasFlag(InductionFields.CompletionDate) ? completionDate : null,
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
                if (populatedFields.HasFlag(InductionFields.StartDate))
                {
                    Assert.Equal(startDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("start-date"));
                }
                if (populatedFields.HasFlag(InductionFields.CompletionDate))
                {
                    Assert.Equal(completionDate?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("completed-date")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("completed-date"));
                }
                if (populatedFields.HasFlag(InductionFields.Status))
                {
                    Assert.Equal(migratedInductionStatus, item.GetElementByTestId("induction-status")?.TextContent.Trim());
                    Assert.Equal(inductionStatus.ToString(), item.GetElementByTestId("dqt-induction-status")?.TextContent.Trim());
                }
                else
                {
                    Assert.Null(item.GetElementByTestId("induction-status"));
                }
                if (populatedFields.HasFlag(InductionFields.ExemptionReason))
                {
                    Assert.Equal(migratedInductionExemptionReason, item.GetElementByTestId("exemption-reason")?.TextContent.Trim());
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

    [Flags]
    public enum InductionFields
    {
        None = 0,
        StartDate = 1 << 0,
        CompletionDate = 1 << 2,
        Status = 1 << 3,
        ExemptionReason = 1 << 4
    }
}

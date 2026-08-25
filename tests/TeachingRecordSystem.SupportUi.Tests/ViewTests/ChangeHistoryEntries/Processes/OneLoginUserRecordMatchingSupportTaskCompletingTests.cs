using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class OneLoginUserRecordMatchingSupportTaskCompletingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ConnectedOutcome_RendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        // Act
        var entry = await PublishSupportTaskUpdatedEventWithConnectedAsync(supportTask, oneLoginUser);

        // Assert
        AssertTitle(entry, "GOV.UK One Login record matching task completed");

        var outcomeTag = entry.GetElementByTestId("outcome-tag");
        Assert.NotNull(outcomeTag);
        Assert.Contains("Identity verified", outcomeTag!.TextContent);

        var matchedMessage = entry.GetElementByTestId("one-login-matched-message");
        Assert.NotNull(matchedMessage);
        Assert.Contains(oneLoginUser.EmailAddress!, matchedMessage!.TextContent);
    }

    [Fact]
    public async Task NotConnectingOutcome_RendersCorrectly()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        // Act
        var entry = await PublishSupportTaskUpdatedEventAsync(supportTask, SupportTaskOutcome.OneLoginUserRecordMatching_NotConnecting);

        // Assert
        AssertTitle(entry, "GOV.UK One Login record matching task completed");

        var outcomeTag = entry.GetElementByTestId("outcome-tag");
        Assert.NotNull(outcomeTag);
        Assert.Contains("Record not connected", outcomeTag!.TextContent);

        var notConnectedMessage = entry.GetElementByTestId("not-connected-message");
        Assert.NotNull(notConnectedMessage);
        Assert.Contains("could not be connected", notConnectedMessage!.TextContent);
    }

    [Fact]
    public async Task NoMatchesOutcome_RendersCorrectly()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        // Act
        var entry = await PublishSupportTaskUpdatedEventAsync(supportTask, SupportTaskOutcome.OneLoginUserRecordMatching_NoMatches);

        // Assert
        AssertTitle(entry, "GOV.UK One Login record matching task completed");

        var outcomeTag = entry.GetElementByTestId("outcome-tag");
        Assert.NotNull(outcomeTag);
        Assert.Contains("No matches found", outcomeTag!.TextContent);

        var noMatchesMessage = entry.GetElementByTestId("no-matches-message");
        Assert.NotNull(noMatchesMessage);
        Assert.Contains("No matches were found", noMatchesMessage!.TextContent);
    }

    [Fact]
    public async Task WithTrnRequestMatchedToExistingPerson_RendersMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestMetadata = await WithDbContextAsync(async dbContext =>
        {
            var trnRequest = new TrnRequestMetadata
            {
                ApplicationUserId = applicationUser.UserId,
                RequestId = Guid.NewGuid().ToString(),
                CreatedOn = TestData.TimeProvider.UtcNow,
                IdentityVerified = true,
                EmailAddress = TestData.GenerateUniqueEmail(),
                OneLoginUserSubject = oneLoginUser.Subject,
                FirstName = TestData.GenerateFirstName(),
                MiddleName = TestData.GenerateMiddleName(),
                LastName = TestData.GenerateLastName(),
                Name = [TestData.GenerateFirstName(), TestData.GenerateMiddleName(), TestData.GenerateLastName()],
                DateOfBirth = TestData.GenerateDateOfBirth(),
                PotentialDuplicate = false,
                NationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber(),
                Status = TrnRequestStatus.Completed,
                ResolvedPersonId = person.PersonId
            };
            dbContext.TrnRequestMetadata.Add(trnRequest);
            await dbContext.SaveChangesAsync();
            return trnRequest;
        });

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        // Act
        var entry = await PublishSupportTaskWithTrnRequestUpdatedAsync(supportTask, oneLoginUser, trnRequestMetadata, person.PersonId, createPerson: false);

        // Assert
        var trnRequestMessage = entry.GetElementByTestId("trn-request-resolved-message");
        Assert.NotNull(trnRequestMessage);
        Assert.Contains("TRN request matched to an existing record", trnRequestMessage!.TextContent);
    }

    [Fact]
    public async Task WithTrnRequestResolvedByCreatingPerson_RendersMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestMetadata = await WithDbContextAsync(async dbContext =>
        {
            var trnRequest = new TrnRequestMetadata
            {
                ApplicationUserId = applicationUser.UserId,
                RequestId = Guid.NewGuid().ToString(),
                CreatedOn = TestData.TimeProvider.UtcNow,
                IdentityVerified = true,
                EmailAddress = TestData.GenerateUniqueEmail(),
                OneLoginUserSubject = oneLoginUser.Subject,
                FirstName = TestData.GenerateFirstName(),
                MiddleName = TestData.GenerateMiddleName(),
                LastName = TestData.GenerateLastName(),
                Name = [TestData.GenerateFirstName(), TestData.GenerateMiddleName(), TestData.GenerateLastName()],
                DateOfBirth = TestData.GenerateDateOfBirth(),
                PotentialDuplicate = false,
                NationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber(),
                Status = TrnRequestStatus.Completed,
                ResolvedPersonId = person.PersonId
            };
            dbContext.TrnRequestMetadata.Add(trnRequest);
            await dbContext.SaveChangesAsync();
            return trnRequest;
        });

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        // Act
        var entry = await PublishSupportTaskWithTrnRequestUpdatedAsync(supportTask, oneLoginUser, trnRequestMetadata, person.PersonId, createPerson: true);

        // Assert
        var trnRequestMessage = entry.GetElementByTestId("trn-request-resolved-message");
        Assert.NotNull(trnRequestMessage);
        Assert.Contains("request resolved", trnRequestMessage!.TextContent);
    }

    [Fact]
    public async Task WithPersonCreatedEvent_RendersMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        // Act
        var entry = await PublishSupportTaskWithPersonCreatedAsync(supportTask, person);

        // Assert
        var personCreatedMessage = entry.GetElementByTestId("person-created-message");
        Assert.NotNull(personCreatedMessage);
        Assert.Contains("Record created for", personCreatedMessage!.TextContent);
        Assert.Contains(person.FirstName, personCreatedMessage.TextContent);
        Assert.Contains(person.LastName, personCreatedMessage.TextContent);
    }

    [Fact]
    public async Task WithSupportTaskCreatedEvent_TrnRequest_RendersMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));
        var createdTrnRequestResult = await TestData.CreateTrnRequestSupportTaskAsync();

        // Act
        var entry = await PublishSupportTaskWithSupportTaskCreatedAsync(supportTask, createdTrnRequestResult.SupportTask);

        // Assert
        var supportTaskCreatedMessage = entry.GetElementByTestId("support-task-created-message");
        Assert.NotNull(supportTaskCreatedMessage);
        Assert.Contains("TRN request support task", supportTaskCreatedMessage!.TextContent);
        Assert.Contains(createdTrnRequestResult.SupportTask.SupportTaskReference, supportTaskCreatedMessage.TextContent);
    }

    [Fact]
    public async Task WithSupportTaskCreatedEvent_ManualChecksNeeded_RendersMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));
        var createdSupportTask = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync();

        // Act
        var entry = await PublishSupportTaskWithSupportTaskCreatedAsync(supportTask, createdSupportTask);

        // Assert
        var supportTaskCreatedMessage = entry.GetElementByTestId("support-task-created-message");
        Assert.NotNull(supportTaskCreatedMessage);
        Assert.Contains("TRN request manual checks needed support task", supportTaskCreatedMessage!.TextContent);
        Assert.Contains(createdSupportTask.SupportTaskReference, supportTaskCreatedMessage.TextContent);
    }

    [Fact]
    public async Task WithEmailSentEvent_RendersEmailMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        // Act
        var entry = await PublishSupportTaskUpdatedEventWithConnectedAsync(supportTask, oneLoginUser, includeEmailEvent: true);

        // Assert
        var emailMessage = entry.GetElementByTestId("email-sent-message");
        Assert.NotNull(emailMessage);
        Assert.Contains("email", emailMessage!.TextContent);
    }

    [Theory]
    [InlineData("person", true, false)]
    [InlineData("oneLogin", false, true)]
    [InlineData("supportTask", true, true)]
    public async Task Links_RenderBasedOnContext(string contextType, bool expectOneLoginLink, bool expectPersonLink)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        var process = await CreateProcessWithConnectedAsync(supportTask, oneLoginUser);

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            personId: contextType == "person" ? person.PersonId : null,
            contextType: contextType == "person" ? null : contextType,
            oneLoginSubject: contextType == "oneLogin" ? oneLoginUser.Subject : null,
            supportTaskReference: contextType == "supportTask" ? supportTask.SupportTaskReference : null);

        // Assert
        var oneLoginLink = entry.GetElementByTestId("one-login-link");
        if (expectOneLoginLink)
        {
            Assert.NotNull(oneLoginLink);
            Assert.Contains(oneLoginUser.EmailAddress!, oneLoginLink!.TextContent);
        }
        else
        {
            Assert.Null(oneLoginLink);
        }

        var personLink = entry.GetElementByTestId("person-link");
        if (expectPersonLink)
        {
            Assert.NotNull(personLink);
            Assert.Contains(person.FirstName, personLink!.TextContent);
            Assert.Contains(person.LastName, personLink.TextContent);
        }
        else
        {
            Assert.Null(personLink);
        }
    }

    private async Task<IHtmlElement> PublishSupportTaskUpdatedEventAsync(
        SupportTask supportTask,
        SupportTaskOutcome outcome = SupportTaskOutcome.OneLoginUserRecordMatching_Connected,
        params IEvent[] additionalEvents)
    {
        var process = await CreateProcessAsync(supportTask, outcome, additionalEvents);
        return await GetEntryHtmlAsync(process.ProcessId);
    }

    private async Task<IHtmlElement> PublishSupportTaskUpdatedEventWithConnectedAsync(
        SupportTask supportTask,
        OneLoginUser oneLoginUser,
        bool includeEmailEvent = false)
    {
        var events = new List<IEvent>
        {
            new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null },
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            }
        };

        if (includeEmailEvent)
        {
            events.Add(new EmailSentEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = oneLoginUser.PersonId,
                Email = new EventModels.Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = "template-123",
                    EmailAddress = oneLoginUser.EmailAddress!,
                    Personalization = new Dictionary<string, string>(),
                    Metadata = new Dictionary<string, object>(),
                    SentOn = TimeProvider.UtcNow,
                    EmailReplyToId = null
                }
            });
        }

        return await PublishSupportTaskUpdatedEventAsync(supportTask, SupportTaskOutcome.OneLoginUserRecordMatching_Connected, events.ToArray());
    }

    private async Task<IHtmlElement> PublishSupportTaskWithTrnRequestUpdatedAsync(
        SupportTask supportTask,
        OneLoginUser oneLoginUser,
        TrnRequestMetadata trnRequest,
        Guid resolvedPersonId,
        bool createPerson)
    {
        var events = new List<IEvent>
        {
            new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null },
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            },
            new TrnRequestUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SourceApplicationUserId = trnRequest.ApplicationUserId,
                RequestId = trnRequest.RequestId,
                TrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest) with
                {
                    ResolvedPersonId = resolvedPersonId,
                    Status = TrnRequestStatus.Completed
                },
                OldTrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest),
                Changes = TrnRequestUpdatedChanges.ResolvedPersonId | TrnRequestUpdatedChanges.Status,
                ReasonDetails = null
            }
        };

        if (createPerson)
        {
            var createdPerson = await WithDbContextAsync(dbContext =>
                dbContext.Persons.SingleAsync(p => p.PersonId == resolvedPersonId));

            events.Add(new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = resolvedPersonId,
                Details = EventModels.PersonDetails.FromModel(createdPerson),
                TrnRequestMetadata = null
            });
        }

        return await PublishSupportTaskUpdatedEventAsync(supportTask, SupportTaskOutcome.OneLoginUserRecordMatching_Connected, events.ToArray());
    }

    private async Task<IHtmlElement> PublishSupportTaskWithPersonCreatedAsync(
        SupportTask supportTask,
        Person person)
    {
        return await PublishSupportTaskUpdatedEventAsync(
            supportTask,
            SupportTaskOutcome.OneLoginUserRecordMatching_Connected,
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = EventModels.PersonDetails.FromModel(person),
                TrnRequestMetadata = null
            });
    }

    private async Task<IHtmlElement> PublishSupportTaskWithSupportTaskCreatedAsync(
        SupportTask supportTask,
        SupportTask createdSupportTask)
    {
        return await PublishSupportTaskUpdatedEventAsync(
            supportTask,
            SupportTaskOutcome.OneLoginUserRecordMatching_Connected,
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(createdSupportTask)
            });
    }

    private async Task<Process> CreateProcessAsync(
        SupportTask supportTask,
        SupportTaskOutcome outcome = SupportTaskOutcome.OneLoginUserRecordMatching_Connected,
        params IEvent[] additionalEvents)
    {
        var oldSupportTask = EventModels.SupportTask.FromModel(supportTask);
        var supportTaskEventModel = oldSupportTask with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = outcome
        };

        var events = new List<IEvent>
        {
            new SupportTaskUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = supportTask.SupportTaskReference,
                SupportTask = supportTaskEventModel,
                OldSupportTask = oldSupportTask,
                Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Outcome,
                Comments = null,
                RejectionReason = null
            }
        };

        events.AddRange(additionalEvents);

        return await TestData.CreateProcessAsync(
            ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting,
            userId: null,
            changeReason: null,
            events.ToArray());
    }

    private async Task<Process> CreateProcessWithConnectedAsync(
        SupportTask supportTask,
        OneLoginUser oneLoginUser,
        bool includeEmailEvent = false)
    {
        var events = new List<IEvent>
        {
            new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null },
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            }
        };

        if (includeEmailEvent)
        {
            events.Add(new EmailSentEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = oneLoginUser.PersonId,
                Email = new EventModels.Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = "template-123",
                    EmailAddress = oneLoginUser.EmailAddress!,
                    Personalization = new Dictionary<string, string>(),
                    Metadata = new Dictionary<string, object>(),
                    SentOn = TimeProvider.UtcNow,
                    EmailReplyToId = null
                }
            });
        }

        return await CreateProcessAsync(supportTask, SupportTaskOutcome.OneLoginUserRecordMatching_Connected, events.ToArray());
    }
}


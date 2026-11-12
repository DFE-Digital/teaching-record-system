using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class TeacherSigningInTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Theory]
    [InlineData("oneLoginSignedIn", null, "GOV.UK One Login user attempted to login")]
    [InlineData("oneLoginUpdated", null, "GOV.UK One Login updated")]
    [InlineData("trnRequestCreated", null, "TRN request created")]
    [InlineData("supportTaskCreated", SupportTaskType.OneLoginUserIdVerification, "GOV.UK One Login ID verification task created")]
    [InlineData("supportTaskCreated", SupportTaskType.OneLoginUserRecordMatching, "GOV.UK One Login record matching task created")]
    public async Task Title_RendersExpectedValueForEachEventType(string eventType, SupportTaskType? supportTaskType, string expectedTitle)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

        Process process;
        if (eventType == "oneLoginSignedIn")
        {
            process = await TestData.CreateProcessAsync(
                ProcessType.TeacherSigningIn,
                userId: null,
                events: new OneLoginUserSignedInEvent
                {
                    EventId = Guid.NewGuid(),
                    OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser)
                });
        }
        else if (eventType == "oneLoginUpdated")
        {
            process = await TestData.CreateProcessAsync(
                ProcessType.TeacherSigningIn,
                userId: null,
                events: new OneLoginUserUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                    OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null },
                    Changes = OneLoginUserUpdatedEventChanges.PersonId
                });
        }
        else if (eventType == "trnRequestCreated")
        {
            process = await TestData.CreateProcessAsync(
                ProcessType.TeacherSigningIn,
                userId: null,
                events:
                [
                    new TrnRequestCreatedEvent
                    {
                        EventId = Guid.NewGuid(),
                        TrnRequest = EventModels.TrnRequestMetadata.FromModel(new TrnRequestMetadata
                        {
                            ApplicationUserId = Guid.NewGuid(),
                            RequestId = Guid.NewGuid().ToString(),
                            OneLoginUserSubject = oneLoginUser.Subject,
                            EmailAddress = oneLoginUser.EmailAddress,
                            FirstName = person.FirstName,
                            MiddleName = string.Empty,
                            LastName = person.LastName,
                            Name = [person.FirstName, person.LastName],
                            DateOfBirth = person.DateOfBirth!.Value,
                            Status = TrnRequestStatus.Dormant,
                            IdentityVerified = true,
                            CreatedOn = DateTime.UtcNow
                        })
                    }
                ]);
        }
        else
        {
            SupportTask? supportTask = supportTaskType switch
            {
                SupportTaskType.OneLoginUserIdVerification => await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject, configure: b => b.WithStatus(SupportTaskStatus.Open)),
                SupportTaskType.OneLoginUserRecordMatching => await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject, configure: b => b.WithStatus(SupportTaskStatus.Open)),
                _ => null
            };

            process = await TestData.CreateProcessAsync(
                ProcessType.TeacherSigningIn,
                userId: null,
                events: new SupportTaskCreatedEvent
                {
                    EventId = Guid.NewGuid(),
                    SupportTask = EventModels.SupportTask.FromModel(supportTask!)
                });
        }

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, expectedTitle);
    }

    [Fact]
    public async Task SupportTaskCreatedEvent_RendersSupportTaskLinkInNonSupportTaskContexts()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Open));

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherSigningIn,
            userId: null,
            events: new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            });

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            personId: person.PersonId,
            contextType: null,
            oneLoginSubject: null,
            supportTaskReference: null);

        // Assert
        var supportTaskLink = entry.GetElementByTestId("support-task-link");
        Assert.NotNull(supportTaskLink);
        Assert.Equal($"/support-tasks/{supportTask.SupportTaskReference}", supportTaskLink!.GetAttribute("href"));
        Assert.Contains(supportTask.SupportTaskReference, supportTaskLink.TextContent, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("person", true, true)]
    [InlineData("oneLogin", false, true)]
    [InlineData("supportTask", true, false)]
    public async Task SupportTaskCreatedEvent_RendersOneLoginLinkAndTaskLinkBasedOnContext(string contextType, bool expectOneLoginLink, bool expectSupportTaskLink)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person, email: Option.Some<string?>("one-login@example.com"));
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Open));

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherSigningIn,
            userId: null,
            events: new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            });

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            personId: contextType == "person" ? person.PersonId : null,
            contextType: contextType == "person" ? null : contextType,
            oneLoginSubject: contextType == "oneLogin" ? oneLoginUser.Subject : null,
            supportTaskReference: contextType == "supportTask" ? supportTask.SupportTaskReference : null);

        // Assert
        var supportTaskLink = entry.GetElementByTestId("support-task-link");
        if (expectSupportTaskLink)
        {
            Assert.NotNull(supportTaskLink);
            Assert.Equal($"/support-tasks/{supportTask.SupportTaskReference}", supportTaskLink!.GetAttribute("href"));
            Assert.Contains(supportTask.SupportTaskReference, supportTaskLink.TextContent, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.Null(supportTaskLink);
        }

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
    }

    [Theory]
    [InlineData("person", false, false, true)]
    [InlineData("oneLogin", false, true, true)]
    [InlineData("supportTask", true, true, true)]
    public async Task FirstMatchedOneLogin_RendersMessageWithContextAwareLinks(string contextType, bool expectOneLoginLink, bool expectPersonLink, bool expectMessage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("one-login@example.com"));

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherSigningIn,
            userId: null,
            events:
            [
                new OneLoginUserUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = person.PersonId },
                    OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null },
                    Changes = OneLoginUserUpdatedEventChanges.PersonId
                },
                new AuthorizeAccessRequestCompletedEvent
                {
                    EventId = Guid.NewGuid()
                }
            ]);

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            personId: contextType == "person" ? person.PersonId : null,
            contextType: contextType == "person" ? null : contextType,
            oneLoginSubject: contextType == "oneLogin" ? oneLoginUser.Subject : null,
            supportTaskReference: contextType == "supportTask" ? "SUP-1" : null);

        // Assert
        var message = entry.QuerySelector("[data-testid='person-linked-message']");
        if (expectMessage)
        {
            Assert.NotNull(message);
            Assert.Contains("User authenticated with GOV.UK One Login", message!.TextContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(oneLoginUser.EmailAddress!, message.TextContent);
        }
        else
        {
            Assert.Null(message);
        }

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

    [Fact]
    public async Task PreviouslyMatchedOneLogin_DoesNotRenderFirstMatchMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherSigningIn,
            userId: null,
            events: new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            });

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            contextType: "supportTask",
            supportTaskReference: "SUP-1");

        // Assert
        Assert.Null(entry.QuerySelector("[data-testid='person-linked-message']"));
    }

    [Fact]
    public async Task TrnRequestCreatedEvent_WithoutSupportTask_RendersNotMatchedMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherSigningIn,
            userId: null,
            events:
            [
                new TrnRequestCreatedEvent
                {
                    EventId = Guid.NewGuid(),
                    TrnRequest = EventModels.TrnRequestMetadata.FromModel(new TrnRequestMetadata
                    {
                        ApplicationUserId = Guid.NewGuid(),
                        RequestId = Guid.NewGuid().ToString(),
                        OneLoginUserSubject = oneLoginUser.Subject,
                        EmailAddress = oneLoginUser.EmailAddress,
                        FirstName = person.FirstName,
                        MiddleName = string.Empty,
                        LastName = person.LastName,
                        Name = [person.FirstName, person.LastName],
                        DateOfBirth = person.DateOfBirth!.Value,
                        Status = TrnRequestStatus.Dormant,
                        IdentityVerified = true,
                        CreatedOn = DateTime.UtcNow
                    })
                }
            ]);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: "SUP-1");

        // Assert
        Assert.NotNull(entry.QuerySelector("[data-testid='trn-request-created-message']"));
        Assert.NotNull(entry.QuerySelector("[data-testid='trn-request-not-matched-message']"));
    }

    [Fact]
    public async Task TrnRequestCreatedEvent_WithSupportTask_DoesNotRenderDuplicateNotMatchedMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Open));

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherSigningIn,
            userId: null,
            events:
            [
                new SupportTaskCreatedEvent
                {
                    EventId = Guid.NewGuid(),
                    SupportTask = EventModels.SupportTask.FromModel(supportTask)
                },
                new TrnRequestCreatedEvent
                {
                    EventId = Guid.NewGuid(),
                    TrnRequest = EventModels.TrnRequestMetadata.FromModel(new TrnRequestMetadata
                    {
                        ApplicationUserId = Guid.NewGuid(),
                        RequestId = Guid.NewGuid().ToString(),
                        OneLoginUserSubject = oneLoginUser.Subject,
                        EmailAddress = oneLoginUser.EmailAddress,
                        FirstName = person.FirstName,
                        MiddleName = string.Empty,
                        LastName = person.LastName,
                        Name = [person.FirstName, person.LastName],
                        DateOfBirth = person.DateOfBirth!.Value,
                        Status = TrnRequestStatus.Dormant,
                        IdentityVerified = true,
                        CreatedOn = DateTime.UtcNow
                    })
                }
            ]);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, contextType: "supportTask", supportTaskReference: supportTask.SupportTaskReference);

        // Assert
        Assert.Null(entry.QuerySelector("[data-testid='trn-request-not-matched-message']"));
        Assert.NotNull(entry.QuerySelector("[data-testid='trn-request-created-message']"));
    }

    [Fact]
    public async Task MultipleOneLoginUserUpdatedEvents_RendersAllMessagesInJourneyOrder()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>("new@example.com"));
        var baseline = EventModels.OneLoginUser.FromModel(oneLoginUser) with
        {
            EmailAddress = "old@example.com",
            PersonId = null,
            VerifiedOn = null,
            VerificationRoute = null,
            VerifiedNames = null,
            VerifiedDatesOfBirth = null,
            MatchedOn = null,
            MatchRoute = null,
            MatchedAttributes = null
        };
        var afterEmailChange = baseline with { EmailAddress = "new@example.com" };
        var afterVerification = afterEmailChange with
        {
            VerifiedOn = DateTime.UtcNow,
            VerificationRoute = OneLoginUserVerificationRoute.OneLogin,
            VerifiedNames = [[person.FirstName, person.LastName]],
            VerifiedDatesOfBirth = [person.DateOfBirth!.Value]
        };
        var afterMatch = afterVerification with
        {
            PersonId = person.PersonId,
            MatchedOn = DateTime.UtcNow,
            MatchRoute = OneLoginUserMatchRoute.TrnToken
        };

        // Deliberately supplied out of order
        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherSigningIn,
            userId: null,
            events:
            [
                new OneLoginUserUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    OldOneLoginUser = afterVerification,
                    OneLoginUser = afterMatch,
                    Changes = OneLoginUserUpdatedEventChanges.PersonId | OneLoginUserUpdatedEventChanges.MatchedOn | OneLoginUserUpdatedEventChanges.MatchRoute
                },
                new OneLoginUserUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    OldOneLoginUser = baseline,
                    OneLoginUser = afterEmailChange,
                    Changes = OneLoginUserUpdatedEventChanges.EmailAddress
                },
                new OneLoginUserUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    OldOneLoginUser = afterEmailChange,
                    OneLoginUser = afterVerification,
                    Changes = OneLoginUserUpdatedEventChanges.VerifiedOn | OneLoginUserUpdatedEventChanges.VerificationRoute | OneLoginUserUpdatedEventChanges.VerifiedNames | OneLoginUserUpdatedEventChanges.VerifiedDatesOfBirth
                }
            ]);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId, personId: person.PersonId);

        // Assert
        var emailMessage = entry.QuerySelector("[data-testid='one-login-email-changed-message']");
        var verifiedMessage = entry.QuerySelector("[data-testid='one-login-verified-message']");
        var matchMessage = entry.QuerySelector("[data-testid='one-login-match-route-message']");
        Assert.NotNull(emailMessage);
        Assert.NotNull(verifiedMessage);
        Assert.NotNull(matchMessage);
        Assert.Contains("old@example.com", emailMessage!.TextContent);
        Assert.Contains("new@example.com", emailMessage.TextContent);
        Assert.Contains("via magic link", matchMessage!.TextContent);

        var testIds = entry.QuerySelectorAll("[data-testid]").Select(e => e.GetAttribute("data-testid")).ToArray();
        var emailIndex = Array.IndexOf(testIds, "one-login-email-changed-message");
        var verifiedIndex = Array.IndexOf(testIds, "one-login-verified-message");
        var matchIndex = Array.IndexOf(testIds, "one-login-match-route-message");
        Assert.True(emailIndex < verifiedIndex);
        Assert.True(verifiedIndex < matchIndex);
    }

    [Fact]
    public async Task OneLoginUserUpdatedEvent_NameChange_RendersNameChangedMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var oldUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { VerifiedNames = [["Old", "Name"]] };
        var newUser = oldUser with { VerifiedNames = [["New", "Name"]] };

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherSigningIn,
            userId: null,
            events: new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OldOneLoginUser = oldUser,
                OneLoginUser = newUser,
                Changes = OneLoginUserUpdatedEventChanges.VerifiedNames
            });

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        var message = entry.QuerySelector("[data-testid='one-login-name-changed-message']");
        Assert.NotNull(message);
        Assert.Contains("Old Name", message!.TextContent);
        Assert.Contains("New Name", message.TextContent);
    }
}

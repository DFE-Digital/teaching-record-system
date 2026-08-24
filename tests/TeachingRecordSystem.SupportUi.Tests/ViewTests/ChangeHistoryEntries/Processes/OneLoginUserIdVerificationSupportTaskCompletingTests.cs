using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class OneLoginUserIdVerificationSupportTaskCompletingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task VerifiedOutcome_RendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        // Act
        var entry = await PublishSupportTaskUpdatedEventAsync(supportTask, oneLoginUser);

        // Assert
        AssertTitle(entry, "GOV.UK One Login ID verification task completed");

        var verificationOutcome = entry.GetElementByTestId("verification-outcome");
        Assert.NotNull(verificationOutcome);
        Assert.Contains("verified", verificationOutcome!.TextContent, StringComparison.OrdinalIgnoreCase);

        var taskClosedMessage = entry.GetElementByTestId("task-closed-message");
        Assert.NotNull(taskClosedMessage);
        Assert.Contains(oneLoginUser.EmailAddress!, taskClosedMessage!.TextContent);
    }

    [Fact]
    public async Task NotVerifiedOutcome_RendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        // Act
        var entry = await PublishSupportTaskUpdatedEventAsync(supportTask, oneLoginUser: null, outcome: OneLoginUserIdVerificationOutcome.NotVerified);

        // Assert
        AssertTitle(entry, "GOV.UK One Login ID verification task completed");

        var verificationOutcome = entry.GetElementByTestId("verification-outcome");
        Assert.NotNull(verificationOutcome);
        Assert.Contains("rejected", verificationOutcome!.TextContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task WithOneLoginUserUpdatedEvent_RendersMatchedMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        // Act
        var entry = await PublishSupportTaskUpdatedEventAsync(supportTask, oneLoginUser);

        // Assert
        var matchedMessage = entry.GetElementByTestId("one-login-matched-message");
        Assert.NotNull(matchedMessage);
        Assert.Contains(oneLoginUser.EmailAddress!, matchedMessage!.TextContent);
    }

    [Fact]
    public async Task WithEmailSentEvent_RendersEmailMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        // Act
        var entry = await PublishSupportTaskUpdatedEventAsync(supportTask, oneLoginUser, includeEmailEvent: true);

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
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        var process = await CreateProcessAsync(supportTask, oneLoginUser);

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
        OneLoginUser? oneLoginUser,
        bool includeEmailEvent = false,
        OneLoginUserIdVerificationOutcome outcome = OneLoginUserIdVerificationOutcome.VerifiedAndConnected)
    {
        var process = await CreateProcessAsync(supportTask, oneLoginUser, includeEmailEvent, outcome);
        return await GetEntryHtmlAsync(process.ProcessId);
    }

    private async Task<Process> CreateProcessAsync(
        SupportTask supportTask,
        OneLoginUser? oneLoginUser,
        bool includeEmailEvent = false,
        OneLoginUserIdVerificationOutcome outcome = OneLoginUserIdVerificationOutcome.VerifiedAndConnected)
    {
        var oldSupportTask = EventModels.SupportTask.FromModel(supportTask);
        var supportTaskEventModel = oldSupportTask with
        {
            Status = SupportTaskStatus.Closed,
            Data = (oldSupportTask.Data as OneLoginUserIdVerificationData)! with
            {
                Outcome = outcome
            }
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

        if (oneLoginUser is not null)
        {
            events.Add(new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { VerificationRoute = null },
                Changes = OneLoginUserUpdatedEventChanges.VerificationRoute
            });
        }

        if (includeEmailEvent)
        {
            events.Add(new EmailSentEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = oneLoginUser?.PersonId,
                Email = new EventModels.Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = "template-123",
                    EmailAddress = oneLoginUser?.EmailAddress ?? "test@example.com",
                    Personalization = new Dictionary<string, string>(),
                    Metadata = new Dictionary<string, object>(),
                    SentOn = TimeProvider.UtcNow,
                    EmailReplyToId = null
                }
            });
        }

        return await TestData.CreateProcessAsync(
            ProcessType.OneLoginUserIdVerificationSupportTaskCompleting,
            userId: null,
            changeReason: null,
            events.ToArray());
    }
}

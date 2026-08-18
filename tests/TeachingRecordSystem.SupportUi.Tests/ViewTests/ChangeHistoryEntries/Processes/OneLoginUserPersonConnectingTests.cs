using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class OneLoginUserPersonConnectingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

        // Act
        var entry = await PublishOneLoginUserUpdatedEventAsync(oneLoginUser, changeReason: null);

        // Assert
        AssertTitle(entry, "GOV.UK One Login manually connected to a record");
    }

    [Fact]
    public async Task WithoutChangeReason_DoesNotRenderReason()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

        // Act
        var entry = await PublishOneLoginUserUpdatedEventAsync(oneLoginUser, changeReason: null);

        // Assert
        AssertTitle(entry, "GOV.UK One Login manually connected to a record");
        Assert.Null(entry.GetElementByTestId("change-reason"));
    }

    [Theory]
    [InlineData("Another reason", "Some reason details", "Another reason: Some reason details")]
    [InlineData("New information received", null, "New information received")]
    public async Task WithChangeReason_RendersCorrectly(string reason, string? details, string expectedReasonText)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = reason,
            Details = details,
            AdditionalInformation = null,
            EvidenceFile = null
        };

        // Act
        var entry = await PublishOneLoginUserUpdatedEventAsync(oneLoginUser, changeReason);

        // Assert
        AssertTitle(entry, "GOV.UK One Login manually connected to a record");

        var changeReasonDetails = entry.GetElementByTestId("change-reason");
        Assert.NotNull(changeReasonDetails);

        var changeReasonDetailsSummary = changeReasonDetails.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Reason for change", changeReasonDetailsSummary?.TrimmedText());

        changeReasonDetails.AssertSummaryListRowValueContentMatches("Reason details", expectedReasonText);
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

        var process = await CreateProcessAsync(oneLoginUser);

        // Act
        var entry = await GetEntryHtmlAsync(
            process.ProcessId,
            personId: contextType == "person" ? person.PersonId : null,
            contextType: contextType == "person" ? null : contextType,
            oneLoginSubject: contextType == "oneLogin" ? oneLoginUser.Subject : null,
            supportTaskReference: contextType == "supportTask" ? "SUP-1" : null);

        // Assert
        var oneLoginLink = entry.GetElementByTestId("one-login-link");
        if (expectOneLoginLink)
        {
            Assert.NotNull(oneLoginLink);
            Assert.Contains(oneLoginUser.EmailAddress!, oneLoginLink!.TextContent);
            var href = oneLoginLink.GetAttribute("href")!;
            Assert.True(href.Contains(oneLoginUser.Subject) || href.Contains(Uri.EscapeDataString(oneLoginUser.Subject)));
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
            var href = personLink.GetAttribute("href")!;
            Assert.Contains(person.PersonId.ToString(), href);
        }
        else
        {
            Assert.Null(personLink);
        }
    }

    private async Task<IHtmlElement> PublishOneLoginUserUpdatedEventAsync(OneLoginUser oneLoginUser, IChangeReasonInfo? changeReason)
    {
        var oldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null };

        var process = await TestData.CreateProcessAsync(
            ProcessType.OneLoginUserPersonConnecting,
            changeReason: changeReason,
            events: new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                OldOneLoginUser = oldOneLoginUser,
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            });

        return await GetEntryHtmlAsync(process.ProcessId);
    }

    private async Task<Process> CreateProcessAsync(OneLoginUser oneLoginUser, IChangeReasonInfo? changeReason = null)
    {
        var oldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null };

        return await TestData.CreateProcessAsync(
            ProcessType.OneLoginUserPersonConnecting,
            changeReason: changeReason,
            events: new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                OldOneLoginUser = oldOneLoginUser,
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            });
    }
}

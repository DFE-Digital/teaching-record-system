using Optional;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.SupportTaskDetail;

public class ZendeskTicketsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_ValidSupportTask_DisplaysForm()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/{supportTask.SupportTask.SupportTaskReference}/zendesk-tickets");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_NoZendeskTickets_ShowsNoTicketsAdded()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>(TestData.GenerateUniqueEmail()),
            verifiedInfo: null);

        var supportTask =
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
                oneLoginUser.Subject,
                configure => configure
                    .WithStatedFirstName("Alphie")
                    .WithStatedLastName("Smith")
                    .WithCreatedOn(new DateTime(2025, 1, 22, 1, 1, 1))
                    .WithClientApplicationUserId(applicationUser.UserId));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/{supportTask.SupportTaskReference}/zendesk-tickets");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await AssertEx.HtmlResponseAsync(response);

        var noTickets = document.GetElementByTestId("no-tickets");

        Assert.NotNull(noTickets);
        Assert.Equal("No tickets added", noTickets.TrimmedText());
    }

    [Fact]
    public async Task Post_ZendeskTicketUrlIsEmpty_IsExcludedFromSave()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();
        var ticketUrl1 = "https://example.zendesk.com/agent/tickets/123";
        var ticketUrl2 = "https://example.zendesk.com/agent/tickets/456";

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/{supportTask.SupportTask.SupportTaskReference}/zendesk-tickets")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TicketUrls[0]", "" },
                { "TicketUrls[1]", ticketUrl2 }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var updatedSupportTask = await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.SingleAsync(
                t => t.SupportTaskReference == supportTask.SupportTask.SupportTaskReference));

        Assert.NotNull(updatedSupportTask);
        Assert.DoesNotContain(ticketUrl1, updatedSupportTask.ZendeskTickets);
        Assert.Contains(ticketUrl2, updatedSupportTask.ZendeskTickets);
    }

    [Fact]
    public async Task Post_AddTicket_AddsEmptyTicketInput()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/{supportTask.SupportTask.SupportTaskReference}/zendesk-tickets?handler=AddTicket")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await AssertEx.HtmlResponseAsync(response);

        var ticketInput = document.QuerySelector(
            "input[name='TicketUrls[0]']");

        Assert.NotNull(ticketInput);
    }

    [Fact]
    public async Task Post_ValidTickets_UpdatesSupportTask()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var ticketUrl1 = "https://example.zendesk.com/agent/tickets/123";
        var ticketUrl2 = "https://example.zendesk.com/agent/tickets/456";

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/{supportTask.SupportTask.SupportTaskReference}/zendesk-tickets")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TicketUrls[0]", ticketUrl1 },
                { "TicketUrls[1]", ticketUrl2 }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();

        // Assert
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, "Zendesk tickets updated");

        var updatedSupportTask = await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.SingleAsync(
                t => t.SupportTaskReference == supportTask.SupportTask.SupportTaskReference));

        Assert.Equal(
            [ticketUrl1, ticketUrl2],
            updatedSupportTask.ZendeskTickets);

        Events.AssertProcessesCreated(x =>
        {
            Assert.Equal(ProcessType.SupportTaskZendeskUrlsUpdating, x.ProcessContext.ProcessType);
            Assert.Collection(x.Events, e =>
            {
                var supportTaskZendeskEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                Assert.NotNull(supportTaskZendeskEvent.SupportTask.ZendeskTickets);
                Assert.NotNull(supportTaskZendeskEvent.OldSupportTask.ZendeskTickets);
                Assert.Contains(ticketUrl1, supportTaskZendeskEvent.SupportTask.ZendeskTickets);
                Assert.Contains(ticketUrl2, supportTaskZendeskEvent.SupportTask.ZendeskTickets);
            });
        });
    }

    [Fact]
    public async Task Post_ExistingTicketIsUpdated_UpdatesSupportTask()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(personId: null, email: Option.Some<string?>(TestData.GenerateUniqueEmail()), verifiedInfo: null);
        var existingTicketUrl =
            "https://example.zendesk.com/agent/tickets/123";

        var updatedTicketUrl =
            "https://example.zendesk.com/agent/tickets/456";

        var supportTask =
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
                oneLoginUser1.Subject,
                configure => configure
                    .WithStatedFirstName("Alphie")
                    .WithStatedLastName("Smith")
                    .WithCreatedOn(new DateTime(2025, 1, 22, 1, 1, 1))
                    .WithClientApplicationUserId(applicationUser.UserId)
                    .WithZendeskTickets(existingTicketUrl));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/{supportTask.SupportTaskReference}/zendesk-tickets")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TicketUrls[0]", updatedTicketUrl }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedSupportTask = await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.SingleAsync(
                t => t.SupportTaskReference == supportTask.SupportTaskReference));

        Assert.Equal(
            [updatedTicketUrl],
            updatedSupportTask.ZendeskTickets);

        Events.AssertProcessesCreated(x =>
        {
            Assert.Equal(ProcessType.SupportTaskZendeskUrlsUpdating, x.ProcessContext.ProcessType);
            Assert.Collection(x.Events, e =>
            {
                var supportTaskZendeskEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                Assert.NotNull(supportTaskZendeskEvent.SupportTask.ZendeskTickets);
                Assert.Contains(updatedTicketUrl, supportTaskZendeskEvent.SupportTask.ZendeskTickets);
                Assert.NotNull(supportTaskZendeskEvent.OldSupportTask.ZendeskTickets);
                Assert.Contains(existingTicketUrl, supportTaskZendeskEvent.OldSupportTask.ZendeskTickets);
            });
        });
    }

    [Fact]
    public async Task Post_RemoveAllTickets_UpdatesSupportTask()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>(TestData.GenerateUniqueEmail()),
            verifiedInfo: null);

        var existingTicketUrl1 =
            "https://example.zendesk.com/agent/tickets/123";
        var existingTicketUrl2 =
            "https://example.zendesk.com/agent/tickets/123";


        var supportTask =
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
                oneLoginUser1.Subject,
                configure => configure
                    .WithStatedFirstName("Alphie")
                    .WithStatedLastName("Smith")
                    .WithCreatedOn(new DateTime(2025, 1, 22, 1, 1, 1))
                    .WithClientApplicationUserId(applicationUser.UserId)
                    .WithZendeskTickets(existingTicketUrl1)
                    .WithZendeskTickets(existingTicketUrl2));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/{supportTask.SupportTaskReference}/zendesk-tickets")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TicketUrls[0]", "" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedSupportTask = await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.SingleAsync(
                t => t.SupportTaskReference == supportTask.SupportTaskReference));

        Assert.Empty(updatedSupportTask.ZendeskTickets);

        Events.AssertProcessesCreated(x =>
        {
            Assert.Equal(ProcessType.SupportTaskZendeskUrlsUpdating, x.ProcessContext.ProcessType);
            Assert.Collection(x.Events, e =>
            {
                var supportTaskZendeskEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                Assert.NotNull(supportTaskZendeskEvent.SupportTask.ZendeskTickets);
                Assert.NotNull(supportTaskZendeskEvent.OldSupportTask.ZendeskTickets);
                Assert.Empty(supportTaskZendeskEvent.SupportTask.ZendeskTickets);
                Assert.Contains(existingTicketUrl1, supportTaskZendeskEvent.OldSupportTask.ZendeskTickets);
                Assert.Contains(existingTicketUrl2, supportTaskZendeskEvent.OldSupportTask.ZendeskTickets);
            });
        });
    }

    [Fact]
    public async Task Post_ExistingTicketsAreUpdated_UpdatesSupportTask()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>(TestData.GenerateUniqueEmail()),
            verifiedInfo: null);

        var existingTicketUrl =
            "https://example.zendesk.com/agent/tickets/123";

        var updatedTicketUrl1 =
            "https://example.zendesk.com/agent/tickets/456";

        var updatedTicketUrl2 =
            "https://example.zendesk.com/agent/tickets/789";

        var supportTask =
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
                oneLoginUser1.Subject,
                configure => configure
                    .WithStatedFirstName("Alphie")
                    .WithStatedLastName("Smith")
                    .WithCreatedOn(new DateTime(2025, 1, 22, 1, 1, 1))
                    .WithClientApplicationUserId(applicationUser.UserId)
                    .WithZendeskTickets(existingTicketUrl));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/{supportTask.SupportTaskReference}/zendesk-tickets")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TicketUrls[0]", updatedTicketUrl1 },
                { "TicketUrls[1]", updatedTicketUrl2 }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedSupportTask = await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.SingleAsync(
                t => t.SupportTaskReference == supportTask.SupportTaskReference));

        Assert.Equal(
            [updatedTicketUrl1, updatedTicketUrl2],
            updatedSupportTask.ZendeskTickets);

        Events.AssertProcessesCreated(x =>
        {
            Assert.Equal(ProcessType.SupportTaskZendeskUrlsUpdating, x.ProcessContext.ProcessType);
            Assert.Collection(x.Events, e =>
            {
                var supportTaskZendeskEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                Assert.NotNull(supportTaskZendeskEvent.SupportTask.ZendeskTickets);
                Assert.NotNull(supportTaskZendeskEvent.OldSupportTask.ZendeskTickets);
                Assert.Contains(updatedTicketUrl1, supportTaskZendeskEvent.SupportTask.ZendeskTickets);
                Assert.Contains(updatedTicketUrl2, supportTaskZendeskEvent.SupportTask.ZendeskTickets);
                Assert.Contains(existingTicketUrl, supportTaskZendeskEvent.OldSupportTask.ZendeskTickets);
            });
        });
    }

    [Fact]
    public async Task Post_OneTicketIsInvalid_ReturnsError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>(TestData.GenerateUniqueEmail()),
            verifiedInfo: null);

        var ticketUrl1 =
            "https://example.zendesk.com/agent/tickets/123";

        var ticketUrl2 =
            "https://example.zendesk.com/agent/tickets/456";

        var invalidTicketUrl =
            "https://example.com/tickets/789";

        var supportTask =
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
                oneLoginUser1.Subject,
                configure => configure
                    .WithStatedFirstName("Alphie")
                    .WithStatedLastName("Smith")
                    .WithCreatedOn(new DateTime(2025, 1, 22, 1, 1, 1))
                    .WithClientApplicationUserId(applicationUser.UserId));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/{supportTask.SupportTaskReference}/zendesk-tickets")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TicketUrls[0]", ticketUrl1 },
                { "TicketUrls[1]", ticketUrl2 },
                { "TicketUrls[2]", invalidTicketUrl }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(
            response,
            "TicketUrls_2_",
            "Enter a valid Zendesk URL");
    }
}

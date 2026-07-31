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
    public async Task Get_NoZendeskTickets_ShowsNoTicketRows()
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

        Assert.Null(document.GetElementByTestId("zendesk-ticket-0"));
        Assert.NotNull(document.GetElementByTestId("zendesk-ticket-new"));
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
                { "TicketUrls", "" },
                { "TicketUrls", ticketUrl2 }
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

        var ticketInput = document.QuerySelector("input#TicketUrls-0");

        Assert.NotNull(ticketInput);
        Assert.Equal("TicketUrls", ticketInput.GetAttribute("name"));
        Assert.True(string.IsNullOrEmpty(ticketInput.GetAttribute("value")));
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
                { "TicketUrls", ticketUrl1 },
                { "TicketUrls", ticketUrl2 }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();

        // Assert
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, "Zendesk tickets updated");

        var details = redirectDoc.GetElementByTestId("zendesk-tickets");
        Assert.NotNull(details);
        Assert.True(details.HasAttribute("open"));

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
    public async Task Post_ValidTicketsWithReturnUrl_RedirectsToDetailWithReturnUrlPreserved()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();
        var returnUrl = "/support-tasks/active?sortBy=Source";

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/{supportTask.SupportTask.SupportTaskReference}/zendesk-tickets?returnUrl={Uri.EscapeDataString(returnUrl)}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TicketUrls", "https://example.zendesk.com/agent/tickets/123" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var location = response.Headers.Location?.OriginalString;
        Assert.NotNull(location);
        Assert.Contains("xpandZendeskTickets=True", location);
        Assert.Contains($"returnUrl={Uri.EscapeDataString(returnUrl)}", location);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        Assert.Equal(returnUrl, redirectDoc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
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
                { "TicketUrls", updatedTicketUrl }
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
                { "TicketUrls", "" },
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
                { "TicketUrls", updatedTicketUrl1 },
                { "TicketUrls", updatedTicketUrl2 }
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
    public async Task Post_TicketsAreUnchanged_DoesNotPublishEventOrShowNotification()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>(TestData.GenerateUniqueEmail()),
            verifiedInfo: null);

        var existingTicketUrl =
            "https://example.zendesk.com/agent/tickets/123";

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
                { "TicketUrls", existingTicketUrl }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();

        Assert.Empty(redirectDoc.GetElementsByClassName("govuk-notification-banner"));

        var updatedSupportTask = await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.SingleAsync(
                t => t.SupportTaskReference == supportTask.SupportTaskReference));

        Assert.Equal([existingTicketUrl], updatedSupportTask.ZendeskTickets);

        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task Post_TicketIsAddedToOpenSupportTask_SetsStatusToInProgress()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var oneLoginUser1 = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>(TestData.GenerateUniqueEmail()),
            verifiedInfo: null);

        var addedTicketUrl =
            "https://example.zendesk.com/agent/tickets/123";

        var supportTask =
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
                oneLoginUser1.Subject,
                configure => configure
                    .WithStatedFirstName("Alphie")
                    .WithStatedLastName("Smith")
                    .WithCreatedOn(new DateTime(2025, 1, 22, 1, 1, 1))
                    .WithClientApplicationUserId(applicationUser.UserId)
                    .WithStatus(SupportTaskStatus.Open));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/{supportTask.SupportTaskReference}/zendesk-tickets")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TicketUrls", addedTicketUrl }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedSupportTask = await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.SingleAsync(
                t => t.SupportTaskReference == supportTask.SupportTaskReference));

        Assert.Equal(SupportTaskStatus.InProgress, updatedSupportTask.Status);

        Events.AssertProcessesCreated(x =>
        {
            Assert.Equal(ProcessType.SupportTaskZendeskUrlsUpdating, x.ProcessContext.ProcessType);
            Assert.Collection(x.Events, e =>
            {
                var supportTaskZendeskEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                Assert.Equal(
                    SupportTaskUpdatedEventChanges.ZendeskTicketUrls | SupportTaskUpdatedEventChanges.Status,
                    supportTaskZendeskEvent.Changes);
                Assert.Equal(SupportTaskStatus.Open, supportTaskZendeskEvent.OldSupportTask.Status);
                Assert.Equal(SupportTaskStatus.InProgress, supportTaskZendeskEvent.SupportTask.Status);
            });
        });
    }

    [Fact]
    public async Task Post_TicketIsRemovedFromOpenSupportTask_LeavesStatusUnchanged()
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
            "https://example.zendesk.com/agent/tickets/456";

        var supportTask =
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
                oneLoginUser1.Subject,
                configure => configure
                    .WithStatedFirstName("Alphie")
                    .WithStatedLastName("Smith")
                    .WithCreatedOn(new DateTime(2025, 1, 22, 1, 1, 1))
                    .WithClientApplicationUserId(applicationUser.UserId)
                    .WithStatus(SupportTaskStatus.Open)
                    .WithZendeskTickets(existingTicketUrl1, existingTicketUrl2));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/{supportTask.SupportTaskReference}/zendesk-tickets")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TicketUrls", existingTicketUrl1 }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedSupportTask = await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.SingleAsync(
                t => t.SupportTaskReference == supportTask.SupportTaskReference));

        Assert.Equal([existingTicketUrl1], updatedSupportTask.ZendeskTickets);
        Assert.Equal(SupportTaskStatus.Open, updatedSupportTask.Status);

        Events.AssertProcessesCreated(x =>
        {
            Assert.Equal(ProcessType.SupportTaskZendeskUrlsUpdating, x.ProcessContext.ProcessType);
            Assert.Collection(x.Events, e =>
            {
                var supportTaskZendeskEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                Assert.Equal(SupportTaskUpdatedEventChanges.ZendeskTicketUrls, supportTaskZendeskEvent.Changes);
                Assert.Equal(SupportTaskStatus.Open, supportTaskZendeskEvent.SupportTask.Status);
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
                { "TicketUrls", ticketUrl1 },
                { "TicketUrls", ticketUrl2 },
                { "TicketUrls", invalidTicketUrl }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(
            response,
            "TicketUrls-2",
            "Enter a valid Zendesk URL");
    }
}

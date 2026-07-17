using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.SupportTaskDetail;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/NON-EXISTENT");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_OutstandingTask_DisplaysOverviewFormAndNotesSection()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.NotNull(doc.GetElementById("AssignedToUserId"));
        Assert.NotNull(doc.GetElementById("Status"));
        Assert.Contains(supportTask.SupportTaskReference, doc.Body!.TextContent);
        Assert.Contains(supportTask.GetSubject(), doc.Body!.TextContent);
    }

    [Fact]
    public async Task Get_OutstandingTask_PreSelectsAssignedUserAndStatus()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();
        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            dbTask.AssignedToUserId = user.UserId;
            dbTask.Status = SupportTaskStatus.InProgress;
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var assignedToSelect = (IHtmlSelectElement)doc.GetElementById("AssignedToUserId")!;
        Assert.Equal(user.UserId.ToString(), assignedToSelect.Value);

        var statusSelect = (IHtmlSelectElement)doc.GetElementById("Status")!;
        Assert.Equal(SupportTaskStatus.InProgress.ToString(), statusSelect.Value);
    }

    [Fact]
    public async Task Get_AssignToOptions_OnlyIncludesAccessManagerAndRecordManagerUsers()
    {
        // Arrange
        var recordManager = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        var accessManager = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        var administrator = await TestData.CreateUserAsync(role: UserRoles.Administrator);

        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var optionValues = ((IHtmlSelectElement)doc.GetElementById("AssignedToUserId")!)
            .Options
            .Select(o => o.Value)
            .ToArray();

        Assert.Contains(recordManager.UserId.ToString(), optionValues);
        Assert.Contains(accessManager.UserId.ToString(), optionValues);
        Assert.DoesNotContain(administrator.UserId.ToString(), optionValues);
    }

    [Fact]
    public async Task Get_OutstandingTask_DisplaysNotesInDescendingCreatedOrder()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var olderContent = "This is the older note";
        var newerContent = "This is the newer note";

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.SupportTaskNotes.Add(new SupportTaskNote
            {
                SupportTaskNoteId = Guid.NewGuid(),
                SupportTaskReference = supportTask.SupportTaskReference,
                Content = olderContent,
                CreatedOn = TimeProvider.UtcNow,
                CreatedByUserId = GetCurrentUserId()
            });
            dbContext.SupportTaskNotes.Add(new SupportTaskNote
            {
                SupportTaskNoteId = Guid.NewGuid(),
                SupportTaskReference = supportTask.SupportTaskReference,
                Content = newerContent,
                CreatedOn = TimeProvider.UtcNow.AddMinutes(1),
                CreatedByUserId = GetCurrentUserId()
            });
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var bodyText = doc.Body!.TextContent;

        Assert.Contains(olderContent, bodyText);
        Assert.Contains(newerContent, bodyText);
        Assert.True(bodyText.IndexOf(newerContent, StringComparison.Ordinal) < bodyText.IndexOf(olderContent, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_ExpandNotesQueryParameter_ControlsWhetherNotesAreExpanded(bool expandNotes)
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/{supportTask.SupportTaskReference}?ExpandNotes={expandNotes}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var details = doc.QuerySelector(".govuk-details");
        Assert.NotNull(details);
        Assert.Equal(expandNotes, details.HasAttribute("open"));
    }

    [Fact]
    public async Task Get_ClosedTask_DisplaysKeyTaskDetailsAndNotOverviewForm()
    {
        // Arrange
        var completedOn = TimeProvider.UtcNow;
        var (supportTask, completedByUser) = await CreateClosedSupportTaskAsync(
            outcome: SupportTaskOutcome.ChangeNameRequest_Approved,
            completedOn: completedOn);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/{supportTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        // The overview form is only shown for outstanding tasks
        Assert.Null(doc.GetElementById("Status"));
        Assert.Null(doc.GetElementById("AssignedToUserId"));

        var summaryList = doc.QuerySelector(".govuk-summary-list");
        Assert.NotNull(summaryList);
        Assert.Equal("Approved", summaryList.GetSummaryListValueByKey("Outcome"));
        Assert.Equal(supportTask.SupportTaskType.GetTitle(), summaryList.GetSummaryListValueByKey("Type"));
        Assert.Equal(completedOn.ToString(WebConstants.DateDisplayFormat), summaryList.GetSummaryListValueByKey("Completed on"));
        Assert.Equal(completedByUser.Name, summaryList.GetSummaryListValueByKey("Completed by"));
    }

    [Fact]
    public async Task Post_ValidChanges_UpdatesTaskPublishesEventAndRedirects()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/{supportTask.SupportTaskReference}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AssignedToUserId", user.UserId.ToString() },
                { "Status", SupportTaskStatus.InProgress.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/{supportTask.SupportTaskReference}", response.Headers.Location?.OriginalString);

        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.InProgress, dbTask.Status);
            Assert.Equal(user.UserId, dbTask.AssignedToUserId);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.SupportTaskAllocating, p.ProcessContext.ProcessType);
            Assert.Collection(p.Events, e =>
            {
                var updatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                Assert.Equal(supportTask.SupportTaskReference, updatedEvent.SupportTaskReference);
                Assert.Equal(user.UserId, updatedEvent.SupportTask.AssignedToUserId);
                Assert.Equal(
                    SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.AssignedToUserId,
                    updatedEvent.Changes);
            });
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(nextPageDoc, "Task updated");
    }

    [Fact]
    public async Task Post_NoChanges_RedirectsWithoutPublishingEvent()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/{supportTask.SupportTaskReference}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AssignedToUserId", "" },
                { "Status", SupportTaskStatus.Open.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/{supportTask.SupportTaskReference}", response.Headers.Location?.OriginalString);

        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task Post_ClosedTask_ReturnsBadRequest()
    {
        // Arrange
        var (supportTask, _) = await CreateClosedSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/{supportTask.SupportTaskReference}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AssignedToUserId", "" },
                { "Status", SupportTaskStatus.InProgress.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task Post_InvalidStatus_ReturnsBadRequest()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/{supportTask.SupportTaskReference}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AssignedToUserId", "" },
                { "Status", SupportTaskStatus.Closed.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task Post_AssignedUserNotInOptions_ReturnsBadRequest()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/{supportTask.SupportTaskReference}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AssignedToUserId", Guid.NewGuid().ToString() },
                { "Status", SupportTaskStatus.InProgress.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        Events.AssertNoEventsPublished();
    }

    private async Task<(SupportTask SupportTask, User CompletedByUser)> CreateClosedSupportTaskAsync(
        SupportTaskOutcome outcome = SupportTaskOutcome.ChangeNameRequest_Approved,
        DateTime? completedOn = null)
    {
        var completedByUser = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            configure: b => b.WithStatus(SupportTaskStatus.Closed));

        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            dbTask.Outcome = outcome;
            dbTask.CompletedOn = completedOn ?? TimeProvider.UtcNow;
            dbTask.CompletedByUserId = completedByUser.UserId;
            await dbContext.SaveChangesAsync();
        });

        return (supportTask, completedByUser);
    }
}

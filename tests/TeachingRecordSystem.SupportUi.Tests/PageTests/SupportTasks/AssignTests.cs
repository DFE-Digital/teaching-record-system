using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks;

public class AssignTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoSupportTaskReferences_ReturnsBadRequest()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/assign");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithSelectedTasks_DisplaysTasksAndAssignableUsers()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(name: "Reviewer One", role: UserRoles.RecordManager);
        var supportTask1 = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20)));
        var supportTask2 = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21)));

        var request = new HttpRequestMessage(HttpMethod.Get, GetAssignUrl(supportTask1.SupportTaskReference, supportTask2.SupportTaskReference));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var references = doc.QuerySelectorAll("[data-testid=task-reference]").Select(e => e.TrimmedText()).ToArray();
        Assert.Equal([supportTask1.SupportTaskReference, supportTask2.SupportTaskReference], references);

        var optionValues = doc.GetElementById("AssignToUserId")!
            .QuerySelectorAll("option")
            .Select(o => o.GetAttribute("value"))
            .ToArray();
        Assert.Contains(user.UserId.ToString(), optionValues);
    }

    [Fact]
    public async Task Get_AssignToOptions_StartWithUnassignedAndMyselfAndExcludeCurrentUserFromUserList()
    {
        // Arrange
        var currentUser = await CreateAndSetCurrentUserAsync(name: "Current User");
        var otherUser = await TestData.CreateUserAsync(name: "Other User", role: UserRoles.RecordManager);
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, GetAssignUrl(supportTask.SupportTaskReference));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var options = doc.GetElementById("AssignToUserId")!
            .QuerySelectorAll("option")
            .Select(o => (Value: o.GetAttribute("value"), Text: o.TrimmedText()))
            .ToArray();

        Assert.Equal(
            [
                ("", ""),
                (SupportTaskSearchService.UnassignedUserId.ToString(), "Unassigned"),
                (currentUser.UserId.ToString(), "Myself")
            ],
            options.Take(3));
        Assert.Contains((otherUser.UserId.ToString(), "Other User"), options);
        Assert.DoesNotContain(options, o => o.Value == currentUser.UserId.ToString() && o.Text != "Myself");
    }

    [Fact]
    public async Task Get_CurrentUserIsNotAssignable_AssignToOptionsDoNotIncludeMyself()
    {
        // Arrange
        SupportTaskAssignmentOptions.IncludeAdministrators = false;

        var currentUser = await TestData.CreateUserAsync(name: "Current User", role: UserRoles.Administrator);
        SetCurrentUser(currentUser);
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, GetAssignUrl(supportTask.SupportTaskReference));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var options = doc.GetElementById("AssignToUserId")!
            .QuerySelectorAll("option")
            .Select(o => (Value: o.GetAttribute("value"), Text: o.TrimmedText()))
            .ToArray();

        Assert.DoesNotContain(options, o => o.Text == "Myself");
        Assert.DoesNotContain(options, o => o.Value == currentUser.UserId.ToString());
    }

    [Fact]
    public async Task Get_ShowsTasksInTheOrderTheyWereSelected()
    {
        // Arrange
        var olderTask = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20)));
        var newerTask = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21)));

        var request = new HttpRequestMessage(HttpMethod.Get, GetAssignUrl(newerTask.SupportTaskReference, olderTask.SupportTaskReference));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var references = doc.QuerySelectorAll("[data-testid=task-reference]").Select(e => e.TrimmedText()).ToArray();
        Assert.Equal([newerTask.SupportTaskReference, olderTask.SupportTaskReference], references);
    }

    [Fact]
    public async Task Get_ExcludesTasksThatAreNoLongerOutstanding()
    {
        // Arrange
        var outstandingTask = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20)));
        var closedTask = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, GetAssignUrl(outstandingTask.SupportTaskReference, closedTask.SupportTaskReference));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var references = doc.QuerySelectorAll("[data-testid=task-reference]").Select(e => e.TrimmedText()).ToArray();
        Assert.Equal([outstandingTask.SupportTaskReference], references);
    }

    [Fact]
    public async Task Get_NoTasksAreStillOutstanding_RedirectsToActive()
    {
        // Arrange
        var closedTask = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, GetAssignUrl(closedTask.SupportTaskReference));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/active", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_NoUserSelected_ReturnsErrorAndDoesNotPublishEvent()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetAssignUrl(supportTask.SupportTaskReference))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AssignToUserId", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "AssignToUserId", "Select who to assign the tasks to");
        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task Post_SelectedUserIsNotAnAssignableOption_ReturnsBadRequest()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetAssignUrl(supportTask.SupportTaskReference))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AssignToUserId", Guid.NewGuid().ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task Post_ValidSelection_AssignsTasksPublishesEventsAndRedirects()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(name: "Reviewer One", role: UserRoles.RecordManager);
        var supportTask1 = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20)));
        var supportTask2 = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21)));

        var request = new HttpRequestMessage(HttpMethod.Post, GetAssignUrl(supportTask1.SupportTaskReference, supportTask2.SupportTaskReference))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AssignToUserId", user.UserId.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/active", response.Headers.Location?.OriginalString);

        await WithDbContextAsync(async dbContext =>
        {
            var dbTasks = await dbContext.SupportTasks
                .Where(t => t.SupportTaskReference == supportTask1.SupportTaskReference || t.SupportTaskReference == supportTask2.SupportTaskReference)
                .ToArrayAsync();
            Assert.All(dbTasks, t => Assert.Equal(user.UserId, t.AssignedToUserId));
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.SupportTasksAssigning, p.ProcessContext.ProcessType);
            Assert.Collection(
                p.Events,
                e =>
                {
                    var updatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                    Assert.Equal(supportTask1.SupportTaskReference, updatedEvent.SupportTaskReference);
                    Assert.Equal(user.UserId, updatedEvent.SupportTask.AssignedToUserId);
                    Assert.Equal(SupportTaskUpdatedEventChanges.AssignedToUserId, updatedEvent.Changes);
                },
                e =>
                {
                    var updatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                    Assert.Equal(supportTask2.SupportTaskReference, updatedEvent.SupportTaskReference);
                    Assert.Equal(user.UserId, updatedEvent.SupportTask.AssignedToUserId);
                    Assert.Equal(SupportTaskUpdatedEventChanges.AssignedToUserId, updatedEvent.Changes);
                });
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(nextPageDoc, "2 tasks assigned to Reviewer One");
    }

    [Fact]
    public async Task Post_SelectedUserIsCurrentUser_AssignsTasksToCurrentUser()
    {
        // Arrange
        var currentUser = await CreateAndSetCurrentUserAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetAssignUrl(supportTask.SupportTaskReference))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AssignToUserId", currentUser.UserId.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(currentUser.UserId, dbTask.AssignedToUserId);
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(nextPageDoc, "1 task assigned to you");
    }

    [Fact]
    public async Task Post_UnassignedSelected_RemovesTheAssignmentFromTasks()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();
        await AssignToUserAsync(supportTask.SupportTaskReference, user.UserId);

        var request = new HttpRequestMessage(HttpMethod.Post, GetAssignUrl(supportTask.SupportTaskReference))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AssignToUserId", SupportTaskSearchService.UnassignedUserId.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Null(dbTask.AssignedToUserId);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.SupportTasksAssigning, p.ProcessContext.ProcessType);
            Assert.Collection(
                p.Events,
                e =>
                {
                    var updatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                    Assert.Equal(supportTask.SupportTaskReference, updatedEvent.SupportTaskReference);
                    Assert.Null(updatedEvent.SupportTask.AssignedToUserId);
                    Assert.Equal(SupportTaskUpdatedEventChanges.AssignedToUserId, updatedEvent.Changes);
                });
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(nextPageDoc, "1 task unassigned");
    }

    [Fact]
    public async Task Post_WithReturnUrl_RedirectsToReturnUrl()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();
        var returnUrl = "/support-tasks/active?status=InProgress";

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{GetAssignUrl(supportTask.SupportTaskReference)}&returnUrl={Uri.EscapeDataString(returnUrl)}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AssignToUserId", user.UserId.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(returnUrl, response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_TaskAlreadyAssignedToSelectedUser_DoesNotPublishEventForThatTask()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        var alreadyAssignedTask = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20)));
        var unassignedTask = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21)));
        await AssignToUserAsync(alreadyAssignedTask.SupportTaskReference, user.UserId);

        var request = new HttpRequestMessage(HttpMethod.Post, GetAssignUrl(alreadyAssignedTask.SupportTaskReference, unassignedTask.SupportTaskReference))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "AssignToUserId", user.UserId.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.SupportTasksAssigning, p.ProcessContext.ProcessType);
            Assert.Collection(
                p.Events,
                e =>
                {
                    var updatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                    Assert.Equal(unassignedTask.SupportTaskReference, updatedEvent.SupportTaskReference);
                });
        });
    }

    private async Task<User> CreateAndSetCurrentUserAsync(string? name = null)
    {
        // The current user from HostFixture isn't in the database once it's been cleared,
        // so create a user we can assign tasks to and sign in as them
        var user = await TestData.CreateUserAsync(name: name, role: UserRoles.RecordManager);
        SetCurrentUser(user);
        return user;
    }

    private static string GetAssignUrl(params string[] supportTaskReferences)
    {
        var query = string.Join("&", supportTaskReferences.Select(r => $"SupportTaskReference={Uri.EscapeDataString(r)}"));
        return $"/support-tasks/assign?{query}";
    }

    private Task AssignToUserAsync(string supportTaskReference, Guid userId) =>
        WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.FindAsync(supportTaskReference);
            dbTask!.AssignedToUserId = userId;
            await dbContext.SaveChangesAsync();
        });
}

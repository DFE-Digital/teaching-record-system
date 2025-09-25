using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ChangeRequests.EditChangeRequest;

public class RejectTests : TestBase
{
    public RejectTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.RecordManager));
    }

    [Fact]
    public async Task Get_WhenUserHasNoRoles_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithPersonDataSource(TestDataPersonDataSource.Trs));
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{supportTask.SupportTaskReference}/reject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [RoleNamesData(except: [UserRoles.RecordManager, UserRoles.AccessManager, UserRoles.Administrator])]
    public async Task Get_WhenUserDoesNotHaveSupportOfficerOrAccessManagerOrAdministratorRole_ReturnsForbidden(string role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithPersonDataSource(TestDataPersonDataSource.Trs));
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{supportTask.SupportTaskReference}/reject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithSupportTaskReferenceForNonExistentSupportTask_ReturnsNotFound()
    {
        // Arrange
        var nonExistentSupportTaskReference = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{nonExistentSupportTaskReference}/reject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithSupportTaskReferenceForClosedSupportTask_ReturnsNotFound()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs));
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)).WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{supportTask.SupportTaskReference}/reject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenRejectionReasonChoiceHasNoSelection_ReturnsError()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithPersonDataSource(TestDataPersonDataSource.Trs));
        var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth)));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/change-requests/{supportTask.SupportTaskReference}/reject")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "RejectionReasonChoice", "Select the reason for rejecting this change");
    }

    [Theory]
    [RoleNamesData(except: [UserRoles.RecordManager, UserRoles.AccessManager, UserRoles.Administrator])]
    public async Task Post_WhenUserDoesNotHaveSupportOfficerOrAccessManagerOrAdministratorRole_ReturnsForbidden(string role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithPersonDataSource(TestDataPersonDataSource.CrmAndTrs));
        var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth)));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/change-requests/{supportTask.SupportTaskReference}/reject")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "RejectionReasonChoice", "RequestAndProofDontMatch" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_WhenRejectionReasonChoiceIsNotChangeNoLongerRequired_RedirectsWithFlashMessage(bool isNameChange)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithPersonDataSource(TestDataPersonDataSource.Trs));
        SupportTask supportTask;
        if (isNameChange)
        {
            supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));
        }
        else
        {
            supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth)));
        }

        EventPublisher.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/change-requests/{supportTask.SupportTaskReference}/reject")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "RejectionReasonChoice", "RequestAndProofDontMatch" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await WithDbContext(async dbContext =>
        {
            var supportTask = await dbContext.SupportTasks.SingleOrDefaultAsync(t => t.PersonId == createPersonResult.PersonId);
            Assert.Equal(SupportTaskStatus.Closed, supportTask!.Status);

            if (isNameChange)
            {
                var requestData = (ChangeNameRequestData)supportTask!.Data;
                Assert.Equal(SupportRequestOutcome.Rejected, requestData!.ChangeRequestOutcome);
                var email = await dbContext.Emails
                    .Where(e => e.EmailAddress == requestData.EmailAddress)
                    .SingleOrDefaultAsync();
                Assert.NotNull(email);
                Assert.NotNull(email.SentOn);
                Assert.Equal(ChangeRequestEmailConstants.GetAnIdentityChangeOfNameRejectedEmailConfirmationTemplateId, email.TemplateId);
            }
            else
            {
                var requestData = (ChangeDateOfBirthRequestData)supportTask!.Data;
                Assert.Equal(SupportRequestOutcome.Rejected, requestData!.ChangeRequestOutcome);
                var email = await dbContext.Emails
                    .Where(e => e.EmailAddress == requestData.EmailAddress)
                    .SingleOrDefaultAsync();
                Assert.NotNull(email);
                Assert.NotNull(email.SentOn);
                Assert.Equal(ChangeRequestEmailConstants.GetAnIdentityChangeOfDateOfBirthRejectedEmailConfirmationTemplateId, email.TemplateId);
            }
        });

        EventPublisher.AssertEventsSaved(e =>
        {
            if (isNameChange)
            {
                var actualEvent = Assert.IsType<ChangeNameRequestSupportTaskRejectedEvent>(e);
                Assert.Equal("Request and proof don’t match", actualEvent.RejectionReason);
                Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
                Assert.Equal(SupportTaskStatus.Open, actualEvent.OldSupportTask.Status);
                Assert.Equal(SupportTaskStatus.Closed, actualEvent.SupportTask.Status);
            }
            else
            {
                var actualEvent = Assert.IsType<ChangeDateOfBirthRequestSupportTaskRejectedEvent>(e);
                Assert.Equal("Request and proof don’t match", actualEvent.RejectionReason);
                Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
                Assert.Equal(SupportTaskStatus.Open, actualEvent.OldSupportTask.Status);
                Assert.Equal(SupportTaskStatus.Closed, actualEvent.SupportTask.Status);
            }
        },
        e2 =>
        {
            var emailEvent = Assert.IsType<EmailSentEvent>(e2);
        });

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "The request has been rejected");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_WhenRejectionReasonChoiceIsChangeNoLongerRequired_RedirectsWithFlashMessage(bool isNameChange)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync(p => p.WithPersonDataSource(TestDataPersonDataSource.Trs));
        SupportTask supportTask;
        if (isNameChange)
        {
            supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));
        }
        else
        {
            supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth)));
        }

        EventPublisher.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/change-requests/{supportTask.SupportTaskReference}/reject")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "RejectionReasonChoice", "ChangeNoLongerRequired" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await WithDbContext(async dbContext =>
        {
            var supportTask = await dbContext.SupportTasks.SingleOrDefaultAsync(t => t.PersonId == createPersonResult.PersonId);
            Assert.Equal(SupportTaskStatus.Closed, supportTask!.Status);

            if (isNameChange)
            {
                var requestData = (ChangeNameRequestData)supportTask!.Data;
                Assert.Equal(SupportRequestOutcome.Cancelled, requestData!.ChangeRequestOutcome);
            }
            else
            {
                var requestData = (ChangeDateOfBirthRequestData)supportTask!.Data;
                Assert.Equal(SupportRequestOutcome.Cancelled, requestData!.ChangeRequestOutcome);
            }
        });

        EventPublisher.AssertEventsSaved(e =>
        {
            SupportTaskUpdatedEvent? actualEvent;
            if (isNameChange)
            {
                actualEvent = Assert.IsType<ChangeNameRequestSupportTaskCancelledEvent>(e);
            }
            else
            {
                actualEvent = Assert.IsType<ChangeDateOfBirthRequestSupportTaskCancelledEvent>(e);
            }

            Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
            Assert.Equal(SupportTaskStatus.Open, actualEvent.OldSupportTask.Status);
            Assert.Equal(SupportTaskStatus.Closed, actualEvent.SupportTask.Status);
        });

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "The request has been cancelled");
    }
}

using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ChangeRequests.EditChangeRequest;

public class AcceptTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Before(Test)]
    public async Task SetUser() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.RecordManager));

    [Test]
    public async Task Get_WhenUserHasNoRoles_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: null));
        var createPersonResult = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{supportTask.SupportTaskReference}/accept");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    [RoleNamesData(except: [UserRoles.RecordManager, UserRoles.AccessManager, UserRoles.Administrator])]
    public async Task Get_WhenUserDoesNotHaveSupportOfficerOrAccessManagerOrAdministratorRole_ReturnsForbidden(string role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));
        var createPersonResult = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{supportTask.SupportTaskReference}/accept");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_WithSupportTaskReferenceForNonExistentSupportTask_ReturnsNotFound()
    {
        // Arrange
        var nonExistentSupportTaskReference = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{nonExistentSupportTaskReference}/accept");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_WithSupportTaskReferenceForClosedSupportTask_ReturnsNotFound()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)).WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{supportTask.SupportTaskReference}/accept");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    [RoleNamesData(except: [UserRoles.RecordManager, UserRoles.AccessManager, UserRoles.Administrator])]
    public async Task Post_WhenUserDoesNotHaveSupportOfficerOrAccessManagerOrAdministratorRole_ReturnsForbidden(string role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));
        var createPersonResult = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth)));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/change-requests/{supportTask.SupportTaskReference}/accept")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Post_ValidRequest_RedirectsWithFlashMessage(bool isNameChange)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        SupportTask supportTask;
        if (isNameChange)
        {
            supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b
                    .WithFirstName(TestData.GenerateChangedFirstName(createPersonResult.FirstName))
                    .WithMiddleName(TestData.GenerateChangedMiddleName(createPersonResult.MiddleName))
                    .WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));
        }
        else
        {
            supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth)));
        }

        EventObserver.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/change-requests/{supportTask.SupportTaskReference}/accept")
        {
            Content = new FormUrlEncodedContentBuilder()
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
                Assert.Equal(SupportRequestOutcome.Approved, requestData!.ChangeRequestOutcome);
                var email = await dbContext.Emails
                    .Where(e => e.EmailAddress == requestData.EmailAddress)
                    .SingleOrDefaultAsync();
                Assert.NotNull(email);
                Assert.NotNull(email.SentOn);
                Assert.Equal(EmailTemplateIds.GetAnIdentityChangeOfNameApprovedEmailConfirmation, email.TemplateId);

                var updatedPerson = await dbContext.Persons
                    .SingleAsync(p => p.PersonId == createPersonResult.PersonId);
                Assert.Equal(requestData.FirstName, updatedPerson.FirstName);
                Assert.Equal(requestData.MiddleName, updatedPerson.MiddleName);
                Assert.Equal(requestData.LastName, updatedPerson.LastName);
                Assert.Equal(Clock.UtcNow, updatedPerson.UpdatedOn);

                var previousName = await dbContext.PreviousNames
                    .SingleOrDefaultAsync(pn => pn.PersonId == createPersonResult.PersonId);
                Assert.NotNull(previousName);
                Assert.Equal(createPersonResult.FirstName, previousName!.FirstName);
                Assert.Equal(createPersonResult.MiddleName, previousName.MiddleName);
                Assert.Equal(createPersonResult.LastName, previousName.LastName);
            }
            else
            {
                var requestData = (ChangeDateOfBirthRequestData)supportTask!.Data;
                Assert.Equal(SupportRequestOutcome.Approved, requestData!.ChangeRequestOutcome);
                var email = await dbContext.Emails
                    .Where(e => e.EmailAddress == requestData.EmailAddress)
                    .SingleOrDefaultAsync();
                Assert.NotNull(email);
                Assert.NotNull(email.SentOn);
                Assert.Equal(EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthApprovedEmailConfirmation, email.TemplateId);
            }
        });

        EventObserver.AssertEventsSaved(e =>
        {
            if (isNameChange)
            {
                var actualEvent = Assert.IsType<ChangeNameRequestSupportTaskApprovedEvent>(e);
                Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
                Assert.Equal(SupportTaskStatus.Open, actualEvent.OldSupportTask.Status);
                Assert.Equal(SupportTaskStatus.Closed, actualEvent.SupportTask.Status);
                Assert.Equal(createPersonResult.PersonId, actualEvent.PersonId);
                Assert.Equal(ChangeNameRequestSupportTaskApprovedEventChanges.NameChange, actualEvent.Changes);
            }
            else
            {
                var actualEvent = Assert.IsType<ChangeDateOfBirthRequestSupportTaskApprovedEvent>(e);
                Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
                Assert.Equal(SupportTaskStatus.Open, actualEvent.OldSupportTask.Status);
                Assert.Equal(SupportTaskStatus.Closed, actualEvent.SupportTask.Status);
                Assert.Equal(ChangeDateOfBirthRequestSupportTaskApprovedEventChanges.DateOfBirth, actualEvent.Changes);
            }
        },
        e2 =>
        {
            var emailEvent = Assert.IsType<EmailSentEvent>(e2);
        });

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "The request has been accepted");
    }
}

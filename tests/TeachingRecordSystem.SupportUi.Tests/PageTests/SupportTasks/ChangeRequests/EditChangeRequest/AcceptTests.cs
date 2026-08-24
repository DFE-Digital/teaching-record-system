using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ChangeRequests.EditChangeRequest;

public class AcceptTests(HostFixture hostFixture) : TestBase(hostFixture), IAsyncLifetime
{
    async ValueTask IAsyncLifetime.InitializeAsync() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.RecordManager));

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Get_WhenUserHasNoRoles_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: null));
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(person.LastName)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests/{supportTask.SupportTaskReference}/accept");

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
        SetCurrentUser(await TestData.CreateUserAsync(role: role));
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(person.LastName)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests/{supportTask.SupportTaskReference}/accept");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests/{nonExistentSupportTaskReference}/accept");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithSupportTaskReferenceForClosedSupportTask_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(person.LastName)).WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests/{supportTask.SupportTaskReference}/accept");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [RoleNamesData(except: [UserRoles.RecordManager, UserRoles.AccessManager, UserRoles.Administrator])]
    public async Task Post_WhenUserDoesNotHaveSupportOfficerOrAccessManagerOrAdministratorRole_ReturnsForbidden(string role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth!.Value)));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/change-requests/{supportTask.SupportTaskReference}/accept")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_ValidRequest_RedirectsWithFlashMessage(bool isNameChange)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        SupportTask supportTask;
        if (isNameChange)
        {
            supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
                person.PersonId,
                b => b
                    .WithFirstName(TestData.GenerateChangedFirstName(person.FirstName))
                    .WithMiddleName(TestData.GenerateChangedMiddleName(person.MiddleName))
                    .WithLastName(TestData.GenerateChangedLastName(person.LastName)));
        }
        else
        {
            supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                person.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth!.Value)));
        }

        EventObserver.Clear();
        Events.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/change-requests/{supportTask.SupportTaskReference}/accept")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var supportTask = await dbContext.SupportTasks.SingleOrDefaultAsync(t => t.PersonId == person.PersonId);
            Assert.Equal(SupportTaskStatus.Closed, supportTask!.Status);
            Assert.Equal(
                isNameChange ? SupportTaskOutcome.ChangeNameRequest_Approved : SupportTaskOutcome.ChangeDateOfBirthRequest_Approved,
                supportTask.Outcome);

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
                // The name change has been applied by the time we email, so we address them by their new name
                Assert.Equal(
                    requestData.FirstName,
                    email.Personalization[ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey]);

                var updatedPerson = await dbContext.Persons
                    .SingleAsync(p => p.PersonId == person.PersonId);
                Assert.Equal(requestData.FirstName, updatedPerson.FirstName);
                Assert.Equal(requestData.MiddleName, updatedPerson.MiddleName);
                Assert.Equal(requestData.LastName, updatedPerson.LastName);
                Assert.Equal(TimeProvider.UtcNow, updatedPerson.UpdatedOn);

                var previousName = await dbContext.PreviousNames
                    .SingleOrDefaultAsync(pn => pn.PersonId == person.PersonId);
                Assert.NotNull(previousName);
                Assert.Equal(person.FirstName, previousName!.FirstName);
                Assert.Equal(person.MiddleName, previousName.MiddleName);
                Assert.Equal(person.LastName, previousName.LastName);
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
                Assert.Equal(
                    person.FirstName,
                    email.Personalization[ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey]);
            }
        });

        EventObserver.AssertEventsSaved(e => Assert.IsType<LegacyEvents.EmailSentEvent>(e));

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(
                isNameChange ? ProcessType.ChangeOfNameRequestApproving : ProcessType.ChangeOfDateOfBirthRequestApproving,
                p.ProcessContext.ProcessType);

            p.AssertProcessHasEvents<SupportTaskUpdatedEvent, PersonDetailsUpdatedEvent, EmailSentEvent>(
                supportTaskUpdatedEvent =>
                {
                    Assert.Equal(SupportTaskStatus.Open, supportTaskUpdatedEvent.OldSupportTask.Status);
                    Assert.Equal(SupportTaskStatus.Closed, supportTaskUpdatedEvent.SupportTask.Status);
                },
                personDetailsUpdatedEvent =>
                {
                    Assert.Equal(person.PersonId, personDetailsUpdatedEvent.PersonId);
                    Assert.Equal(
                        isNameChange ? PersonDetailsUpdatedEventChanges.NameChange : PersonDetailsUpdatedEventChanges.DateOfBirth,
                        personDetailsUpdatedEvent.Changes);
                },
                emailSentEvent =>
                {
                    Assert.Equal(person.PersonId, emailSentEvent.PersonId);
                    Assert.Equal(
                        isNameChange
                            ? EmailTemplateIds.GetAnIdentityChangeOfNameApprovedEmailConfirmation
                            : EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthApprovedEmailConfirmation,
                        emailSentEvent.Email.TemplateId);
                });
        });

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, "The request has been accepted");
    }
}

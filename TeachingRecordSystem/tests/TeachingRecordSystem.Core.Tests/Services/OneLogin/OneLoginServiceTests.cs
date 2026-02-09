using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.Core.Tests.Services.OneLogin;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public partial class OneLoginServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task SetUserVerifiedAsync_UserIsAlreadyVerified_ThrowsInvalidOperationException()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new SetUserVerifiedOptions
        {
            OneLoginUserSubject = oneLoginUser.Subject,
            VerificationRoute = OneLoginUserVerificationRoute.Support,
            VerifiedNames = [["Test", "User"]],
            VerifiedDatesOfBirth = [new DateOnly(1990, 1, 1)],
            CoreIdentityClaimVc = null
        };

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.SetUserVerifiedAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task SetUserVerifiedAsync_SetsVerifiedPropertiesOnUser()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new SetUserVerifiedOptions
        {
            OneLoginUserSubject = oneLoginUser.Subject,
            VerificationRoute = OneLoginUserVerificationRoute.Support,
            VerifiedNames = [["Test", "User"]],
            VerifiedDatesOfBirth = [new DateOnly(1990, 1, 1)],
            CoreIdentityClaimVc = null
        };

        // Act
        await WithServiceAsync(s => s.SetUserVerifiedAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.Equal(processContext.Now, updatedUser.VerifiedOn);
            Assert.Equal(OneLoginUserVerificationRoute.Support, updatedUser.VerificationRoute);
            Assert.NotNull(updatedUser.VerifiedNames);
            Assert.Collection(
                updatedUser.VerifiedNames,
                names => Assert.Equivalent(new[] { "Test", "User" }, names));
            Assert.NotNull(updatedUser.VerifiedDatesOfBirth);
            Assert.Collection(
                updatedUser.VerifiedDatesOfBirth,
                dob => Assert.Equal(new DateOnly(1990, 1, 1), dob));
        });
    }

    [Fact]
    public async Task SetUserVerifiedAsync_PublishesOneLoginUserUpdatedEvent()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new SetUserVerifiedOptions
        {
            OneLoginUserSubject = oneLoginUser.Subject,
            VerificationRoute = OneLoginUserVerificationRoute.Support,
            VerifiedNames = [["Test", "User"]],
            VerifiedDatesOfBirth = [new DateOnly(1990, 1, 1)],
            CoreIdentityClaimVc = null
        };

        // Act
        await WithServiceAsync(s => s.SetUserVerifiedAsync(options, processContext));

        // Assert
        Events.AssertEventsPublished(e =>
        {
            var updatedEvent = Assert.IsType<OneLoginUserUpdatedEvent>(e);
            Assert.Equal(oneLoginUser.Subject, updatedEvent.OneLoginUser.Subject);
            Assert.NotNull(updatedEvent.OneLoginUser.VerifiedOn);
            Assert.Equal(OneLoginUserVerificationRoute.Support, updatedEvent.OneLoginUser.VerificationRoute);
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.VerifiedOn));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.VerificationRoute));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.VerifiedNames));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.VerifiedDatesOfBirth));
        });
    }

    [Fact]
    public async Task SetUserMatchedAsync_UserIsNotVerified_ThrowsInvalidOperationException()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new SetUserMatchedOptions
        {
            OneLoginUserSubject = oneLoginUser.Subject,
            MatchedPersonId = Guid.NewGuid(),
            MatchRoute = OneLoginUserMatchRoute.SupportUi,
            MatchedAttributes = []
        };

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.SetUserMatchedAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task SetUserMatchedAsync_SetsMatchedPropertiesOnUser()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var matchedPerson = await TestData.CreatePersonAsync();

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new SetUserMatchedOptions
        {
            OneLoginUserSubject = oneLoginUser.Subject,
            MatchedPersonId = matchedPerson.PersonId,
            MatchRoute = OneLoginUserMatchRoute.SupportUi,
            MatchedAttributes = [
                KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName)
            ]
        };

        // Act
        await WithServiceAsync(s => s.SetUserMatchedAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.Equal(options.MatchedPersonId, updatedUser.PersonId);
            Assert.Equal(options.MatchRoute, updatedUser.MatchRoute);
            Assert.NotNull(updatedUser.MatchedAttributes);
            Assert.Collection(
                updatedUser.MatchedAttributes,
                a => Assert.Equal(KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName), a),
                a => Assert.Equal(KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName), a));
        });
    }

    [Fact]
    public async Task SetUserMatchedAsync_PublishesOneLoginUserUpdatedEvent()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var matchedPerson = await TestData.CreatePersonAsync();

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new SetUserMatchedOptions
        {
            OneLoginUserSubject = oneLoginUser.Subject,
            MatchedPersonId = matchedPerson.PersonId,
            MatchRoute = OneLoginUserMatchRoute.SupportUi,
            MatchedAttributes = [
                KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName)
            ]
        };

        // Act
        await WithServiceAsync(s => s.SetUserMatchedAsync(options, processContext));

        // Assert
        Events.AssertEventsPublished(e =>
        {
            var updatedEvent = Assert.IsType<OneLoginUserUpdatedEvent>(e);
            Assert.Equal(oneLoginUser.Subject, updatedEvent.OneLoginUser.Subject);
            Assert.Equal(matchedPerson.PersonId, updatedEvent.OneLoginUser.PersonId);
            Assert.Equal(OneLoginUserMatchRoute.SupportUi, updatedEvent.OneLoginUser.MatchRoute);
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.PersonId));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.MatchedOn));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.MatchRoute));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.MatchedAttributes));
        });
    }

    [Fact]
    public async Task SetUserVerifiedAndMatchedAsync_UserIsAlreadyVerified_ThrowsInvalidOperationException()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var matchedPerson = await TestData.CreatePersonAsync();

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new SetUserVerifiedAndMatchedOptions
        {
            OneLoginUserSubject = oneLoginUser.Subject,
            VerificationRoute = OneLoginUserVerificationRoute.Support,
            VerifiedNames = [["Test", "User"]],
            VerifiedDatesOfBirth = [new DateOnly(1990, 1, 1)],
            CoreIdentityClaimVc = null,
            MatchedPersonId = matchedPerson.PersonId,
            MatchRoute = OneLoginUserMatchRoute.SupportUi,
            MatchedAttributes = []
        };

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.SetUserVerifiedAndMatchedAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task SetUserVerifiedAndMatchedAsync_SetsVerifiedAndMatchedPropertiesOnUser()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var matchedPerson = await TestData.CreatePersonAsync();

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new SetUserVerifiedAndMatchedOptions
        {
            OneLoginUserSubject = oneLoginUser.Subject,
            VerificationRoute = OneLoginUserVerificationRoute.Support,
            VerifiedNames = [["Test", "User"]],
            VerifiedDatesOfBirth = [new DateOnly(1990, 1, 1)],
            CoreIdentityClaimVc = null,
            MatchedPersonId = matchedPerson.PersonId,
            MatchRoute = OneLoginUserMatchRoute.SupportUi,
            MatchedAttributes = [
                KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName)
            ]
        };

        // Act
        await WithServiceAsync(s => s.SetUserVerifiedAndMatchedAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.Equal(processContext.Now, updatedUser.VerifiedOn);
            Assert.Equal(OneLoginUserVerificationRoute.Support, updatedUser.VerificationRoute);
            Assert.NotNull(updatedUser.VerifiedNames);
            Assert.Collection(
                updatedUser.VerifiedNames,
                names => Assert.Equivalent(new[] { "Test", "User" }, names));
            Assert.NotNull(updatedUser.VerifiedDatesOfBirth);
            Assert.Collection(
                updatedUser.VerifiedDatesOfBirth,
                dob => Assert.Equal(new DateOnly(1990, 1, 1), dob));
            Assert.Equal(options.MatchedPersonId, updatedUser.PersonId);
            Assert.Equal(options.MatchRoute, updatedUser.MatchRoute);
            Assert.NotNull(updatedUser.MatchedAttributes);
            Assert.Collection(
                updatedUser.MatchedAttributes,
                a => Assert.Equal(KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName), a),
                a => Assert.Equal(KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName), a));
        });
    }

    [Fact]
    public async Task SetUserVerifiedAndMatchedAsync_PublishesOneLoginUserUpdatedEvent()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var matchedPerson = await TestData.CreatePersonAsync();

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new SetUserVerifiedAndMatchedOptions
        {
            OneLoginUserSubject = oneLoginUser.Subject,
            VerificationRoute = OneLoginUserVerificationRoute.Support,
            VerifiedNames = [["Test", "User"]],
            VerifiedDatesOfBirth = [new DateOnly(1990, 1, 1)],
            CoreIdentityClaimVc = null,
            MatchedPersonId = matchedPerson.PersonId,
            MatchRoute = OneLoginUserMatchRoute.SupportUi,
            MatchedAttributes = [
                KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName)
            ]
        };

        // Act
        await WithServiceAsync(s => s.SetUserVerifiedAndMatchedAsync(options, processContext));

        // Assert
        Events.AssertEventsPublished(e =>
        {
            var updatedEvent = Assert.IsType<OneLoginUserUpdatedEvent>(e);
            Assert.Equal(oneLoginUser.Subject, updatedEvent.OneLoginUser.Subject);
            Assert.NotNull(updatedEvent.OneLoginUser.VerifiedOn);
            Assert.Equal(OneLoginUserVerificationRoute.Support, updatedEvent.OneLoginUser.VerificationRoute);
            Assert.Equal(matchedPerson.PersonId, updatedEvent.OneLoginUser.PersonId);
            Assert.Equal(OneLoginUserMatchRoute.SupportUi, updatedEvent.OneLoginUser.MatchRoute);
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.VerifiedOn));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.VerificationRoute));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.VerifiedNames));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.VerifiedDatesOfBirth));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.PersonId));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.MatchedOn));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.MatchRoute));
            Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.MatchedAttributes));
        });
    }

    [Fact]
    public async Task OnSignInAsync_NewUser_CreatesUserAndPublishesCreatedAndSignedInEvents()
    {
        // Arrange
        var subject = Guid.NewGuid().ToString();
        var email = Faker.Internet.Email();
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync(s => s.OnSignInAsync(subject, email, processContext));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(subject, result.Subject);
        Assert.Equal(email, result.EmailAddress);

        Events.AssertEventsPublished(
            e =>
            {
                var createdEvent = Assert.IsType<OneLoginUserCreatedEvent>(e);
                Assert.Equal(subject, createdEvent.OneLoginUser.Subject);
                Assert.Equal(email, createdEvent.OneLoginUser.EmailAddress);
            },
            e =>
            {
                var signedInEvent = Assert.IsType<OneLoginUserSignedInEvent>(e);
                Assert.Equal(subject, signedInEvent.Subject);
            });
    }

    [Fact]
    public async Task OnSignInAsync_ExistingUserNoChanges_PublishesOnlySignedInEvent()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync(s => s.OnSignInAsync(oneLoginUser.Subject, oneLoginUser.EmailAddress!, processContext));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(oneLoginUser.Subject, result.Subject);
        Assert.Equal(oneLoginUser.EmailAddress, result.EmailAddress);

        Events.AssertEventsPublished(e =>
        {
            var signedInEvent = Assert.IsType<OneLoginUserSignedInEvent>(e);
            Assert.Equal(oneLoginUser.Subject, signedInEvent.Subject);
        });
    }

    [Fact]
    public async Task OnSignInAsync_ExistingUserWithEmailChange_PublishesSignedInAndUpdatedEvents()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var newEmail = Faker.Internet.Email();
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync(s => s.OnSignInAsync(oneLoginUser.Subject, newEmail, processContext));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(oneLoginUser.Subject, result.Subject);
        Assert.Equal(newEmail, result.EmailAddress);

        Events.AssertEventsPublished(
            e =>
            {
                var signedInEvent = Assert.IsType<OneLoginUserSignedInEvent>(e);
                Assert.Equal(oneLoginUser.Subject, signedInEvent.Subject);
            },
            e =>
            {
                var updatedEvent = Assert.IsType<OneLoginUserUpdatedEvent>(e);
                Assert.Equal(oneLoginUser.Subject, updatedEvent.OneLoginUser.Subject);
                Assert.Equal(newEmail, updatedEvent.OneLoginUser.EmailAddress);
                Assert.True(updatedEvent.Changes.HasFlag(OneLoginUserUpdatedEventChanges.EmailAddress));
            });
    }

    private Task WithServiceAsync(Func<OneLoginService, Task> action, params object[] arguments) =>
        WithServiceAsync<OneLoginService>(action, GetServiceDependencies(arguments));

    private Task<TResult> WithServiceAsync<TResult>(Func<OneLoginService, Task<TResult>> action, params object[] arguments) =>
        WithServiceAsync<OneLoginService, TResult>(action, GetServiceDependencies(arguments));

    private object[] GetServiceDependencies(object[] arguments) =>
        [Mock.Of<INotificationSender>(), Mock.Of<IBackgroundJobScheduler>(), .. arguments];
}

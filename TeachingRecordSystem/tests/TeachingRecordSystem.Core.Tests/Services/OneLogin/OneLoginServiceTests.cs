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
            VerifiedDatesOfBirth = [new DateOnly(1990, 1, 1)]
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
            VerifiedDatesOfBirth = [new DateOnly(1990, 1, 1)]
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
    public async Task SetUserMatchedAsync_UserIsNotVerified_ThrowsInvalidOperationException()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new SetUserMatchedOptions
        {
            OneLoginUserSubject = oneLoginUser.Subject,
            MatchedPersonId = Guid.NewGuid(),
            MatchRoute = OneLoginUserMatchRoute.Support,
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
            MatchRoute = OneLoginUserMatchRoute.Support,
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

    private Task WithServiceAsync(Func<OneLoginService, Task> action, params object[] arguments) =>
        WithServiceAsync<OneLoginService>(action, GetServiceDependencies(arguments));

    private Task<TResult> WithServiceAsync<TResult>(Func<OneLoginService, Task<TResult>> action, params object[] arguments) =>
        WithServiceAsync<OneLoginService, TResult>(action, GetServiceDependencies(arguments));

    private object[] GetServiceDependencies(object[] arguments) =>
        [Mock.Of<INotificationSender>(), Mock.Of<IBackgroundJobScheduler>(), .. arguments];
}

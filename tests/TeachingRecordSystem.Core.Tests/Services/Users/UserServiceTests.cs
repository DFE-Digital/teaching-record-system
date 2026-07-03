using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Users;

namespace TeachingRecordSystem.Core.Tests.Services.Users;

public class UserServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task CreateUserAsync_AddsUserToDbAndPublishesEvent()
    {
        // Arrange
        var options = new CreateUserOptions
        {
            Name = TestData.GenerateName(),
            Email = TestData.GenerateUniqueEmail(),
            AzureAdUserId = Guid.NewGuid().ToString(),
            Role = "record_manager"
        };

        var processContext = new ProcessContext(ProcessType.UserAdding, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var user = await WithServiceAsync<UserService, User>(service => service.CreateUserAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbUser = await dbContext.Users.FindAsync(user.UserId);
            Assert.NotNull(dbUser);
            Assert.True(dbUser.Active);
            Assert.Equal(options.Name, dbUser.Name);
            Assert.Equal(options.Email, dbUser.Email);
            Assert.Equal(options.AzureAdUserId, dbUser.AzureAdUserId);
            Assert.Equal(options.Role, dbUser.Role);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.UserAdding, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<UserAddedEvent>(e =>
            {
                Assert.Equal(user.UserId, e.User.UserId);
                Assert.Equal(options.Name, e.User.Name);
            });
        });
    }

    [Fact]
    public async Task UpdateUserAsync_WithChanges_UpdatesUserAndPublishesEvent()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: "record_manager");
        var newName = TestData.GenerateChangedName(user.Name);

        var options = new UpdateUserOptions
        {
            UserId = user.UserId,
            Name = newName,
            Role = "alerts_manager_tra"
        };

        var processContext = new ProcessContext(ProcessType.UserUpdating, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var changes = await WithServiceAsync<UserService, UserUpdatedEventChanges>(service => service.UpdateUserAsync(options, processContext));

        // Assert
        Assert.Equal(UserUpdatedEventChanges.Name | UserUpdatedEventChanges.Roles, changes);

        await WithDbContextAsync(async dbContext =>
        {
            var dbUser = await dbContext.Users.FindAsync(user.UserId);
            Assert.Equal(newName, dbUser!.Name);
            Assert.Equal("alerts_manager_tra", dbUser.Role);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.UserUpdating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<UserUpdatedEvent>(e =>
            {
                Assert.Equal(UserUpdatedEventChanges.Name | UserUpdatedEventChanges.Roles, e.Changes);
                Assert.Equal(newName, e.User.Name);
            });
        });
    }

    [Fact]
    public async Task UpdateUserAsync_WithNoChanges_DoesNotPublishEvent()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: "record_manager");

        var options = new UpdateUserOptions
        {
            UserId = user.UserId,
            Name = user.Name,
            Role = user.Role
        };

        var processContext = new ProcessContext(ProcessType.UserUpdating, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var changes = await WithServiceAsync<UserService, UserUpdatedEventChanges>(service => service.UpdateUserAsync(options, processContext));

        // Assert
        Assert.Equal(UserUpdatedEventChanges.None, changes);
        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task ActivateUserAsync_ActivatesUserAndPublishesEvent()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(active: false);

        var processContext = new ProcessContext(ProcessType.UserActivating, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync<UserService>(service => service.ActivateUserAsync(user.UserId, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbUser = await dbContext.Users.FindAsync(user.UserId);
            Assert.True(dbUser!.Active);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.UserActivating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<UserActivatedEvent>(e => Assert.Equal(user.UserId, e.User.UserId));
        });
    }

    [Fact]
    public async Task DeactivateUserAsync_DeactivatesUserAndPublishesEvent()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var evidenceFileId = Guid.NewGuid();

        var options = new DeactivateUserOptions
        {
            UserId = user.UserId,
            DeactivatedReason = "Left the organisation",
            DeactivatedReasonDetail = "Some more information",
            EvidenceFileId = evidenceFileId
        };

        var processContext = new ProcessContext(ProcessType.UserDeactivating, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync<UserService>(service => service.DeactivateUserAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbUser = await dbContext.Users.FindAsync(user.UserId);
            Assert.False(dbUser!.Active);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.UserDeactivating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<UserDeactivatedEvent>(e =>
            {
                Assert.Equal("Left the organisation", e.DeactivatedReason);
                Assert.Equal("Some more information", e.DeactivatedReasonDetail);
                Assert.Equal(evidenceFileId, e.EvidenceFileId);
            });
        });
    }

    [Fact]
    public async Task CreateApplicationUserAsync_AddsUserToDbAndPublishesEvent()
    {
        // Arrange
        var options = new CreateApplicationUserOptions
        {
            Name = TestData.GenerateApplicationUserName(),
            ShortName = TestData.GenerateApplicationUserShortName()
        };

        var processContext = new ProcessContext(ProcessType.ApplicationUserCreating, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var applicationUser = await WithServiceAsync<UserService, ApplicationUser>(service => service.CreateApplicationUserAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbUser = await dbContext.ApplicationUsers.FindAsync(applicationUser.UserId);
            Assert.NotNull(dbUser);
            Assert.Equal(options.Name, dbUser.Name);
            Assert.Equal(options.ShortName, dbUser.ShortName);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.ApplicationUserCreating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<ApplicationUserCreatedEvent>(e =>
            {
                Assert.Equal(applicationUser.UserId, e.ApplicationUser.UserId);
                Assert.Equal(options.Name, e.ApplicationUser.Name);
            });
        });
    }

    [Fact]
    public async Task UpdateApplicationUserAsync_WithChanges_UpdatesUserAndPublishesEvent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var newName = TestData.GenerateChangedApplicationUserName(applicationUser.Name);

        var options = new UpdateApplicationUserOptions
        {
            UserId = applicationUser.UserId,
            Name = Option.Some(newName)
        };

        var processContext = new ProcessContext(ProcessType.ApplicationUserUpdating, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var changes = await WithServiceAsync<UserService, ApplicationUserUpdatedEventChanges>(service => service.UpdateApplicationUserAsync(options, processContext));

        // Assert
        Assert.Equal(ApplicationUserUpdatedEventChanges.Name, changes);

        await WithDbContextAsync(async dbContext =>
        {
            var dbUser = await dbContext.ApplicationUsers.FindAsync(applicationUser.UserId);
            Assert.Equal(newName, dbUser!.Name);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.ApplicationUserUpdating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<ApplicationUserUpdatedEvent>(e =>
            {
                Assert.Equal(ApplicationUserUpdatedEventChanges.Name, e.Changes);
                Assert.Equal(newName, e.ApplicationUser.Name);
                Assert.Equal(applicationUser.Name, e.OldApplicationUser.Name);
            });
        });
    }

    [Fact]
    public async Task UpdateApplicationUserAsync_WithNoChanges_DoesNotPublishEvent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var options = new UpdateApplicationUserOptions
        {
            UserId = applicationUser.UserId,
            Name = Option.Some(applicationUser.Name)
        };

        var processContext = new ProcessContext(ProcessType.ApplicationUserUpdating, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var changes = await WithServiceAsync<UserService, ApplicationUserUpdatedEventChanges>(service => service.UpdateApplicationUserAsync(options, processContext));

        // Assert
        Assert.Equal(ApplicationUserUpdatedEventChanges.None, changes);
        Events.AssertNoEventsPublished();
    }
}

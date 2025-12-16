using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class SetQtlsTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public async Task HandleAsync_PersonDoesNotExist_ReturnsError()
    {
        // Arrange
        var command = new SetQtlsCommand("0000000", QtsDate: null);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonNotFound);
    }

    [Fact]
    public async Task HandleAsync_NullQtsDateAndNoExistingRoute_DoesNotCreateEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var command = new SetQtlsCommand(person.Trn!, QtsDate: null);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);
        LegacyEventObserver.AssertNoEventsSaved();
    }

    [Fact]
    public async Task HandleAsync_NullQtsDateAndExistingQtlsRoute_DeletesRouteAndSetsQtlsStatusToExpired()
    {
        // Arrange
        var existingQtlsDate = new DateOnly(2025, 4, 1);
        var person = await TestData.CreatePersonAsync(p => p.WithQtls(existingQtlsDate));

        var command = new SetQtlsCommand(person.Trn!, QtsDate: null);

        LegacyEventObserver.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var route = await GetQtlsRoute(person.PersonId);
        Assert.Equal(Clock.UtcNow, route?.DeletedOn);

        var qtlsStatus = await GetQtlsStatus(person.PersonId);
        Assert.Equal(QtlsStatus.Expired, qtlsStatus);

        LegacyEventObserver.AssertEventsSaved(e =>
        {
            var deletedEvent = Assert.IsType<RouteToProfessionalStatusDeletedEvent>(e);
            Assert.Equal(Clock.UtcNow, deletedEvent.CreatedUtc);
            Assert.Equal(person.PersonId, deletedEvent.PersonId);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), deletedEvent.RaisedBy);
        });
    }

    [Fact]
    public async Task HandleAsync_NonNullQtsDateAndNoExistingRoute_CreatesRouteAndSetsQtlsStatusToActive()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var qtlsDate = new DateOnly(2025, 4, 1);
        var command = new SetQtlsCommand(person.Trn!, qtlsDate);

        LegacyEventObserver.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var route = await GetQtlsRoute(person.PersonId);
        Assert.NotNull(route);
        Assert.Equal(qtlsDate, route.HoldsFrom);
        Assert.Equal(RouteToProfessionalStatusStatus.Holds, route.Status);
        Assert.Equal(Clock.UtcNow, route.CreatedOn);

        var qtlsStatus = await GetQtlsStatus(person.PersonId);
        Assert.Equal(QtlsStatus.Active, qtlsStatus);

        LegacyEventObserver.AssertEventsSaved(e =>
        {
            var createdEvent = Assert.IsType<RouteToProfessionalStatusCreatedEvent>(e);
            Assert.Equal(Clock.UtcNow, createdEvent.CreatedUtc);
            Assert.Equal(person.PersonId, createdEvent.PersonId);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), createdEvent.RaisedBy);
        });
    }

    [Fact]
    public async Task HandleAsync_NonNullQtsDateAndExistingRouteHoldsFromMatches_DoesNotCreateEvent()
    {
        // Arrange
        var qtlsDate = new DateOnly(2025, 4, 1);
        var person = await TestData.CreatePersonAsync(p => p.WithQtls(qtlsDate));

        var command = new SetQtlsCommand(person.Trn!, qtlsDate);

        LegacyEventObserver.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        LegacyEventObserver.AssertNoEventsSaved();
    }

    [Fact]
    public async Task HandleAsync_NonNullQtsDateAndExistingRouteHoldFromDoesNotMatch_UpdatesRoute()
    {
        // Arrange
        var existingQtsDate = new DateOnly(2025, 4, 1);
        var person = await TestData.CreatePersonAsync(p => p.WithQtls(existingQtsDate));

        var newQtlsDate = new DateOnly(2025, 4, 10);
        var command = new SetQtlsCommand(person.Trn!, newQtlsDate);

        LegacyEventObserver.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var route = await GetQtlsRoute(person.PersonId);
        Assert.NotNull(route);
        Assert.Equal(newQtlsDate, route.HoldsFrom);
        Assert.Equal(RouteToProfessionalStatusStatus.Holds, route.Status);
        Assert.Equal(Clock.UtcNow, route.UpdatedOn);

        LegacyEventObserver.AssertEventsSaved(e =>
        {
            var updatedEvent = Assert.IsType<RouteToProfessionalStatusUpdatedEvent>(e);
            Assert.Equal(Clock.UtcNow, updatedEvent.CreatedUtc);
            Assert.Equal(person.PersonId, updatedEvent.PersonId);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), updatedEvent.RaisedBy);
            Assert.True(updatedEvent.Changes.HasFlag(RouteToProfessionalStatusUpdatedEventChanges.HoldsFrom));
        });
    }

    private Task<RouteToProfessionalStatus?> GetQtlsRoute(Guid personId) =>
        WithDbContextAsync(dbContext =>
            dbContext.Qualifications
                .OfType<RouteToProfessionalStatus>()
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(q => q.PersonId == personId && q.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId));

    private Task<QtlsStatus> GetQtlsStatus(Guid personId) =>
        WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == personId)
                .Select(p => p.QtlsStatus)
                .SingleAsync());
}

using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class SetQtlsTests : OperationTestBase
{
    [Test]
    public Task HandleAsync_PersonDoesNotExist_ReturnsError() =>
        WithHandler<SetQtlsHandler>(async handler =>
        {
            // Arrange
            var command = new SetQtlsCommand("0000000", QtsDate: null);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertError(result, ApiError.ErrorCodes.PersonNotFound);
        });

    [Test]
    public Task HandleAsync_NullQtsDateAndNoExistingRoute_DoesNotCreateEvent() =>
        WithHandler<SetQtlsHandler>(async handler =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();

            var command = new SetQtlsCommand(person.Trn!, QtsDate: null);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertSuccess(result);
            EventObserver.AssertNoEventsSaved();
        });

    [Test]
    public Task HandleAsync_NullQtsDateAndExistingQtlsRoute_DeletesRouteAndSetsQtlsStatusToExpired() =>
        WithHandler<SetQtlsHandler>(async handler =>
        {
            // Arrange
            var existingQtlsDate = new DateOnly(2025, 4, 1);
            var person = await TestData.CreatePersonAsync(p => p.WithQtls(existingQtlsDate));

            var command = new SetQtlsCommand(person.Trn!, QtsDate: null);

            EventObserver.Clear();

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertSuccess(result);

            var route = await GetQtlsRoute(person.PersonId);
            Assert.Equal(Clock.UtcNow, route?.DeletedOn);

            var qtlsStatus = await GetQtlsStatus(person.PersonId);
            Assert.Equal(QtlsStatus.Expired, qtlsStatus);

            EventObserver.AssertEventsSaved(e =>
            {
                var deletedEvent = Assert.IsType<RouteToProfessionalStatusDeletedEvent>(e);
                Assert.Equal(Clock.UtcNow, deletedEvent.CreatedUtc);
                Assert.Equal(person.PersonId, deletedEvent.PersonId);
                Assert.Equal(CurrentUserProvider.GetCurrentApplicationUser().UserId, deletedEvent.RaisedBy);
            });
        });

    [Test]
    public Task HandleAsync_NonNullQtsDateAndNoExistingRoute_CreatesRouteAndSetsQtlsStatusToActive() =>
        WithHandler<SetQtlsHandler>(async handler =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();

            var qtlsDate = new DateOnly(2025, 4, 1);
            var command = new SetQtlsCommand(person.Trn!, qtlsDate);

            EventObserver.Clear();

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertSuccess(result);

            var route = await GetQtlsRoute(person.PersonId);
            Assert.NotNull(route);
            Assert.Equal(qtlsDate, route.HoldsFrom);
            Assert.Equal(RouteToProfessionalStatusStatus.Holds, route.Status);
            Assert.Equal(Clock.UtcNow, route.CreatedOn);

            var qtlsStatus = await GetQtlsStatus(person.PersonId);
            Assert.Equal(QtlsStatus.Active, qtlsStatus);

            EventObserver.AssertEventsSaved(e =>
            {
                var createdEvent = Assert.IsType<RouteToProfessionalStatusCreatedEvent>(e);
                Assert.Equal(Clock.UtcNow, createdEvent.CreatedUtc);
                Assert.Equal(person.PersonId, createdEvent.PersonId);
                Assert.Equal(CurrentUserProvider.GetCurrentApplicationUser().UserId, createdEvent.RaisedBy);
            });
        });

    [Test]
    public Task HandleAsync_NonNullQtsDateAndExistingRouteHoldsFromMatches_DoesNotCreateEvent() =>
        WithHandler<SetQtlsHandler>(async handler =>
        {
            // Arrange
            var qtlsDate = new DateOnly(2025, 4, 1);
            var person = await TestData.CreatePersonAsync(p => p.WithQtls(qtlsDate));

            var command = new SetQtlsCommand(person.Trn!, qtlsDate);

            EventObserver.Clear();

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertSuccess(result);

            EventObserver.AssertNoEventsSaved();
        });

    [Test]
    public Task HandleAsync_NonNullQtsDateAndExistingRouteHoldFromDoesNotMatch_UpdatesRoute() =>
        WithHandler<SetQtlsHandler>(async handler =>
        {
            // Arrange
            var existingQtsDate = new DateOnly(2025, 4, 1);
            var person = await TestData.CreatePersonAsync(p => p.WithQtls(existingQtsDate));

            var newQtlsDate = new DateOnly(2025, 4, 10);
            var command = new SetQtlsCommand(person.Trn!, newQtlsDate);

            EventObserver.Clear();

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertSuccess(result);

            var route = await GetQtlsRoute(person.PersonId);
            Assert.NotNull(route);
            Assert.Equal(newQtlsDate, route.HoldsFrom);
            Assert.Equal(RouteToProfessionalStatusStatus.Holds, route.Status);
            Assert.Equal(Clock.UtcNow, route.UpdatedOn);

            EventObserver.AssertEventsSaved(e =>
            {
                var updatedEvent = Assert.IsType<RouteToProfessionalStatusUpdatedEvent>(e);
                Assert.Equal(Clock.UtcNow, updatedEvent.CreatedUtc);
                Assert.Equal(person.PersonId, updatedEvent.PersonId);
                Assert.Equal(CurrentUserProvider.GetCurrentApplicationUser().UserId, updatedEvent.RaisedBy);
                Assert.True(updatedEvent.Changes.HasFlag(RouteToProfessionalStatusUpdatedEventChanges.HoldsFrom));
            });
        });

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

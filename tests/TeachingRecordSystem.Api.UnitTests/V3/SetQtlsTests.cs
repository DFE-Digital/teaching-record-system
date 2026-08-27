using TeachingRecordSystem.Api.V3.Operations;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class SetQtlsTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    private static readonly DateOnly QtsCutoff = new(2012, 4, 1);

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

        var command = new SetQtlsCommand(person.Trn, QtsDate: null);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);
        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task HandleAsync_NullQtsDateAndExistingQtlsRoute_DeletesRouteAndSetsQtlsStatusToExpired()
    {
        // Arrange
        var existingQtlsDate = new DateOnly(2025, 4, 1);
        var person = await TestData.CreatePersonAsync(p => p.WithQtls(existingQtlsDate));

        var command = new SetQtlsCommand(person.Trn, QtsDate: null);

        Events.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var route = await GetQtlsRoute(person.PersonId);
        Assert.Equal(TimeProvider.UtcNow, route?.DeletedOn);

        var qtlsStatus = await GetQtlsStatus(person.PersonId);
        Assert.Equal(QtlsStatus.Expired, qtlsStatus);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusDeleting, p.ProcessContext.ProcessType);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), p.ProcessContext.UserId);

            p.AssertProcessHasEvents<RouteToProfessionalStatusDeletedEvent>(
                deletedEvent => Assert.Equal(person.PersonId, deletedEvent.PersonId));
        });
    }

    [Fact]
    public async Task HandleAsync_NonNullQtsDateAndNoExistingRoute_CreatesRouteAndSetsQtlsStatusToActive()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var qtlsDate = new DateOnly(2025, 4, 1);
        var command = new SetQtlsCommand(person.Trn, qtlsDate);

        Events.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var route = await GetQtlsRoute(person.PersonId);
        Assert.NotNull(route);
        Assert.Equal(qtlsDate, route.HoldsFrom);
        Assert.Equal(RouteToProfessionalStatusStatus.Holds, route.Status);
        Assert.Equal(TimeProvider.UtcNow, route.CreatedOn);

        var qtlsStatus = await GetQtlsStatus(person.PersonId);
        Assert.Equal(QtlsStatus.Active, qtlsStatus);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusCreating, p.ProcessContext.ProcessType);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), p.ProcessContext.UserId);

            p.AssertProcessHasEvents<RouteToProfessionalStatusCreatedEvent>(
                createdEvent => Assert.Equal(person.PersonId, createdEvent.PersonId));
        });
    }

    [Fact]
    public async Task HandleAsync_NonNullQtsDateAndExistingRouteHoldsFromMatches_DoesNotCreateEvent()
    {
        // Arrange
        var qtlsDate = new DateOnly(2025, 4, 1);
        var person = await TestData.CreatePersonAsync(p => p.WithQtls(qtlsDate));

        var command = new SetQtlsCommand(person.Trn, qtlsDate);

        Events.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task HandleAsync_newQtlsDateNonNullQtsDateAndExistingRouteHoldFromDoesNotMatch_UpdatesRoute()
    {
        // Arrange
        var existingQtsDate = new DateOnly(2025, 4, 1);
        var person = await TestData.CreatePersonAsync(p => p.WithQtls(existingQtsDate));

        var newQtlsDate = new DateOnly(2025, 4, 10);
        var command = new SetQtlsCommand(person.Trn, newQtlsDate);

        Events.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var route = await GetQtlsRoute(person.PersonId);
        Assert.NotNull(route);
        Assert.Equal(newQtlsDate, route.HoldsFrom);
        Assert.Equal(RouteToProfessionalStatusStatus.Holds, route.Status);
        Assert.Equal(TimeProvider.UtcNow, route.UpdatedOn);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusUpdating, p.ProcessContext.ProcessType);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), p.ProcessContext.UserId);

            p.AssertProcessHasEvents<RouteToProfessionalStatusUpdatedEvent>(
                updatedEvent =>
                {
                    Assert.Equal(person.PersonId, updatedEvent.PersonId);
                    Assert.True(updatedEvent.Changes.HasFlag(RouteToProfessionalStatusUpdatedEventChanges.HoldsFrom));
                });
        });
    }

    [Fact]
    public async Task HandleAsync_newQtlsDateBeforeCuttoffUpdatesToCuttoff()
    {
        // Arrange
        var existingQtsDate = new DateOnly(2025, 4, 1);
        var person = await TestData.CreatePersonAsync(p => p.WithQtls(existingQtsDate));

        var newQtlsDate = QtsCutoff.AddYears(-1);
        var command = new SetQtlsCommand(person.Trn, newQtlsDate);

        Events.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var route = await GetQtlsRoute(person.PersonId);
        Assert.NotNull(route);
        Assert.Equal(QtsCutoff, route.HoldsFrom);
        Assert.Equal(RouteToProfessionalStatusStatus.Holds, route.Status);
        Assert.Equal(TimeProvider.UtcNow, route.UpdatedOn);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusUpdating, p.ProcessContext.ProcessType);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), p.ProcessContext.UserId);

            p.AssertProcessHasEvents<RouteToProfessionalStatusUpdatedEvent>(
                updatedEvent =>
                {
                    Assert.Equal(person.PersonId, updatedEvent.PersonId);
                    Assert.True(updatedEvent.Changes.HasFlag(RouteToProfessionalStatusUpdatedEventChanges.HoldsFrom));
                });
        });
    }

    [Fact]
    public async Task HandleAsync_newQtlsDateAfterCuttoffUpdatesToNewQtlsDate()
    {
        // Arrange
        var existingQtsDate = new DateOnly(2025, 4, 1);
        var person = await TestData.CreatePersonAsync(p => p.WithQtls(existingQtsDate));

        var newQtlsDate = QtsCutoff.AddYears(1);
        var command = new SetQtlsCommand(person.Trn, newQtlsDate);

        Events.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var route = await GetQtlsRoute(person.PersonId);
        Assert.NotNull(route);
        Assert.Equal(newQtlsDate, route.HoldsFrom);
        Assert.Equal(RouteToProfessionalStatusStatus.Holds, route.Status);
        Assert.Equal(TimeProvider.UtcNow, route.UpdatedOn);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusUpdating, p.ProcessContext.ProcessType);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), p.ProcessContext.UserId);

            p.AssertProcessHasEvents<RouteToProfessionalStatusUpdatedEvent>(
                updatedEvent =>
                {
                    Assert.Equal(person.PersonId, updatedEvent.PersonId);
                    Assert.True(updatedEvent.Changes.HasFlag(RouteToProfessionalStatusUpdatedEventChanges.HoldsFrom));
                });
        });
    }

    [Fact]
    public async Task HandleAsync_newQtls_SetsExistingRouteHoldsDateToCuttoff()
    {
        // Arrange
        var existingQtsDate = new DateOnly(2025, 4, 1);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(s => s
                .WithRouteType(RouteToProfessionalStatusType.AssessmentOnlyRouteId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(existingQtsDate)));

        var newQtlsDate = QtsCutoff.AddYears(-1);
        var command = new SetQtlsCommand(person.Trn, newQtlsDate);

        Events.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);
        var route = await GetQtlsRoute(person.PersonId);
        Assert.NotNull(route);
        Assert.Equal(QtsCutoff, route.HoldsFrom);
        Assert.Equal(RouteToProfessionalStatusStatus.Holds, route.Status);
        Assert.Equal(TimeProvider.UtcNow, route.UpdatedOn);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusCreating, p.ProcessContext.ProcessType);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), p.ProcessContext.UserId);

            p.AssertProcessHasEvents<RouteToProfessionalStatusCreatedEvent>(
                createdQtlsEventEvent =>
                {
                    Assert.Equal(RouteToProfessionalStatusType.QtlsAndSetMembershipId, createdQtlsEventEvent.RouteToProfessionalStatus.RouteToProfessionalStatusTypeId);
                    Assert.Equal(person.PersonId, createdQtlsEventEvent.PersonId);
                    Assert.True(createdQtlsEventEvent.Changes.HasFlag(RouteToProfessionalStatusCreatedEventChanges.PersonQtsDate));
                });
        });
    }

    [Fact]
    public async Task HandleAsync_existingQtlsAndQts_SetsExistingRouteHoldsDateToCuttoff()
    {
        // Arrange
        var existingQtsDate = new DateOnly(2025, 4, 1);
        var existingQtlsDate = new DateOnly(2025, 11, 1);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(s => s
                .WithRouteType(RouteToProfessionalStatusType.AssessmentOnlyRouteId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(existingQtsDate))
            .WithRouteToProfessionalStatus(s => s
                .WithRouteType(RouteToProfessionalStatusType.QtlsAndSetMembershipId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(existingQtlsDate)));

        var newQtlsDate = QtsCutoff.AddYears(-1);
        var command = new SetQtlsCommand(person.Trn, newQtlsDate);

        Events.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var route = await GetQtlsRoute(person.PersonId);
        Assert.NotNull(route);
        Assert.Equal(QtsCutoff, route.HoldsFrom);
        Assert.Equal(RouteToProfessionalStatusStatus.Holds, route.Status);
        Assert.Equal(TimeProvider.UtcNow, route.UpdatedOn);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusUpdating, p.ProcessContext.ProcessType);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), p.ProcessContext.UserId);

            p.AssertProcessHasEvents<RouteToProfessionalStatusUpdatedEvent>(
                updatedQtlsEvent =>
                {
                    Assert.Equal(RouteToProfessionalStatusType.QtlsAndSetMembershipId, updatedQtlsEvent.RouteToProfessionalStatus.RouteToProfessionalStatusTypeId);
                    Assert.Equal(person.PersonId, updatedQtlsEvent.PersonId);
                    Assert.True(updatedQtlsEvent.Changes.HasFlag(RouteToProfessionalStatusUpdatedEventChanges.PersonQtsDate));
                });
        });
    }

    [Fact]
    public async Task HandleAsync_existingQtls_SetsExistingRouteHoldsDateToCuttoff()
    {
        // Arrange
        var existingQtlsDate = new DateOnly(2025, 11, 1);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(s => s
                .WithRouteType(RouteToProfessionalStatusType.QtlsAndSetMembershipId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(existingQtlsDate)));

        var newQtlsDate = QtsCutoff.AddYears(-1);
        var command = new SetQtlsCommand(person.Trn, newQtlsDate);

        Events.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var route = await GetQtlsRoute(person.PersonId);
        Assert.NotNull(route);
        Assert.Equal(QtsCutoff, route.HoldsFrom);
        Assert.Equal(RouteToProfessionalStatusStatus.Holds, route.Status);
        Assert.Equal(TimeProvider.UtcNow, route.UpdatedOn);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusUpdating, p.ProcessContext.ProcessType);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), p.ProcessContext.UserId);

            p.AssertProcessHasEvents<RouteToProfessionalStatusUpdatedEvent>(
                updatedQtlsEvent =>
                {
                    Assert.Equal(RouteToProfessionalStatusType.QtlsAndSetMembershipId, updatedQtlsEvent.RouteToProfessionalStatus.RouteToProfessionalStatusTypeId);
                    Assert.Equal(person.PersonId, updatedQtlsEvent.PersonId);
                    Assert.True(updatedQtlsEvent.Changes.HasFlag(RouteToProfessionalStatusUpdatedEventChanges.PersonQtsDate));
                });
        });
    }

    [Fact]
    public async Task HandleAsync_newQtls_WithExistingQtsWithHoldsDateBeforeCuttOffDoesNotUpdate()
    {
        // Arrange
        var existingQtsDate = new DateOnly(2001, 4, 1);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(s => s
                .WithRouteType(RouteToProfessionalStatusType.AssessmentOnlyRouteId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(existingQtsDate)));

        var newQtlsDate = QtsCutoff.AddYears(-1);
        var command = new SetQtlsCommand(person.Trn, newQtlsDate);

        Events.Clear();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var aorRoute = await GetRoute(person.PersonId, RouteToProfessionalStatusType.AssessmentOnlyRouteId);
        var route = await GetQtlsRoute(person.PersonId);
        Assert.NotNull(aorRoute);
        Assert.Equal(existingQtsDate, aorRoute.HoldsFrom);
        Assert.Equal(RouteToProfessionalStatusStatus.Holds, aorRoute.Status);

        Assert.NotNull(route);
        Assert.Equal(QtsCutoff, route.HoldsFrom);
        Assert.Equal(RouteToProfessionalStatusStatus.Holds, route.Status);
        Assert.Equal(TimeProvider.UtcNow, route.UpdatedOn);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusCreating, p.ProcessContext.ProcessType);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), p.ProcessContext.UserId);

            p.AssertProcessHasEvents<RouteToProfessionalStatusCreatedEvent>(
                createdQtlsEventEvent =>
                {
                    Assert.Equal(
                        RouteToProfessionalStatusType.QtlsAndSetMembershipId,
                        createdQtlsEventEvent.RouteToProfessionalStatus.RouteToProfessionalStatusTypeId);
                    Assert.Equal(person.PersonId, createdQtlsEventEvent.PersonId);
                    Assert.False(
                        createdQtlsEventEvent.Changes.HasFlag(RouteToProfessionalStatusCreatedEventChanges.PersonQtsDate));
                });
        });
    }

    private Task<RouteToProfessionalStatus?> GetRoute(Guid personId, Guid routeToProfessionalStatusTypeId) =>
        WithDbContextAsync(dbContext =>
            dbContext.Qualifications
                .OfType<RouteToProfessionalStatus>()
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(q => q.PersonId == personId && q.RouteToProfessionalStatusTypeId == routeToProfessionalStatusTypeId));

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

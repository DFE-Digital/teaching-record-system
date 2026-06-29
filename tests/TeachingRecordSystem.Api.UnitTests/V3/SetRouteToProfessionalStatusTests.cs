using TeachingRecordSystem.Api.V3.Operations;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class SetRouteToProfessionalStatusTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public async Task HandleAsync_PersonDoesNotExist_ReturnsError()
    {
        // Arrange
        var command = CreateCommand("0000000", RouteToProfessionalStatusType.AssessmentOnlyRouteId, "ref-1");

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonNotFound);
    }

    [Fact]
    public async Task HandleAsync_RouteTypeIsNotPermitted_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var unknownRouteTypeId = new Guid("00000000-0000-0000-0000-000000000001");
        var command = CreateCommand(person.Trn, unknownRouteTypeId, "ref-1");

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.InvalidRouteType);
    }

    [Fact]
    public async Task HandleAsync_SubjectDoesNotExist_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var command = CreateCommand(person.Trn, RouteToProfessionalStatusType.AssessmentOnlyRouteId, "ref-1") with
        {
            TrainingSubjectReferences = ["INVALID-SUBJECT-REF"]
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.InvalidTrainingSubjectReference);
    }

    [Fact]
    public async Task HandleAsync_CountryDoesNotExist_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var command = CreateCommand(person.Trn, RouteToProfessionalStatusType.AssessmentOnlyRouteId, "ref-1") with
        {
            TrainingCountryReference = "INVALID-COUNTRY"
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.InvalidTrainingCountryReference);
    }

    [Fact]
    public async Task HandleAsync_TrainingProviderDoesNotExist_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var command = CreateCommand(person.Trn, RouteToProfessionalStatusType.AssessmentOnlyRouteId, "ref-1") with
        {
            TrainingProviderUkprn = "99999999999"
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.InvalidTrainingProviderUkprn);
    }

    [Fact]
    public async Task HandleAsync_DegreeTypeDoesNotExist_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var command = CreateCommand(person.Trn, RouteToProfessionalStatusType.AssessmentOnlyRouteId, "ref-1") with
        {
            DegreeTypeId = new Guid("00000000-0000-0000-0000-000000000002")
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.InvalidDegreeType);
    }

    [Fact]
    public async Task HandleAsync_RouteAlreadyExistsAndIsOverseas_ReturnsError()
    {
        // Arrange
        var sourceRef = "ref-1";
        var person = await TestData.CreatePersonAsync();

        await CreateExistingRouteAsync(person.PersonId, RouteToProfessionalStatusType.AssessmentOnlyRouteId, sourceRef);

        var command = CreateCommand(person.Trn, RouteToProfessionalStatusType.ApplyForQtsId, sourceRef);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.UpdatesNotAllowedForRouteType);
    }

    [Fact]
    public async Task HandleAsync_RouteAlreadyExistsAndNewRouteTypeIsDifferentProfessionalStatusType_ReturnsError()
    {
        // Arrange
        var sourceRef = "ref-1";
        var person = await TestData.CreatePersonAsync();

        await CreateExistingRouteAsync(person.PersonId, RouteToProfessionalStatusType.AssessmentOnlyRouteId, sourceRef);

        // Early years route type has different ProfessionalStatusType than Assessment Only (QTS)
        var earlyYearsRouteTypeId = new Guid("D9EEF3F8-FDE6-4A3F-A361-F6655A42FA1E"); // Early Years ITT Assessment Only
        var command = CreateCommand(person.Trn, earlyYearsRouteTypeId, sourceRef);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.UnableToChangeRouteType);
    }

    [Fact]
    public async Task HandleAsync_RouteAlreadyExistsAndIsAwarded_ReturnsError()
    {
        // Arrange
        var sourceRef = "ref-1";
        var person = await TestData.CreatePersonAsync();

        await CreateExistingRouteAsync(
            person.PersonId,
            RouteToProfessionalStatusType.AssessmentOnlyRouteId,
            sourceRef,
            status: RouteToProfessionalStatusStatus.Holds,
            holdsFrom: new DateOnly(2024, 6, 1));

        var command = CreateCommand(person.Trn, RouteToProfessionalStatusType.AssessmentOnlyRouteId, sourceRef);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.RouteToProfessionalStatusAlreadyAwarded);
    }

    [Fact]
    public async Task HandleAsync_RouteAlreadyExistsAndIsFailedAndNewStatusIsInTraining_ReturnsError()
    {
        // Arrange
        var sourceRef = "ref-1";
        var person = await TestData.CreatePersonAsync();

        await CreateExistingRouteAsync(
            person.PersonId,
            RouteToProfessionalStatusType.AssessmentOnlyRouteId,
            sourceRef,
            status: RouteToProfessionalStatusStatus.Failed);

        var command = CreateCommand(person.Trn, RouteToProfessionalStatusType.AssessmentOnlyRouteId, sourceRef) with
        {
            Status = RouteToProfessionalStatusStatus.InTraining
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.UnableToChangeFailProfessionalStatusStatus);
    }

    [Fact]
    public async Task HandleAsync_RouteAlreadyExistsAndIsWithdrawnAndNewStatusIsDeferred_ReturnsError()
    {
        // Arrange
        var sourceRef = "ref-1";
        var person = await TestData.CreatePersonAsync();

        await CreateExistingRouteAsync(
            person.PersonId,
            RouteToProfessionalStatusType.AssessmentOnlyRouteId,
            sourceRef,
            status: RouteToProfessionalStatusStatus.Withdrawn);

        var command = CreateCommand(person.Trn, RouteToProfessionalStatusType.AssessmentOnlyRouteId, sourceRef) with
        {
            Status = RouteToProfessionalStatusStatus.Deferred
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.UnableToChangeWithdrawnProfessionalStatusStatus);
    }

    [Fact]
    public async Task HandleAsync_ValidRequestForRouteThatAlreadyExists_UpdatesRouteAndReturnsSuccess()
    {
        // Arrange
        var sourceRef = "ref-1";
        var person = await TestData.CreatePersonAsync();

        await CreateExistingRouteAsync(
            person.PersonId,
            RouteToProfessionalStatusType.AssessmentOnlyRouteId,
            sourceRef,
            status: RouteToProfessionalStatusStatus.InTraining);

        var command = CreateCommand(person.Trn, RouteToProfessionalStatusType.AssessmentOnlyRouteId, sourceRef) with
        {
            Status = RouteToProfessionalStatusStatus.UnderAssessment
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);
    }

    [Fact]
    public async Task HandleAsync_ValidRequestForRouteThatDoesNotExist_CreatesRouteAndReturnsSuccess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var command = CreateCommand(person.Trn, RouteToProfessionalStatusType.AssessmentOnlyRouteId, "ref-1");

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var route = await WithDbContextAsync(dbContext =>
            dbContext.RouteToProfessionalStatuses
                .SingleOrDefaultAsync(r =>
                    r.PersonId == person.PersonId &&
                    r.SourceApplicationReference == command.SourceApplicationReference));

        Assert.NotNull(route);
        Assert.Equal(RouteToProfessionalStatusType.AssessmentOnlyRouteId, route.RouteToProfessionalStatusTypeId);
        Assert.Equal(command.Status, route.Status);
    }

    private static SetRouteToProfessionalStatusCommand CreateCommand(
        string trn,
        Guid routeTypeId,
        string sourceRef) =>
        new SetRouteToProfessionalStatusCommand(
            Trn: trn,
            SourceApplicationReference: sourceRef,
            RouteToProfessionalStatusTypeId: routeTypeId,
            Status: RouteToProfessionalStatusStatus.InTraining,
            HoldsFrom: null,
            TrainingStartDate: null,
            TrainingEndDate: null,
            TrainingSubjectReferences: [],
            TrainingAgeSpecialism: null,
            TrainingCountryReference: null,
            TrainingProviderUkprn: null,
            DegreeTypeId: null,
            IsExemptFromInduction: null);

    private async Task CreateExistingRouteAsync(
        Guid personId,
        Guid routeTypeId,
        string sourceRef,
        RouteToProfessionalStatusStatus status = RouteToProfessionalStatusStatus.InTraining,
        DateOnly? holdsFrom = null)
    {
        var currentUserId = CurrentUserProvider.GetCurrentApplicationUserId();
        var allRouteTypes = await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync();

        await WithDbContextAsync(async dbContext =>
        {
            var person = await dbContext.Persons
                .Include(p => p.Qualifications)
                .SingleAsync(p => p.PersonId == personId);

            var route = RouteToProfessionalStatus.Create(
                person,
                allRouteTypes,
                routeTypeId,
                sourceApplicationUserId: currentUserId,
                sourceApplicationReference: sourceRef,
                status: status,
                holdsFrom: holdsFrom,
                trainingStartDate: null,
                trainingEndDate: null,
                trainingSubjectIds: [],
                trainingAgeSpecialismType: null,
                trainingAgeSpecialismRangeFrom: null,
                trainingAgeSpecialismRangeTo: null,
                trainingCountryId: null,
                trainingProviderId: null,
                degreeTypeId: null,
                isExemptFromInduction: null,
                createdBy: SystemUser.SystemUserId,
                now: TimeProvider.UtcNow,
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                additionalInformation: null,
                @event: out var createdEvent);

            dbContext.RouteToProfessionalStatuses.Add(route);
            dbContext.AddEventWithoutBroadcast(createdEvent);
            await dbContext.SaveChangesAsync();
        });
    }
}

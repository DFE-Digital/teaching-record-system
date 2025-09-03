using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class SetRouteToProfessionalStatusTests : OperationTestBase
{
    //HandleAsync_PersonDoesNotExist_ReturnsError
    //HandleAsync_RouteTypeIsNotPermitted_ReturnsError
    //HandleAsync_SubjectDoesNotExist_ReturnsError
    //HandleAsync_CountryDoesNotExist_ReturnsError
    //HandleAsync_TrainingProviderDoesNotExist_ReturnsError
    //HandleAsync_DegreeTypeDoesNotExist_ReturnsError
    //HandleAsync_RouteAlreadyExistsAndIsOverseas_ReturnsError
    //HandleAsync_RouteAlreadyExistsAndNewRouteTypeIsDifferentProfessionalStatusType_ReturnsError
    //HandleAsync_RouteAlreadyExistsAndIsAwarded_ReturnsError
    //HandleAsync_RouteAlreadyExistsAndIsFailed_ReturnsError
    //HandleAsync_RouteAlreadyExistsAndIsWithdrawn_ReturnsError
    //HandleAsync_ValidRequestForRouteThatAlreadyExists_UpdatesRouteAndReturnsSuccess
    //HandleAsync_ValidRequestForRouteThatDoesNotExist_CreatesRouteAndReturnsSuccess

    private static SetRouteToProfessionalStatusCommand CreateCommand(Action<SetRouteToProfessionalStatusCommand>? configure = null)
    {
        throw new NotImplementedException();
    }
}

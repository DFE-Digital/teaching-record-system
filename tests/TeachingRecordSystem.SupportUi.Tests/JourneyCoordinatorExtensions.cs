using GovUk.Questions.AspNetCore;
using JourneyInstanceId = GovUk.Questions.AspNetCore.JourneyInstanceId;

namespace TeachingRecordSystem.SupportUi.Tests;

public static class JourneyCoordinatorExtensions
{
    public static string GetUniqueIdQueryParameter(this JourneyCoordinator coordinator) =>
        $"{JourneyInstanceId.KeyRouteValueName}={Uri.EscapeDataString(coordinator.InstanceId.Key)}";
}

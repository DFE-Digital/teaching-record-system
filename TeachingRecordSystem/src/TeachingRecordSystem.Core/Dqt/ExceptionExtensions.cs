using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace TeachingRecordSystem.Core.Dqt;

public static class ExceptionExtensions
{
    public static bool IsCrmRateLimitException(this Exception exception, out TimeSpan retryAfter)
    {
        if (exception is FaultException<OrganizationServiceFault> fault &&
            fault.Detail.ErrorDetails.TryGetValue("Retry-After", out var retryAfterObj) &&
            retryAfterObj is TimeSpan retryAfterTs)
        {
            retryAfter = retryAfterTs;
            return true;
        }

        retryAfter = default;
        return false;
    }
}

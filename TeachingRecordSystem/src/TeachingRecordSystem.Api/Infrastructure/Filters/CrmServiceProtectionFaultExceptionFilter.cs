using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Xrm.Sdk;

namespace TeachingRecordSystem.Api.Infrastructure.Filters;

public class CrmServiceProtectionFaultExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is FaultException<OrganizationServiceFault> fault)
        {
            // https://docs.microsoft.com/en-us/powerapps/developer/data-platform/api-limits#service-protection-api-limit-errors-returned

            switch (fault.Detail.ErrorCode)
            {
                case -2147015902:  // Number of requests
                case -2147015903:  // Execution time
                case -2147015898:  // Concurrent requests
                    context.Result = new StatusCodeResult(StatusCodes.Status429TooManyRequests);
                    context.ExceptionHandled = true;

                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger<CrmServiceProtectionFaultExceptionFilter>();
                    logger.LogWarning("Hit CRM service limits; error code: {ErrorCode}", fault.Detail.ErrorCode);

                    break;
            }
        }
    }
}

#nullable disable
using System.ServiceModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace QualifiedTeachersApi.Tests.Filters;

public class CrmServiceProtectionFaultExceptionFilterTests : ApiTestBase
{
    public CrmServiceProtectionFaultExceptionFilterTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Theory]
    [InlineData("number_of_requests")]
    [InlineData("execution_time")]
    [InlineData("concurrent_requests")]
    public async Task Given_a_service_protection_fault_is_thrown_return_a_429_response(string testEndpoint)
    {
        var response = await HttpClientWithApiKey.GetAsync($"CrmServiceProtectionFaultExceptionFilterTests/{testEndpoint}");

        Assert.Equal(StatusCodes.Status429TooManyRequests, (int)response.StatusCode);
    }
}

[Route("CrmServiceProtectionFaultExceptionFilterTests")]
public class CrmServiceProtectionFaultExceptionFilterTestsController : Controller
{
    [HttpGet("number_of_requests")]
    public IActionResult ThrowsNumberOfRequestsError()
    {
        throw new FaultException<OrganizationServiceFault>(new OrganizationServiceFault()
        {
            ErrorCode = -2147015902
        });
    }

    [HttpGet("execution_time")]
    public IActionResult ThrowsExecutionTimeError()
    {
        throw new FaultException<OrganizationServiceFault>(new OrganizationServiceFault()
        {
            ErrorCode = -2147015903
        });
    }

    [HttpGet("concurrent_requests")]
    public IActionResult ConcurrentRequests()
    {
        throw new FaultException<OrganizationServiceFault>(new OrganizationServiceFault()
        {
            ErrorCode = -2147015898
        });
    }
}

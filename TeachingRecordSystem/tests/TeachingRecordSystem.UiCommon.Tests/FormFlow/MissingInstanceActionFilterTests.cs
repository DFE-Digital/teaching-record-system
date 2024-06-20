using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.UiCommon.Tests.FormFlow.Infrastructure;

namespace TeachingRecordSystem.UiCommon.FormFlow.Tests;

public class MissingInstanceActionFilterTests : MvcTestBase
{
    public MissingInstanceActionFilterTests(MvcTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task RequireJourneyInstanceSpecifiedButNoActiveInstance_ReturnsNotFound()
    {
        // Arrange
        var id = 42;
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"MissingInstanceActionFilterTests/{id}/withattribute");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RequireJourneyInstanceSpecifiedWithOverridenStatusCodeButNoActiveInstance_ReturnsStatusCode()
    {
        // Arrange
        var id = 42;
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"MissingInstanceActionFilterTests/{id}/withattributeandoverridenstatuscode");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(400, (int)response.StatusCode);
    }

    [Fact]
    public async Task RequireJourneyInstanceSpecifiedWithActiveInstance_ReturnsOk()
    {
        // Arrange
        await CreateInstanceAsync(
            journeyName: "MissingInstanceActionFilterTests",
            keys: new Dictionary<string, StringValues>()
            {
                { "id", "42" }
            },
            state: new MissingInstanceActionFilterTestsState());

        var id = 42;
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"MissingInstanceActionFilterTests/{id}/withattribute");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RequireJourneyInstanceSpecifiedButNoMetadata_Throws()
    {
        // Arrange
        var id = 42;
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"MissingInstanceActionFilterTests/{id}/withoutmetadata");

        // Act
        var ex = await Record.ExceptionAsync(() => HttpClient.SendAsync(request));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("No journey metadata found on action.", ex.Message);
    }
}

[Route("MissingInstanceActionFilterTests/{id}")]
public class MissingInstanceActionFilterTestsController : Controller
{
    [Journey("MissingInstanceActionFilterTests")]
    [RequireJourneyInstance]
    [HttpGet("withattribute")]
    public IActionResult WithAttribute() => Ok();

    [RequireJourneyInstance]
    [HttpGet("withoutmetadata")]
    public IActionResult WithoutMetadata() => Ok();

    [Journey("MissingInstanceActionFilterTests")]
    [RequireJourneyInstance(errorStatusCode: 400)]
    [HttpGet("withattributeandoverridenstatuscode")]
    public IActionResult WithAttributeAndOverridenStatusCode() => Ok();
}

public class MissingInstanceActionFilterTestsState { }

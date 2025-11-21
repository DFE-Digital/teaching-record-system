using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.WebCommon.FormFlow;
using TeachingRecordSystem.WebCommon.Tests.FormFlow.Infrastructure;

namespace TeachingRecordSystem.WebCommon.Tests.FormFlow;

public class EndToEndTests : MvcTestBase
{
    public EndToEndTests(MvcTestFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task ReadState_ReturnsState()
    {
        // Arrange
        var instance = await StateProvider.CreateInstanceAsync(
            instanceId: GenerateInstanceId(out var id, out var subid),
            stateType: typeof(E2ETestsState),
            state: E2ETestsState.CreateInitialState());

        // Act & Assert
        var responseJson = await ReadStateAndAssertAsync(instance.InstanceId, expectedValue: "initial");
    }

    [Fact]
    public async Task UpdateState_UpdatesStateAndRedirects()
    {
        // Arrange
        var instance = await StateProvider.CreateInstanceAsync(
            instanceId: GenerateInstanceId(out var id, out var subid),
            stateType: typeof(E2ETestsState),
            state: E2ETestsState.CreateInitialState());

        // Act & Assert
        await UpdateStateAsync(instance.InstanceId, newValue: "updated");
    }

    [Fact]
    public async Task Complete_DoesNotAllowStateToBeUpdatedSubsequently()
    {
        // Arrange
        var instance = await StateProvider.CreateInstanceAsync(
            instanceId: GenerateInstanceId(out var id, out var subid),
            stateType: typeof(E2ETestsState),
            state: E2ETestsState.CreateInitialState());

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/E2ETests/{id}/{subid}/Complete?ffiid={instance.InstanceId.UniqueKey}")
        {
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await Assert.ThrowsAnyAsync<Exception>(() => UpdateStateAsync(instance.InstanceId, newValue: "anything"));
    }

    [Fact]
    public async Task Complete_DoesAllowStateToBeReadSubsequently()
    {
        // Arrange
        var instance = await StateProvider.CreateInstanceAsync(
            instanceId: GenerateInstanceId(out var id, out var subid),
            stateType: typeof(E2ETestsState),
            state: E2ETestsState.CreateInitialState());

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/E2ETests/{id}/{subid}/Complete?ffiid={instance.InstanceId.UniqueKey}")
        {
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await ReadStateAndAssertAsync(instance.InstanceId, expectedValue: "initial");
    }

    [Fact]
    public async Task Delete_ReturnsOk()
    {
        // Arrange
        var instance = await StateProvider.CreateInstanceAsync(
            instanceId: GenerateInstanceId(out var id, out var subid),
            stateType: typeof(E2ETestsState),
            state: E2ETestsState.CreateInitialState());

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/E2ETests/{id}/{subid}/Delete?ffiid={instance.InstanceId.UniqueKey}")
        {
        };

        // Act & Assert
        var response = await HttpClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static JourneyInstanceId GenerateInstanceId(out string id, out string subid)
    {
        id = Guid.NewGuid().ToString();
        subid = Guid.NewGuid().ToString();
        var uniqueKey = Guid.NewGuid().ToString();

        return new JourneyInstanceId(
            journeyName: "E2ETests",
            new Dictionary<string, StringValues>()
            {
                { "id", id },
                { "subid", subid },
                { "ffiid", uniqueKey }
            });
    }

    private async Task<JsonNode> ReadStateAndAssertAsync(
        JourneyInstanceId instanceId,
        string expectedValue)
    {
        var id = instanceId.Keys["id"];
        var subid = instanceId.Keys["subid"];

        var response = await HttpClient.GetAsync(
            $"/E2ETests/{id}/{subid}/ReadState?ffiid={instanceId.UniqueKey}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseObj = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        Assert.Equal(expectedValue, responseObj["state"]?["value"]?.ToString());

        return responseObj;
    }

    private async Task UpdateStateAsync(JourneyInstanceId instanceId, string newValue)
    {
        var id = instanceId.Keys["id"];
        var subid = instanceId.Keys["subid"];

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/E2ETests/{id}/{subid}/UpdateState?ffiid={instanceId.UniqueKey}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "newValue", newValue }
            })
        };

        var response = await HttpClient.SendAsync(request);

        if ((int)response.StatusCode >= 400)
        {
            response.EnsureSuccessStatusCode();
        }
    }
}

[Route("E2ETests/{id}/{subid}")]
[Journey("E2ETests")]
public class E2ETestsController : Controller
{
    private readonly JourneyInstanceProvider _journeyInstanceProvider;
    private JourneyInstance<E2ETestsState>? _journeyInstance;

    public E2ETestsController(JourneyInstanceProvider journeyInstanceProvider)
    {
        _journeyInstanceProvider = journeyInstanceProvider;
    }

    [HttpGet("ReadState")]
    [RequireJourneyInstance]
    public IActionResult ReadState()
    {
        return Json(new
        {
            State = _journeyInstance!.State
        });
    }

    [HttpPost("UpdateState")]
    [RequireJourneyInstance]
    public async Task<IActionResult> UpdateState(string newValue, string id, string subid)
    {
        await _journeyInstance!.UpdateStateAsync(state => state.Value = newValue);
        return RedirectToAction(nameof(ReadState), new { id, subid })
            .WithJourneyInstanceUniqueKey(_journeyInstance);
    }

    [HttpPost("Complete")]
    [RequireJourneyInstance]
    public async Task<IActionResult> Complete()
    {
        await _journeyInstance!.CompleteAsync();
        return Ok();
    }

    [HttpPost("Delete")]
    [RequireJourneyInstance]
    public async Task<IActionResult> Delete()
    {
        await _journeyInstance!.DeleteAsync();
        return Ok();
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _journeyInstance = await _journeyInstanceProvider.GetOrCreateInstanceAsync(
            context,
            _ => E2ETestsState.CreateInitialState());

        if (!_journeyInstanceProvider.IsCurrentInstance(context, _journeyInstance))
        {
            context.Result = RedirectToAction()
                .WithJourneyInstanceUniqueKey(_journeyInstance);
            return;
        }

        await base.OnActionExecutionAsync(context, next);
    }
}

public class E2ETestsState
{
    public string? Value { get; set; }

    public static E2ETestsState CreateInitialState() => new E2ETestsState()
    {
        Value = "initial"
    };
}

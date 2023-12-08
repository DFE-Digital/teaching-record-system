#nullable disable
using System.Net;
using TeachingRecordSystem.Api.Tests.Attributes;

namespace TeachingRecordSystem.Api.Tests.V2.Operations;

public class UnlockTeacherTests : ApiTestBase
{
    public UnlockTeacherTests(ApiFixture apiFixture) : base(apiFixture)
    {
        SetCurrentApiClient(new[] { ApiRoles.UnlockPerson });
    }

    [Theory, RoleNamesData(except: new[] { ApiRoles.UnlockPerson })]
    public async Task UnlockTeacher_ClientDoesNotHaveSecurityRoles_ReturnsForbidden(string[] roles)
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        SetCurrentApiClient(roles);

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
            .ReturnsAsync(new Contact()
            {
                Id = teacherId,
                dfeta_ActiveSanctions = false,
                dfeta_loginfailedcounter = 3
            });

        DataverseAdapterMock
            .Setup(mock => mock.UnlockTeacherRecord(teacherId))
            .ReturnsAsync(true)
            .Verifiable();

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_a_teacher_that_does_not_exist_returns_notfound()
    {
        // Arrange
        var teacherId = Guid.NewGuid();

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
            .ReturnsAsync((Contact)null);

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Given_a_teacher_that_does_exist_and_is_locked_returns_ok()
    {
        // Arrange
        var teacherId = Guid.NewGuid();

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
            .ReturnsAsync(new Contact()
            {
                Id = teacherId,
                dfeta_ActiveSanctions = false,
                dfeta_loginfailedcounter = 3
            });

        DataverseAdapterMock
            .Setup(mock => mock.UnlockTeacherRecord(teacherId))
            .ReturnsAsync(true)
            .Verifiable();

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                hasBeenUnlocked = true
            });
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Given_a_teacher_that_does_exist_but_is_not_locked_returns_ok(int? loginFailedCounter)
    {
        // Arrange
        var teacherId = Guid.NewGuid();

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), /* columnNames: */ It.IsAny<bool>()))
            .ReturnsAsync(new Contact()
            {
                Id = teacherId,
                dfeta_ActiveSanctions = false,
                dfeta_loginfailedcounter = loginFailedCounter
            });

        DataverseAdapterMock
            .Setup(mock => mock.UnlockTeacherRecord(teacherId))
            .ReturnsAsync(true)
            .Verifiable();

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            new
            {
                hasBeenUnlocked = false
            });
    }

    [Fact]
    public async Task Given_a_teacher_that_has_activesanctions_returns_error()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var teacher = new Contact() { dfeta_ActiveSanctions = true };

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(teacherId, It.IsAny<string[]>(), It.IsAny<bool>()))
            .ReturnsAsync(teacher);

        var request = new HttpRequestMessage(HttpMethod.Put, $"v2/unlock-teacher/{teacherId}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsError(response, expectedErrorCode: 10014, expectedStatusCode: StatusCodes.Status400BadRequest);
    }
}
